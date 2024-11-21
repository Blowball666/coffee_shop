using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;

namespace кофейня.polzovatel
{
    public partial class otl : Form
    {
        private int userId;
        private int userRoleId = 2;
        private DataTable assortmentData;
        private vkladki vkladkiForm;
        private Dictionary<int, Image> imageCache = new(); // Кэш изображений

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

                await Task.Run(() => adapter.Fill(dataTable)); // Асинхронная загрузка

                assortmentData.Clear();
                assortmentData.Merge(dataTable);

                label2.Text = $"{assortmentData.Rows.Count}";
                SetupDataGridView();
            }
        }
        // Настройка DataGridView
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
            LoadProductImagesAsync(); // Загрузка изображений
        }
        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dataGridView1.Rows.Count || dataGridView1.Rows[e.RowIndex].Cells["id"].Value == null)
                return;

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
                e.Value = GetWishlistImage(assortmentId);
            }
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
                    // Удаляем строку из DataTable
                    var rowToDelete = assortmentData.AsEnumerable().FirstOrDefault(row => (int)row["id"] == assortmentId);
                    if (rowToDelete != null)
                    {
                        assortmentData.Rows.Remove(rowToDelete);
                    }
                }
                else
                {
                    AddToWishlist(wishlistId, assortmentId);
                }
            }
        }
        private Image ResizeImage(Image image, int width, int height)
        {
            float ratioX = (float)width / image.Width;
            float ratioY = (float)height / image.Height;
            float ratio = Math.Min(ratioX, ratioY); // Выбираем наименьшее соотношение для пропорционального масштабирования

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
            string imagesFolderPath = Path.Combine(Application.StartupPath, "Resources", "значки");
            string imagePath = IsItemInWishlist(GetOrCreateWishlistId(), assortmentId)
                ? Path.Combine(imagesFolderPath, "сердце.png")
                : Path.Combine(imagesFolderPath, "пуст_сердце.png");

            return Image.FromFile(imagePath);
        }
        private int GetOrCreateWishlistId()
        {
            int wishlistId = ExecuteScalarQuery("SELECT id FROM Wishlist WHERE user_id = @userId", new NpgsqlParameter("@userId", userId));
            if (wishlistId == 0)
            {
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
        private bool IsItemInWishlist(int wishlistId, int assortmentId)
        {
            string query = "SELECT COUNT(*) FROM Wishlist_Items WHERE wishlist_id = @wishlistId AND assortment_id = @assortmentId";
            return ExecuteScalarQuery(query, new NpgsqlParameter("@wishlistId", wishlistId), new NpgsqlParameter("@assortmentId", assortmentId)) > 0;
        }
        private void AddToWishlist(int wishlistId, int assortmentId)
        {
            string query = "INSERT INTO Wishlist_Items (wishlist_id, assortment_id) VALUES (@wishlistId, @assortmentId)";
            ExecuteNonQuery(query, new NpgsqlParameter("@wishlistId", wishlistId), new NpgsqlParameter("@assortmentId", assortmentId));
            UpdateWishlistCount(wishlistId);
        }
        private void RemoveFromWishlist(int wishlistId, int assortmentId)
        {
            string query = "DELETE FROM Wishlist_Items WHERE wishlist_id = @wishlistId AND assortment_id = @assortmentId";
            ExecuteNonQuery(query, new NpgsqlParameter("@wishlistId", wishlistId), new NpgsqlParameter("@assortmentId", assortmentId));
            UpdateWishlistCount(wishlistId);
        }
        private void UpdateWishlistCount(int wishlistId)
        {
            string query = "SELECT COUNT(*) FROM Wishlist_Items WHERE wishlist_id = @wishlistId";
            int itemCount = ExecuteScalarQuery(query, new NpgsqlParameter("@wishlistId", wishlistId));

            label2.Text = $"{itemCount}";
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
        private int ExecuteScalarQuery(string query, params NpgsqlParameter[] parameters)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddRange(parameters);
                    var result = cmd.ExecuteScalar();
                    return result == DBNull.Value ? 0 : Convert.ToInt32(result);
                }
            }
        }
        private void DataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // Логирование ошибки
            Console.WriteLine($"Ошибка в строке {e.RowIndex}, колонке {e.ColumnIndex}: {e.Exception.Message}");
            e.ThrowException = false; // Подавить исключение
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
            productInfoForm.FormClosing += async (s, args) =>
            {
                await UpdateDataGridViewAsync();
                UpdateWishlistCount(GetOrCreateWishlistId());
            };
            productInfoForm.Show();
        }
        private async Task UpdateDataGridViewAsync()
        {
            var currentIds = assortmentData.AsEnumerable().Select(row => row.Field<int>("id")).ToHashSet();
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
                var newData = new DataTable();
                await Task.Run(() => adapter.Fill(newData));

                var idsToRemove = currentIds.Except(newData.AsEnumerable().Select(row => row.Field<int>("id"))).ToList();

                foreach (var id in idsToRemove)
                {
                    var rowToDelete = assortmentData.AsEnumerable().FirstOrDefault(r => r.Field<int>("id") == id);
                    if (rowToDelete != null)
                    {
                        assortmentData.Rows.Remove(rowToDelete);
                    }
                }

                assortmentData.AcceptChanges();
                // Обновление только видимых строк
                dataGridView1.Invalidate();
            }
        }
        private void LoadImageForRow(DataGridViewRow row, int assortmentId, byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return;

            Image img;
            // Проверяем наличие изображения в кеше
            if (imageCache.TryGetValue(assortmentId, out img))
            {
                row.Cells["photo"].Value = img;
                return; // Если изображение найдено в кеше, сразу выводим его
            }

            // Если изображения нет в кеше, загружаем и сохраняем в кэш
            using (var ms = new MemoryStream(imageData))
            {
                img = Image.FromStream(ms);
                img = ResizeImage(img, dataGridView1.Columns["photo"].Width - 10, dataGridView1.RowTemplate.Height - 10);
                imageCache[assortmentId] = img; // Сохраняем в кэш
                row.Cells["photo"].Value = img;
            }
        }
        private async Task LoadProductImagesAsync()
        {
            // Создаем массив задач для асинхронной загрузки изображений
            var imageTasks = dataGridView1.Rows.Cast<DataGridViewRow>()
                .Where(row => row.Cells["photo"].Value is byte[])
                .Select(row =>
                {
                    int assortmentId = (int)row.Cells["id"].Value;
                    byte[] imageData = (byte[])row.Cells["photo"].Value;
                    return Task.Run(() => LoadImageForRow(row, assortmentId, imageData)); // Асинхронная загрузка
                }).ToArray();

            // Ожидаем завершения всех задач
            await Task.WhenAll(imageTasks);
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

    }
}
