using System.Data;
using System.Drawing.Drawing2D;
using Npgsql;
using кофейня.polzovatel;

namespace кофейня
{
    public partial class menu : Form
    {
        private static info currentInfoForm = null;
        private int userId;
        private int userRoleId;
        private DataTable assortmentData;
        private vkladki vkladkiForm;
        private string connectionString = "Host=localhost;Database=coffee_db;Username=postgres;Password=pwd";

        public menu(int userId, int userRoleId)
        {
            InitializeComponent();
            this.userId = userId;
            this.userRoleId = userRoleId;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);
            assortmentData = new DataTable();
            LoadData();
            dataGridView1.Columns["id"].Visible = false;
        }

        private void PromptForRegistration()
        {
            var result = MessageBox.Show(
                "Для дальнейших действий необходимо зарегистрироваться или войти в аккаунт. Хотите продолжить?",
                "Требуется регистрация",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information
            );
            if (result == DialogResult.Yes)
            {
                registr registrForm = new registr();
                registrForm.Show();
                this.Hide();
            }
        }

        private void LoadData()
        {
            if (assortmentData == null)
            {
                assortmentData = new DataTable();
            }
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id, name, price, photo FROM Assortment WHERE in_stock = TRUE";
                var adapter = new NpgsqlDataAdapter(query, conn);
                assortmentData.Clear();
                adapter.Fill(assortmentData);
            }
            SetupDataGridView();
        }

        private void SetupDataGridView()
        {
            dataGridView1.DataSource = assortmentData;
            dataGridView1.Columns["name"].Width = 180;
            dataGridView1.Columns["price"].Width = 120;
            dataGridView1.Columns["photo"].Width = 200;
            dataGridView1.RowTemplate.Height = 200;
            dataGridView1.Columns["photo"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["photo"].DefaultCellStyle.Padding = new Padding(5);

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

            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.White;
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;
            Font robotoFont = new Font("Roboto", 16, FontStyle.Bold);
            dataGridView1.DefaultCellStyle.Font = robotoFont;
            dataGridView1.CellFormatting += DataGridView1_CellFormatting;
            LoadProductImages();
        }

        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["addToWishlist"].Index && e.RowIndex >= 0)
            {
                int assortmentId = (int)dataGridView1.Rows[e.RowIndex].Cells["id"].Value;
                e.Value = GetWishlistImage(assortmentId);
            }

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
            double ratio = Math.Min((double)maxWidth / image.Width, (double)maxHeight / image.Height);
            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);

            var resizedImage = new Bitmap(newWidth, newHeight);
            using (var g = Graphics.FromImage(resizedImage))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.Clear(Color.White);
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
            string query = "SELECT COUNT(*) FROM Wishlist WHERE user_id = @userId AND assortment_id = @assortmentId";
            using (var conn = new NpgsqlConnection(connectionString))
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
            using (var conn = new NpgsqlConnection(connectionString))
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
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddRange(parameters);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выполнении запроса: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            dv.RowFilter = string.Empty;
            dataGridView1.DataSource = dv.ToTable();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (userRoleId == 3) // Проверка, если пользователь - гость
            {
                PromptForRegistration();
                return;
            }

            // Открываем форму vkladki без закрытия формы info
            if (vkladkiForm == null || vkladkiForm.IsDisposed)
            {
                vkladkiForm = new vkladki(userId, this);
                vkladkiForm.Show();
            }
            else
            {
                vkladkiForm.Visible = !vkladkiForm.Visible;
                if (vkladkiForm.Visible)
                {
                    vkladkiForm.Focus();
                }
            }
            if (currentInfoForm != null && !currentInfoForm.IsDisposed)
            {
                currentInfoForm.Close();
                currentInfoForm = null;
            }
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show($"Ошибка в столбце {e.ColumnIndex}, строке {e.RowIndex}");
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dataGridView1.Columns["addToWishlist"].Index)
            {
                if (userRoleId == 3) // Проверка, если пользователь - гость
                {
                    PromptForRegistration();
                    return;
                }

                int assortmentId = (int)dataGridView1.Rows[e.RowIndex].Cells["id"].Value;

                // Проверка, есть ли уже товар в отложенных
                string queryCheck = "SELECT COUNT(*) FROM Wishlist WHERE user_id = @userId AND assortment_id = @assortmentId";
                int count = ExecuteScalarQuery(queryCheck, new NpgsqlParameter("@userId", userId), new NpgsqlParameter("@assortmentId", assortmentId));

                if (count > 0)
                {
                    // Удаляем товар из отложенных
                    ExecuteNonQuery(
                        "DELETE FROM Wishlist WHERE user_id = @userId AND assortment_id = @assortmentId",
                        new NpgsqlParameter("@userId", userId),
                        new NpgsqlParameter("@assortmentId", assortmentId)
                    );
                    ShowNotification("Товар удален из отложенных.");
                }
                else
                {
                    // Добавляем товар в отложенные
                    ExecuteNonQuery(
                        "INSERT INTO Wishlist (user_id, assortment_id) VALUES (@userId, @assortmentId)",
                        new NpgsqlParameter("@userId", userId),
                        new NpgsqlParameter("@assortmentId", assortmentId)
                    );
                }

                // Обновляем иконку в DataGrid
                dataGridView1.Rows[e.RowIndex].Cells["addToWishlist"].Value = GetWishlistImage(assortmentId);
            }
        }

        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && (currentInfoForm == null || currentInfoForm.IsDisposed || !currentInfoForm.Visible))
            {
                int assortmentId = (int)dataGridView1.Rows[e.RowIndex].Cells["id"].Value;

                // Создаем новое окно info без закрытия текущего
                currentInfoForm = new info(assortmentId, userId, userRoleId, this);
                currentInfoForm.Show();


                // Подписываемся на событие закрытия формы info
                currentInfoForm.FormClosing += (s, args) =>
                {
                    UpdateProductInGrid(assortmentId); // Обновляем данные в DataGridView
                    currentInfoForm = null; // Очищаем ссылку на закрытое окно
                };
            }
        }

        private void UpdateProductInGrid(int assortmentId)
        {
            var row = dataGridView1.Rows.Cast<DataGridViewRow>()
                .FirstOrDefault(r => (int)r.Cells["id"].Value == assortmentId);

            if (row != null)
            {
                // Обновляем конкретную строку (например, статус "В отложенные" или фото)
                row.Cells["addToWishlist"].Value = GetWishlistImage(assortmentId);
            }
        }

        
    }
}