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
        private static info currentInfoFormOtl = null; // Текущая форма info
        private int userId;
        private vkladki vkladkiForm; // Форма навигации
        private DataTable wishlistData; // Данные для DataGridView
        private Dictionary<int, Image> imageCache = new(); // Кэш изображений

        public otl(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);
            wishlistData = new DataTable();
            _ = LoadDataAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    MessageBox.Show("Ошибка при загрузке данных.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        // Метод для создания подключения к базе данных
        private NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection("Host=localhost;Database=coffee_db;Username=postgres;Password=pwd");
        }

        // Метод загрузки данных из базы данных
        private async Task LoadDataAsync()
        {
            string query = @"
                SELECT 
                    w.assortment_id AS id,
                    a.name,
                    a.price,
                    a.photo
                FROM 
                    Wishlist w
                JOIN 
                    Assortment a ON w.assortment_id = a.id
                WHERE 
                    w.user_id = @userId";
            using (var conn = CreateConnection())
            using (var adapter = new NpgsqlDataAdapter(query, conn))
            {
                adapter.SelectCommand.Parameters.AddWithValue("@userId", userId);
                var dataTable = new DataTable();
                await Task.Run(() => adapter.Fill(dataTable)); // Асинхронная загрузка
                wishlistData.Clear();
                wishlistData.Merge(dataTable);
                label2.Text = $"{wishlistData.Rows.Count}";
                SetupDataGridView();
            }
        }

        // Настройка DataGridView
        private void SetupDataGridView()
        {
            dataGridView1.DataSource = wishlistData;
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
                if (IsItemInWishlist(userId, assortmentId))
                {
                    RemoveFromWishlist(userId, assortmentId);
                    var rowToDelete = wishlistData.AsEnumerable().FirstOrDefault(row => (int)row["id"] == assortmentId);
                    if (rowToDelete != null)
                    {
                        wishlistData.Rows.Remove(rowToDelete);
                    }
                }
                else
                {
                    AddToWishlist(userId, assortmentId);
                }
            }
        }

        private Image ResizeImage(Image image, int width, int height)
        {
            float ratioX = (float)width / image.Width;
            float ratioY = (float)height / image.Height;
            float ratio = Math.Min(ratioX, ratioY);
            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);

            using (var resizedImage = new Bitmap(newWidth, newHeight))
            {
                using (Graphics g = Graphics.FromImage(resizedImage))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(image, 0, 0, newWidth, newHeight);
                }
                return new Bitmap(resizedImage); // Создаем новый объект для возврата
            }
        }

        private Image GetWishlistImage(int assortmentId)
        {
            string imagesFolderPath = Path.Combine(Application.StartupPath, "Resources", "значки");
            string imagePath = IsItemInWishlist(userId, assortmentId)
                ? Path.Combine(imagesFolderPath, "сердце.png")
                : Path.Combine(imagesFolderPath, "пуст_сердце.png");
            return Image.FromFile(imagePath);
        }

        private bool IsItemInWishlist(int userId, int assortmentId)
        {
            string query = "SELECT COUNT(*) FROM Wishlist WHERE user_id = @userId AND assortment_id = @assortmentId";
            return ExecuteScalarQuery(query, new NpgsqlParameter("@userId", userId), new NpgsqlParameter("@assortmentId", assortmentId)) > 0;
        }

        private void AddToWishlist(int userId, int assortmentId)
        {
            string query = "INSERT INTO Wishlist (user_id, assortment_id) VALUES (@userId, @assortmentId)";
            ExecuteNonQuery(query, new NpgsqlParameter("@userId", userId), new NpgsqlParameter("@assortmentId", assortmentId));
            UpdateWishlistCount(userId);
        }

        private void RemoveFromWishlist(int userId, int assortmentId)
        {
            string query = "DELETE FROM Wishlist WHERE user_id = @userId AND assortment_id = @assortmentId";
            ExecuteNonQuery(query, new NpgsqlParameter("@userId", userId), new NpgsqlParameter("@assortmentId", assortmentId));
            UpdateWishlistCount(userId);
        }

        private void UpdateWishlistCount(int userId)
        {
            string query = "SELECT COUNT(*) FROM Wishlist WHERE user_id = @userId";
            int itemCount = ExecuteScalarQuery(query, new NpgsqlParameter("@userId", userId));
            label2.Text = $"{itemCount}";
        }

        private void ExecuteNonQuery(string query, params NpgsqlParameter[] parameters)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выполнении запроса: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            Console.WriteLine($"Ошибка в строке {e.RowIndex}, колонке {e.ColumnIndex}: {e.Exception.Message}");
            e.ThrowException = false;
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && (currentInfoFormOtl == null || currentInfoFormOtl.IsDisposed))
            {
                int assortmentId = (int)dataGridView1.Rows[e.RowIndex].Cells["id"].Value;
                currentInfoFormOtl = new info(assortmentId, userId, 2, this);
                currentInfoFormOtl.Show();
                currentInfoFormOtl.FormClosing += async (s, args) =>
                {
                    await UpdateDataGridViewAsync();
                    UpdateWishlistCount(userId);
                    currentInfoFormOtl = null;
                };
            }
            else if (currentInfoFormOtl != null && !currentInfoFormOtl.IsDisposed)
            {
                currentInfoFormOtl.Focus(); // Переводим фокус на уже открытую форму
            }
        }

        private async Task UpdateDataGridViewAsync()
        {
            ClearImageCache(); // Очищаем кэш перед обновлением данных
            var currentIds = wishlistData.AsEnumerable().Select(row => row.Field<int>("id")).ToHashSet();
            string query = @"
        SELECT 
            w.assortment_id AS id,
            a.name,
            a.price,
            a.photo
        FROM 
            Wishlist w
        JOIN 
            Assortment a ON w.assortment_id = a.id
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
                    var rowToDelete = wishlistData.AsEnumerable().FirstOrDefault(r => r.Field<int>("id") == id);
                    if (rowToDelete != null)
                    {
                        wishlistData.Rows.Remove(rowToDelete);
                    }
                }
                wishlistData.AcceptChanges();
                dataGridView1.Invalidate();
            }
        }

        private void LoadImageForRow(DataGridViewRow row, int assortmentId, byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return;

            Image img;
            if (imageCache.TryGetValue(assortmentId, out img))
            {
                row.Cells["photo"].Value = img;
                return;
            }

            using (var ms = new MemoryStream(imageData))
            {
                try
                {
                    img = Image.FromStream(ms);
                }
                catch (ArgumentException)
                {
                    img = null; // Или используйте запасное изображение
                }
                img = ResizeImage(img, dataGridView1.Columns["photo"].Width - 10, dataGridView1.RowTemplate.Height - 10);
                imageCache[assortmentId] = img;
                row.Cells["photo"].Value = img;
            }
        }

        private async Task LoadProductImagesAsync()
        {
            var imageTasks = dataGridView1.Rows.Cast<DataGridViewRow>()
                .Where(row => row.Cells["photo"].Value is byte[])
                .Select(row =>
                {
                    int assortmentId = (int)row.Cells["id"].Value;
                    byte[] imageData = (byte[])row.Cells["photo"].Value;
                    return Task.Run(() => LoadImageForRow(row, assortmentId, imageData));
                }).ToArray();
            await Task.WhenAll(imageTasks);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Открываем форму vkladki
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
            if (currentInfoFormOtl != null && !currentInfoFormOtl.IsDisposed)
            {
                currentInfoFormOtl.Close();
                currentInfoFormOtl = null;
            }
        }
        private void otl_FormClosing(object sender, FormClosingEventArgs e)
        {
            ClearImageCache();
        }
        private void ClearImageCache()
        {
            foreach (var image in imageCache.Values)
            {
                image?.Dispose(); // Освобождаем ресурсы изображения
            }
            imageCache.Clear(); // Очищаем словарь
        }
    }
}