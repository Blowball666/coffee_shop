using System.Data;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Npgsql;

namespace кофейня.polzovatel
{
    public partial class otl : Form
    {
        private int userId;
        private DataTable assortmentData;
        private vkladki vkladkiForm;
        private Dictionary<string, Image> imageCache = new Dictionary<string, Image>(); // Image Cache

        public otl(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);

            assortmentData = new DataTable();            
            LoadData();
            dataGridView1.Columns["id"].Visible = false;
        }
        private void LoadData()
        {
            using (var conn = new NpgsqlConnection("Host=172.20.7.6;Database=krezhowa_coffee;Username=st;Password=pwd"))
            {
                conn.Open();
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

                var adapter = new NpgsqlDataAdapter(query, conn);
                adapter.SelectCommand.Parameters.AddWithValue("@userId", userId);

                assortmentData.Clear();  // Очищаем таблицу перед загрузкой новых данных
                adapter.Fill(assortmentData);  // Заполняем данными                

                label2.Text = $"{assortmentData.Rows.Count}";  // Обновляем количество                
                SetupDataGridView();
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

            // Настроим стиль для столбца с фото
            dataGridView1.Columns["photo"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["photo"].DefaultCellStyle.Padding = new Padding(5);

            // Добавляем колонку для кнопок добавления в отложенные товары
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

            // Настройки выделения ячеек
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.White;
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;

            // Устанавливаем шрифт
            Font robotoFont = new Font("Roboto", 16, FontStyle.Bold);
            dataGridView1.DefaultCellStyle.Font = robotoFont;

            // Подписываемся на событие форматирования ячеек
            dataGridView1.CellFormatting += DataGridView1_CellFormatting;

            // Загружаем изображения продуктов
            LoadProductImages();
        }
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int assortmentId = (int)dataGridView1.Rows[e.RowIndex].Cells["id"].Value;
                OpenProductInfoForm(assortmentId);
            }
        }
        private void OpenProductInfoForm(int assortmentId)
        {
            var productInfoForm = new info(assortmentId, userId);
            productInfoForm.FormClosing += (s, e) =>
            {
                LoadData();
            };

            productInfoForm.Show();
        }        
        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dataGridView1.Columns["addToWishlist"].Index)
            {
                int assortmentId = (int)dataGridView1.Rows[e.RowIndex].Cells["id"].Value;
                int wishlistId = GetOrCreateWishlistId();

                if (IsItemInWishlist(wishlistId, assortmentId))
                {
                    RemoveFromWishlist(wishlistId, assortmentId);
                }
                else
                {
                    AddToWishlist(wishlistId, assortmentId);
                }

                LoadData();
            }
        }
        private bool IsItemInWishlist(int wishlistId, int assortmentId)
        {
            string query = "SELECT COUNT(*) FROM Wishlist_Items WHERE wishlist_id = @wishlistId AND assortment_id = @assortmentId";
            return ExecuteScalarQuery(query, new NpgsqlParameter("@wishlistId", wishlistId), new NpgsqlParameter("@assortmentId", assortmentId)) > 0;
        }
        private int GetOrCreateWishlistId()
        {
            int wishlistId = ExecuteScalarQuery("SELECT id FROM Wishlist WHERE user_id = @userId", new NpgsqlParameter("@userId", userId));
            if (wishlistId == 0)
            {
                using (var conn = new NpgsqlConnection("Host=172.20.7.6;Database=krezhowa_coffee;Username=st;Password=pwd"))
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
        private void AddToWishlist(int wishlistId, int assortmentId)
        {
            using (var conn = new NpgsqlConnection("Host=172.20.7.6;Database=krezhowa_coffee;Username=st;Password=pwd"))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("INSERT INTO Wishlist_Items (wishlist_id, assortment_id) VALUES (@wishlistId, @assortmentId)", conn))
                {
                    cmd.Parameters.AddWithValue("@wishlistId", wishlistId);
                    cmd.Parameters.AddWithValue("@assortmentId", assortmentId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        private void RemoveFromWishlist(int wishlistId, int assortmentId)
        {
            using (var conn = new NpgsqlConnection("Host=172.20.7.6;Database=krezhowa_coffee;Username=st;Password=pwd"))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM Wishlist_Items WHERE wishlist_id = @wishlistId AND assortment_id = @assortmentId", conn))
                {
                    cmd.Parameters.AddWithValue("@wishlistId", wishlistId);
                    cmd.Parameters.AddWithValue("@assortmentId", assortmentId);
                    cmd.ExecuteNonQuery();
                }
            }

            // После удаления обновляем данные
            LoadData();                        
        }
        private void LoadProductImages()
        {
            // Перезагружаем изображения и пересчитываем их размеры
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["photo"].Value is byte[] imageData && imageData.Length > 0)
                {
                    var imageHash = Convert.ToBase64String(imageData);
                    if (!imageCache.ContainsKey(imageHash))
                    {
                        using (var ms = new MemoryStream(imageData))
                        {
                            using (var img = Image.FromStream(ms))
                            {
                                // Сжимаем изображение по новым размерам
                                imageCache[imageHash] = ResizeImage(img, dataGridView1.Columns["photo"].Width - 10, dataGridView1.RowTemplate.Height - 10);
                            }
                        }
                    }
                    row.Cells["photo"].Value = imageCache[imageHash];
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
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;  // Высококачественная интерполяция
                g.FillRectangle(Brushes.White, 0, 0, newWidth, newHeight); // Фон белый
                g.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            return resizedImage;
        }
        private Image GetWishlistImage(int assortmentId)
        {
            string imagesFolderPath = Path.Combine(Application.StartupPath, "Resources", "значки");
            string filledHeartPath = Path.Combine(imagesFolderPath, "сердце.png");
            string emptyHeartPath = Path.Combine(imagesFolderPath, "пуст_сердце.png");

            int wishlistId = ExecuteScalarQuery("SELECT id FROM Wishlist WHERE user_id = @userId", new NpgsqlParameter("@userId", userId));
            if (wishlistId > 0)
            {
                int itemCount = ExecuteScalarQuery(
                    "SELECT COUNT(*) FROM Wishlist_Items WHERE wishlist_id = @wishlistId AND assortment_id = @assortmentId",
                    new NpgsqlParameter("@wishlistId", wishlistId),
                    new NpgsqlParameter("@assortmentId", assortmentId)
                );
                return itemCount > 0 ? Image.FromFile(filledHeartPath) : Image.FromFile(emptyHeartPath);
            }
            return Image.FromFile(emptyHeartPath);
        }
        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView1.Columns.Contains("addToWishlist") && e.ColumnIndex == dataGridView1.Columns["addToWishlist"].Index && e.RowIndex >= 0)
            {
                int assortmentId = (int)dataGridView1.Rows[e.RowIndex].Cells["id"].Value;
                e.Value = GetWishlistImage(assortmentId); // Отображаем изображение на кнопке
            }
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

    }
}
