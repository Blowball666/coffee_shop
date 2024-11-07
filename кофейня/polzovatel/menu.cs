using System.Data;
using System.Drawing.Drawing2D;
using Npgsql;
using кофейня.polzovatel;

namespace кофейня
{
    public partial class menu : Form
    {
        private int userId;
        private DataTable assortmentData;
        private vkladki vkladkiForm;

        public menu(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);
            textBox1.KeyDown += textBox1_KeyDown;
            textBox1.Enter += textBox1_Enter;
            textBox1.Leave += textBox1_Leave;

            assortmentData = new DataTable(); // Инициализация здесь
            LoadData();
            SetupDataGridView();
            dataGridView1.Columns["id"].Visible = false;

        }

        private void LoadData()
        {
            // Убедитесь, что assortmentData инициализирована
            if (assortmentData == null)
            {
                assortmentData = new DataTable();
            }

            using (var conn = new NpgsqlConnection("Host=172.20.7.6;Database=krezhowa_coffee;Username=st;Password=pwd"))
            {
                conn.Open();
                string query = "SELECT id, name, price, photo FROM Assortment WHERE in_stock = TRUE";
                var adapter = new NpgsqlDataAdapter(query, conn);
                assortmentData.Clear();
                adapter.Fill(assortmentData);
            }
        }

