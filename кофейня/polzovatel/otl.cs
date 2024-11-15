using System.Data;
using System.Drawing.Drawing2D;
using Npgsql;

namespace кофейня.polzovatel
{
    public partial class otl : Form
    {
        private int userId;
        private int userRoleId=2;
        private DataTable assortmentData;
        private vkladki vkladkiForm;
        private Dictionary<int, Image> imageCache = new Dictionary<int, Image>(); // Кэш изображений

        public otl(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);

            assortmentData = new DataTable();
            LoadDataAsync();
        }

        // Метод для создания подключения к базе данных
        private NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection("Host=172.20.7.6;Database=krezhowa_coffee;Username=st;Password=pwd");
        }

        // Метод загрузки данных из базы данных
        private async Task LoadDataAsync()
        {
            string query = @"
            SELECT 
                wi.assortment_id AS id,
                a.name,
                a.price,
                a.photo
            FROM 
                Wishlist_Items wi
            JOIN         
                Wishlist w ON wi.wishlist_id = w.id
            JOIN         
                Assortment a ON wi.assortment_id = a.id
            WHERE 
                w.user_id = @userId";

            using (var conn = CreateConnection())
            using (var adapter = new NpgsqlDataAdapter(query, conn))
            {
                adapter.SelectCommand.Parameters.AddWithValue("@userId", userId);
                var dataTable = new DataTable();

                // Асинхронная загрузка данных в фоновом потоке
                await Task.Run(() =>
                {
                    adapter.Fill(dataTable);
                });

                assortmentData.Clear();
                assortmentData.Merge(dataTable);

                label2.Text = $"{assortmentData.Rows.Count}";
                SetupDataGridView();
            }
        }

        // Настройка DataGridView для отображения данных
        private void SetupDataGridView()
        {
            dataGridView1.DataSource = assortmentData;

            dataGridView1.Columns["name"].Width = 180;
            dataGridView1.Columns["price"].Width = 120;
            dataGridView1.Columns["photo"].Width = 200;
            dataGridView1.RowTemplate.Height = 200;

            dataGridView1.Columns["photo"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["photo"].DefaultCellStyle.Padding = new Padding(5);
            dataGridView1.Columns["id"].Visible = false;

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

            LoadProductImagesAsync();  // Загрузка изображений
        }

        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (e.ColumnIndex == dataGridView1.Columns["price"].Index && e.Value != null)
                {
                    if (decimal.TryParse(e.Value.ToString(), out decimal price))
                    {
                        e.Value = $"от {Math.Truncate(price)} ₽";
                        e.FormattingApplied = true;
                    }
                }

                if (e.ColumnIndex == dataGridView1.Columns["addToWishlist"].Index)
                {
                    int assortmentId = (int)dataGridView1.Rows[e.RowIndex].Cells["id"].Value;
                    e.Value = GetWishlistImage(assortmentId); // Загружаем изображение при необходимости
                }
            }
        }

        // Обработчик клика по кнопке в DataGridView для добавления/удаления товара из списка желаемых
        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dataGridView1.Columns["addToWishlist"].Index)
            {
                int assortmentId = (int)dataGridView1.Rows[e.RowIndex].Cells["id"].Value;
                int wishlistId = GetOrCreateWishlistId();

                if (IsItemInWishlist(wishlistId, assortmentId))
                {
                    RemoveFromWishlist(wishlistId, assortmentId);
                    dataGridView1.Rows.RemoveAt(e.RowIndex);  // Удаляем строку без перезагрузки
                }
                else
                {
                    AddToWishlist(wishlistId, assortmentId);
                }
            }
        }

        // Синхронное выполнение SQL-запроса с возвратом одного значения
        private int ExecuteScalarQuery(string query, params NpgsqlParameter[] parameters)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddRange(parameters);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        // Загрузка изображений продуктов в DataGridView
        private async Task LoadProductImagesAsync()
        {
            var imageTasks = new List<Task>();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["photo"].Value is byte[] imageData && imageData.Length > 0)
                {
                    int assortmentId = (int)row.Cells["id"].Value;
                    imageTasks.Add(Task.Run(() =>
                    {
                        LoadImageForRow(row, assortmentId, imageData);
                    }));
                }
            }

            await Task.WhenAll(imageTasks); // Ждем завершения всех задач
        }

        private void LoadImageForRow(DataGridViewRow row, int assortmentId, byte[] imageData)
        {
            Image img;
            // Проверка кэша изображений
            if (imageCache.ContainsKey(assortmentId))
            {
                img = imageCache[assortmentId];
            }
            else
            {
                using (var ms = new MemoryStream(imageData))
                {
                    img = Image.FromStream(ms);
                    imageCache[assortmentId] = img;
                }
            }
            row.Cells["photo"].Value = ResizeImage(img, dataGridView1.Columns["photo"].Width - 10, dataGridView1.RowTemplate.Height - 10);
        }

        // Изменение размера изображения
        private Image ResizeImage(Image image, int width, int height)
        {
            Bitmap resizedImage = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(resizedImage))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, 0, 0, width, height);
            }
            return resizedImage;
        }

        // Получение изображения для кнопки добавления в список желаемых
        private Image GetWishlistImage(int assortmentId)
        {
            string imagesFolderPath = Path.Combine(Application.StartupPath, "Resources", "значки");
            string imagePath = IsItemInWishlist(GetOrCreateWishlistId(), assortmentId)
                ? Path.Combine(imagesFolderPath, "сердце.png")
                : Path.Combine(imagesFolderPath, "пуст_сердце.png");

            return Image.FromFile(imagePath);
        }
        // Добавление товара в список желаемого
        private void AddToWishlist(int wishlistId, int assortmentId)
        {
            string query = "INSERT INTO Wishlist_Items (wishlist_id, assortment_id) VALUES (@wishlistId, @assortmentId)";
            ExecuteNonQuery(query, new NpgsqlParameter("@wishlistId", wishlistId), new NpgsqlParameter("@assortmentId", assortmentId));
            UpdateWishlistCount(wishlistId); // Обновляем количество товаров в списке желаемого
        }

        // Получение или создание списка желаемого для пользователя
        private int GetOrCreateWishlistId()
        {
            int wishlistId = ExecuteScalarQuery("SELECT id FROM Wishlist WHERE user_id = @userId", new NpgsqlParameter("@userId", userId));
            if (wishlistId == 0)
            {
                // Если список не найден, создаем новый
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("INSERT INTO Wishlist (user_id) VALUES (@userId) RETURNING id", conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        wishlistId = (int)cmd.ExecuteScalar();
                    }
                }
            }
            return wishlistId;
        }

        // Проверка, находится ли товар в списке желаемого
        private bool IsItemInWishlist(int wishlistId, int assortmentId)
        {
            string query = "SELECT COUNT(*) FROM Wishlist_Items WHERE wishlist_id = @wishlistId AND assortment_id = @assortmentId";
            return ExecuteScalarQuery(query, new NpgsqlParameter("@wishlistId", wishlistId), new NpgsqlParameter("@assortmentId", assortmentId)) > 0;
        }

        // Удаление товара из списка желаемого
        private void RemoveFromWishlist(int wishlistId, int assortmentId)
        {
            string query = "DELETE FROM Wishlist_Items WHERE wishlist_id = @wishlistId AND assortment_id = @assortmentId";
            ExecuteNonQuery(query, new NpgsqlParameter("@wishlistId", wishlistId), new NpgsqlParameter("@assortmentId", assortmentId));
            UpdateWishlistCount(wishlistId); // Обновляем количество товаров в списке желаемого
        }
        private void ExecuteNonQuery(string query, params NpgsqlParameter[] parameters)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddRange(parameters);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        private void UpdateWishlistCount(int wishlistId)
        {
            string query = "SELECT COUNT(*) FROM Wishlist_Items WHERE wishlist_id = @wishlistId";
            int itemCount = ExecuteScalarQuery(query, new NpgsqlParameter("@wishlistId", wishlistId));

            label2.Text = $"{itemCount}"; // Обновляем label2 с новым количеством товаров
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (vkladkiForm == null || vkladkiForm.IsDisposed)
            {
                vkladkiForm = new vkladki(userId, this);
                vkladkiForm.Show();
            }
            else
            {
                ToggleFormVisibility(vkladkiForm);
            }
        }
        private void ToggleFormVisibility(Form form)
        {
            form.Visible = !form.Visible;
            if (form.Visible) form.Focus();
        }
        private void DataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show($"Ошибка в столбце {e.ColumnIndex} и строке {e.RowIndex}: {e.Exception.Message}");
            e.ThrowException = false; // Отключает выброс исключения и позволяет продолжить работу
        }
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int assortmentId = (int)dataGridView1.Rows[e.RowIndex].Cells["id"].Value;
                OpenProductInfoForm(assortmentId);
            }
        }

        // Открытие формы с информацией о продукте
        private void OpenProductInfoForm(int assortmentId)
        {
            var productInfoForm = new info(assortmentId, userId, userRoleId, this);
            productInfoForm.FormClosing += (s, args) => UpdateProductInGrid(assortmentId);
            productInfoForm.Show();
        }
        // Обновление продукта в сетке
        private void UpdateProductInGrid(int assortmentId)
        {
            var row = dataGridView1.Rows.Cast<DataGridViewRow>()
                .FirstOrDefault(r => (int)r.Cells["id"].Value == assortmentId);

            if (row != null)
            {
                row.Cells["addToWishlist"].Value = GetWishlistImage(assortmentId); // Обновить только нужную ячейку
                LoadDataAsync();
            }
        }

    }
}
