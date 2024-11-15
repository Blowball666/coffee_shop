using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Npgsql;

namespace кофейня.polzovatel
{
    public partial class korz : Form
    {
        private int userId;
        private vkladki vkladkiForm;
        private DataTable cartData;
        private string connectionString = "Host=172.20.7.6;Database=krezhowa_coffee;Username=st;Password=pwd";

        public korz(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);
            SetupDataGridView();
            LoadCartData();
            UpdateUserPoints();
        }
        private void SetupDataGridView()
        {
            dataGridView1.Columns.Clear();

            // Настройка столбцов
            var checkColumn = new DataGridViewCheckBoxColumn
            {
                Name = "Select",
                HeaderText = "",
                Width = 30
            };
            dataGridView1.Columns.Add(checkColumn);

            dataGridView1.Columns.Add("Name", "Название");
            dataGridView1.Columns["Name"].Width = 140;

            dataGridView1.Columns.Add("Size", "Размер");
            dataGridView1.Columns["Size"].Width = 120;

            dataGridView1.Columns.Add("Syrup", "Сироп");
            dataGridView1.Columns["Syrup"].Width = 160;

            var minusButtonColumn = new DataGridViewImageColumn
            {
                Name = "Minus",
                HeaderText = "",
                Image = Image.FromFile(Path.Combine(Application.StartupPath, "Resources", "кнопки", "минус.png")),
                Width = 45
            };
            dataGridView1.Columns.Add(minusButtonColumn);

            dataGridView1.Columns.Add("Quantity", "Кол-во");
            dataGridView1.Columns["Quantity"].Width = 74;

            var plusButtonColumn = new DataGridViewImageColumn
            {
                Name = "Plus",
                HeaderText = "",
                Image = Image.FromFile(Path.Combine(Application.StartupPath, "Resources", "кнопки", "плюс.png")),
                Width = 45
            };
            dataGridView1.Columns.Add(plusButtonColumn);

            dataGridView1.Columns.Add("Price", "Цена");
            dataGridView1.Columns["Price"].Width = 95;

            var photoColumn = new DataGridViewImageColumn
            {
                Name = "Photo",
                HeaderText = "Фото",
                Width = 200,
                ImageLayout = DataGridViewImageCellLayout.Zoom
            };
            dataGridView1.Columns.Add(photoColumn);

            // Настройка высоты строк и общего стиля
            dataGridView1.RowTemplate.Height = 200;
            dataGridView1.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["Photo"].DefaultCellStyle.Padding = new Padding(5);

            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.White;
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;
            Font robotoFont = new Font("Roboto", 16, FontStyle.Bold);
            dataGridView1.DefaultCellStyle.Font = robotoFont;

            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }
        private Image ResizeImage(Image image, int maxWidth, int maxHeight)
        {
            // Рассчитываем пропорции
            double ratio = Math.Min((double)maxWidth / image.Width, (double)maxHeight / image.Height);
            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);

            // Создаем новое изображение с заданными размерами
            var resizedImage = new Bitmap(newWidth, newHeight);

            using (var g = Graphics.FromImage(resizedImage))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.Clear(Color.White);
                g.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            return resizedImage;
        }
        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Форматирование цены
            if (e.ColumnIndex == dataGridView1.Columns["Price"].Index && e.Value != null)
            {
                if (e.ColumnIndex == dataGridView1.Columns["Price"].Index && e.Value != null)
                {
                    if (decimal.TryParse(e.Value.ToString(), out decimal price))
                    {
                        e.Value = $"{Math.Round(price):0} ₽";
                        e.FormattingApplied = true;
                    }
                }
            }

            // Масштабирование изображений для фото
            if (e.ColumnIndex == dataGridView1.Columns["Photo"].Index && e.Value is Image img)
            {
                e.Value = ResizeImage(img, dataGridView1.Columns["Photo"].Width, dataGridView1.RowTemplate.Height);
                e.FormattingApplied = true;
            }
        }
        private void LoadCartData()
        {
            // Загрузка данных корзины из базы данных
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT 
                        dp.id,
                        a.name AS Name,
                        s.name AS Size,
                        sy.name AS Syrup,
                        dp.quantity,
                        dp.price,
                        a.photo
                    FROM DesiredProduct dp
                    JOIN Carts c ON dp.cart_id = c.id
                    JOIN Assortment a ON dp.assortment_id = a.id
                    LEFT JOIN Sizes s ON dp.size_id = s.id
                    LEFT JOIN Syrups sy ON dp.syrup_id = sy.id
                    WHERE c.user_id = @userId";
                using (var command = new NpgsqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("userId", userId);
                    var adapter = new NpgsqlDataAdapter(command);
                    cartData = new DataTable();
                    adapter.Fill(cartData);
                    UpdateGrid();
                }
            }
        }
        private void UpdateGrid()
        {
            dataGridView1.Rows.Clear();
            foreach (DataRow row in cartData.Rows)
            {
                var photo = row["photo"] != DBNull.Value
                    ? Image.FromStream(new MemoryStream((byte[])row["photo"]))
                    : null;

                dataGridView1.Rows.Add(
                    false,
                    row["Name"],
                    row["Size"],
                    row["Syrup"],
                    null, // Minus Button
                    row["quantity"],
                    null, // Plus Button
                    $"{Convert.ToInt32(row["price"])} ₽",
                    photo
                );
            }
        }
        private void UpdateUserPoints()
        {
            // Получение количества баллов пользователя
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT points FROM Users WHERE id = @userId";
                using (var command = new NpgsqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("userId", userId);
                    int points = (int)command.ExecuteScalar();
                    label9.Text = $"У вас {points} баллов";
                }
            }
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["Minus"].Index && e.RowIndex >= 0)
            {
                // Уменьшение количества
                ChangeQuantity(e.RowIndex, -1);
            }
            else if (e.ColumnIndex == dataGridView1.Columns["Plus"].Index && e.RowIndex >= 0)
            {
                // Увеличение количества
                ChangeQuantity(e.RowIndex, 1);
            }
        }
        private void ChangeQuantity(int rowIndex, int delta)
        {
            int quantity = Convert.ToInt32(dataGridView1.Rows[rowIndex].Cells["Quantity"].Value);
            int productId = (int)cartData.Rows[rowIndex]["id"];

            quantity = Math.Max(1, quantity + delta); // Минимум 1
            dataGridView1.Rows[rowIndex].Cells["Quantity"].Value = quantity;

            // Обновление в базе данных
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE DesiredProduct SET quantity = @quantity WHERE id = @id";
                using (var command = new NpgsqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("quantity", quantity);
                    command.Parameters.AddWithValue("id", productId);
                    command.ExecuteNonQuery();
                }
            }

            // Обновление цены
            UpdateGrid();
        }
        private void button1_Click(object sender, EventArgs e)
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

    }
}