        private void SetupDataGridView()
        {
            // Привязываем данные к DataGridView
            dataGridView1.DataSource = assortmentData;

            // Устанавливаем ширину столбцов
            dataGridView1.Columns["name"].Width = 180;
            dataGridView1.Columns["price"].Width = 120;
            dataGridView1.Columns["photo"].Width = 200;
            dataGridView1.RowTemplate.Height = 200;

            // Настраиваем стиль для столбца с фото
            dataGridView1.Columns["photo"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["photo"].DefaultCellStyle.Padding = new Padding(5);

            // Проверяем, существует ли столбец "addToWishlist"
            if (!dataGridView1.Columns.Contains("addToWishlist"))
            {
                var wishlistButtonColumn = new DataGridViewImageColumn
                {
                    Name = "addToWishlist",
                    HeaderText = "",
                    Width = 100
                };
                dataGridView1.Columns.Add(wishlistButtonColumn);
            }

            // Установка стиля выделения
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.White;
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;

            // Установка шрифта
            Font robotoFont = new Font("Roboto", 16, FontStyle.Bold);
            dataGridView1.DefaultCellStyle.Font = robotoFont;

            // Подписываемся на событие форматирования ячеек
            dataGridView1.CellFormatting += DataGridView1_CellFormatting;

            // Загружаем изображения продуктов
            LoadProductImages();
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dataGridView1.Columns["addToWishlist"].Index)
            {
                int assortmentId = (int)dataGridView1.Rows[e.RowIndex].Cells["id"].Value;

                int wishlistId = ExecuteScalarQuery(
                    "SELECT id FROM Wishlist WHERE user_id = @userId",
                    new NpgsqlParameter("@userId", userId)
                );

                if (wishlistId == 0)
                {
                    ExecuteNonQuery("INSERT INTO Wishlist (user_id) VALUES (@userId)", new NpgsqlParameter("@userId", userId));
                    wishlistId = ExecuteScalarQuery("SELECT id FROM Wishlist WHERE user_id = @userId", new NpgsqlParameter("@userId", userId));
                }

                int itemCount = ExecuteScalarQuery(
                    "SELECT COUNT(*) FROM Wishlist_Items WHERE wishlist_id = @wishlistId AND assortment_id = @assortmentId",
                    new NpgsqlParameter("@wishlistId", wishlistId),
                    new NpgsqlParameter("@assortmentId", assortmentId)
                );

                if (itemCount > 0)
                {
                    ExecuteNonQuery(
                        "DELETE FROM Wishlist_Items WHERE wishlist_id = @wishlistId AND assortment_id = @assortmentId",
                        new NpgsqlParameter("@wishlistId", wishlistId),
                        new NpgsqlParameter("@assortmentId", assortmentId)
                    );

                    int remainingItems = ExecuteScalarQuery(
                        "SELECT COUNT(*) FROM Wishlist_Items WHERE wishlist_id = @wishlistId",
                        new NpgsqlParameter("@wishlistId", wishlistId)
                    );

                    if (remainingItems == 0)
                    {
                        ExecuteNonQuery("DELETE FROM Wishlist WHERE id = @wishlistId", new NpgsqlParameter("@wishlistId", wishlistId));
                    }

                    ShowNotification("Товар удален из отложенных.");
                }
                else
                {
                    ExecuteNonQuery(
                        "INSERT INTO Wishlist_Items (wishlist_id, assortment_id) VALUES (@wishlistId, @assortmentId)",
                        new NpgsqlParameter("@wishlistId", wishlistId),
                        new NpgsqlParameter("@assortmentId", assortmentId)
                    );

                    ShowNotification("Товар добавлен в отложенные.");
                }

                dataGridView1.Rows[e.RowIndex].Cells["addToWishlist"].Value = GetWishlistImage(assortmentId);
            }
        }

        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int assortmentId = (int)dataGridView1.Rows[e.RowIndex].Cells["id"].Value;
                var productInfoForm = new info(assortmentId, userId);
                //productInfoForm.FormClosed += ProductInfoForm_FormClosed;
                productInfoForm.Show();
                productInfoForm.FormClosed += (s, args) => LoadData(); // Обновление данных после закрытия формы
                productInfoForm.FormClosed += (s, args) => LoadProductImages();
            }
        }

        /*private void ProductInfoForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            DataView dv = assortmentData.DefaultView;
            dv.RowFilter = string.Empty; dataGridView1.DataSource = dv.ToTable();
            LoadData();
            SetupDataGridView();
        }*/

        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["addToWishlist"].Index && e.RowIndex >= 0)
            {
                int assortmentId = (int)dataGridView1.Rows[e.RowIndex].Cells["id"].Value;
                e.Value = GetWishlistImage(assortmentId);
            }

            // Форматирование столбца цены
            if (e.ColumnIndex == dataGridView1.Columns["price"].Index && e.RowIndex >= 0)
            {
                if (e.Value != null && e.Value is decimal price)
                {
                    e.Value = $"от {Math.Round(price)} ₽";
                    e.FormattingApplied = true;
                }
            }
        }

        private void LoadProductImages()
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                var imageData = row.Cells["photo"].Value as byte[];

                if (imageData != null && imageData.Length > 0)
                {
                    using (var ms = new MemoryStream(imageData))
                    {
                        using (var img = Image.FromStream(ms))
                        {
                            var resizedImage = ResizeImage(img, dataGridView1.Columns["photo"].Width - 10, dataGridView1.RowTemplate.Height - 10);
                            row.Cells["photo"].Value = resizedImage;
                        }
                    }
                }
            }
        }

        private Image ResizeImage(Image image, int maxWidth, int maxHeight)
        {
            double ratioX = (double)maxWidth / image.Width;
            double ratioY = (double)maxHeight / image.Height;
            double ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);

            Bitmap resizedImage = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(resizedImage))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return resizedImage;
        }

        private Image GetWishlistImage(int assortmentId)
        {
            string filledHeartPath = Path.Combine(Application.StartupPath, "Resources", "значки", "сердце.png");
            string emptyHeartPath = Path.Combine(Application.StartupPath, "Resources", "значки", "пуст_сердце.png");

            Image filledHeart = File.Exists(filledHeartPath) ? Image.FromFile(filledHeartPath) : null;
            Image emptyHeart = File.Exists(emptyHeartPath) ? Image.FromFile(emptyHeartPath) : null;

            return CheckIfInWishlist(assortmentId) ? filledHeart : emptyHeart;
        }

        private bool CheckIfInWishlist(int assortmentId)
        {
            string query = "SELECT COUNT(*) FROM Wishlist_Items wi JOIN Wishlist w ON wi.wishlist_id = w.id WHERE w.user_id = @userId AND wi.assortment_id = @assortmentId";

            using (var conn = new NpgsqlConnection("Host=172.20.7.6;Database=krezhowa_coffee;Username=st;Password=pwd"))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@assortmentId", assortmentId);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        private void ShowNotification(string message)
        {
            MessageBox.Show(message, "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private int ExecuteScalarQuery(string query, params NpgsqlParameter[] parameters)
        {
            using (var conn = new NpgsqlConnection("Host=172.20.7.6;Database=krezhowa_coffee;Username=st;Password=pwd"))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddRange(parameters);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        private void ExecuteNonQuery(string query, params NpgsqlParameter[] parameters)
        {
            using (var conn = new NpgsqlConnection("Host=172.20.7.6;Database=krezhowa_coffee;Username=st;Password=pwd"))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddRange(parameters);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                button1.PerformClick();
            }
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (textBox1.Text == "ПОИСК")
            {
                textBox1.Clear();
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                textBox1.Text = "ПОИСК";
            }
        }

        private void PerformSearch()
        {
            string searchText = textBox1.Text.Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                DataView dv = assortmentData.DefaultView;
                dv.RowFilter = $"name LIKE '%{searchText}%'";
                dataGridView1.DataSource = dv.ToTable();
            }
        }

        private void button1_Click(object sender, EventArgs e) => PerformSearch();

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "ПОИСК";
            DataView dv = assortmentData.DefaultView;
            dv.RowFilter = string.Empty; dataGridView1.DataSource = dv.ToTable();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Проверка, открыт ли экземпляр окна навигации
            if (vkladkiForm == null || vkladkiForm.IsDisposed)
            {
                // Если окно не создано или было закрыто, создаём новый экземпляр и показываем его
                vkladkiForm = new vkladki(userId, this);
                vkladkiForm.Show();
            }
            else
            {
                // Если окно уже создано, переключаем видимость
                vkladkiForm.Visible = !vkladkiForm.Visible;

                // Переводим фокус на окно, если оно отображено
                if (vkladkiForm.Visible)
                {
                    vkladkiForm.Focus();
                }
            }
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show($"Ошибка в столбце {e.ColumnIndex} и строке {e.RowIndex}: {e.Exception.Message}");
            e.ThrowException = false; // Отключает выброс исключения и позволяет продолжить работу
        }
    }
}
