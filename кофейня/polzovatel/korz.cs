using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Npgsql;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace кофейня.polzovatel
{
    public partial class korz : Form
    {
        private int userId;
        private vkladki vkladkiForm;
        private DataTable cartData;
        private System.Windows.Forms.ToolTip toolTip1;
        private string connectionString = "Host=172.20.7.6;Database=krezhowa_coffee;Username=st;Password=pwd";

        public korz(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);
            toolTip1 = new ToolTip();
            SetupDataGridView();
            LoadCartData();
            UpdateSelectButtonTooltip();
            UpdateTotalPrice();
            UpdateUserPointsLabel();
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
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["Select"].Index && e.RowIndex >= 0)
            {
                UpdateTotalPrice();
                UpdateSelectButtonImage();
            }
        }
        private void UpdateSelectButtonImage()
        {
            bool allSelected = true;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Select"].Value == null || !(bool)row.Cells["Select"].Value)
                {
                    allSelected = false;
                    break;
                }
            }

            string imagePath = Path.Combine(Application.StartupPath, "Resources", "значки",
                allSelected ? "снять выделение.png" : "выделить все.png");

            button4.Image = Image.FromFile(imagePath);
        }
        private void UpdateSelectButtonTooltip()
        {
            int selectedCount = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Select"].Value != null && (bool)row.Cells["Select"].Value)
                {
                    selectedCount++;
                }
            }

            if (selectedCount == dataGridView1.Rows.Count)
            {
                toolTip1.SetToolTip(button4, $"Снять выделение с {dataGridView1.Rows.Count} товаров");
            }
            else
            {
                toolTip1.SetToolTip(button4, $"Выделить все {dataGridView1.Rows.Count} товаров");
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            bool allSelected = true;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Select"].Value == null || !(bool)row.Cells["Select"].Value)
                {
                    allSelected = false;
                    break;
                }
            }

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.Cells["Select"].Value = !allSelected;
            }

            button4.Image = Image.FromFile(Path.Combine(Application.StartupPath, "Resources", "значки",
                allSelected ? "выделить все.png" : "снять выделение.png"));

            UpdateTotalPrice();
        }
        private void UpdateTotalPrice()
        {
            int totalPrice = 0;
            int totalPoints = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Select"].Value != null && (bool)row.Cells["Select"].Value)
                {
                    totalPrice += Convert.ToInt32(Math.Round(Convert.ToDecimal(row.Cells["Price"].Value.ToString().Replace(" ₽", ""))));
                }
            }

            label3.Text = $"Итоговая стоимость: {totalPrice} ₽";

            if (checkBox1.Checked)
            {
                totalPoints = (int)Math.Floor(totalPrice * 0.3); // Баллы округляем вниз
                label2.Text = $"Спишется {totalPoints} баллов";
            }
            else
            {
                label2.Text = string.Empty;
            }
        }     
        private void UpdateUserPointsLabel()
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT points FROM Users WHERE id = @userId";
                using (var command = new NpgsqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("userId", userId);
                    int userPoints = Convert.ToInt32(command.ExecuteScalar());
                    label9.Text = $"Ваши баллы: {userPoints}";
                }
            }
        }
        private void ChangeQuantity(int rowIndex, int delta)
        {
            int quantity = Convert.ToInt32(dataGridView1.Rows[rowIndex].Cells["Quantity"].Value);
            int productId = (int)cartData.Rows[rowIndex]["id"];
            int newQuantity = Math.Max(1, quantity + delta); // Минимум 1
            dataGridView1.Rows[rowIndex].Cells["Quantity"].Value = newQuantity;

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE DesiredProduct SET quantity = @quantity WHERE id = @id";
                using (var command = new NpgsqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("quantity", newQuantity);
                    command.Parameters.AddWithValue("id", productId);
                    command.ExecuteNonQuery();
                }
            }

            // Перезагрузка данных в таблице
            LoadCartData();
        }
        private void LoadCartData()
        {
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

            // Обновляем подсказку на button4
            UpdateSelectButtonTooltip();
        }
        private void UpdateGrid()
        {
            dataGridView1.Rows.Clear();
            foreach (DataRow row in cartData.Rows)
            {
                var photo = row["photo"] != DBNull.Value
                    ? Image.FromStream(new MemoryStream((byte[])row["photo"]))
                    : null;

                decimal price = Convert.ToDecimal(row["price"]);
                int quantity = Convert.ToInt32(row["quantity"]);
                decimal totalPrice = price * quantity;

                dataGridView1.Rows.Add(
                    false,
                    row["Name"],
                    row["Size"],
                    row["Syrup"],
                    null, // Minus Button
                    quantity,
                    null, // Plus Button
                    $"{(int)Math.Floor(totalPrice)} ₽",
                    //, // Общая стоимость для данного товара
                    photo
                );
            }
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["Select"].Index && e.RowIndex >= 0)
            {
                // Обновляем метки при клике на чекбокс
                UpdateTotalPrice();
                UpdateSelectButtonImage();
            }
            else if (e.ColumnIndex == dataGridView1.Columns["Minus"].Index && e.RowIndex >= 0)
            {
                ChangeQuantity(e.RowIndex, -1);
            }
            else if (e.ColumnIndex == dataGridView1.Columns["Plus"].Index && e.RowIndex >= 0)
            {
                ChangeQuantity(e.RowIndex, 1);
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            List<int> selectedProductIds = new List<int>();
            decimal totalPrice = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Select"].Value != null && (bool)row.Cells["Select"].Value)
                {
                    selectedProductIds.Add((int)cartData.Rows[row.Index]["id"]);
                    totalPrice += Convert.ToDecimal(row.Cells["Price"].Value.ToString().Replace(" ₽", ""));
                }
            }

            if (selectedProductIds.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите хотя бы один товар для оформления.");
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите оформить заказ?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No) return;

            int userPoints = GetUserPoints();
            decimal pointsToRedeem = checkBox1.Checked ? totalPrice * 0.3m : 0;

            if (checkBox1.Checked && userPoints < pointsToRedeem)
            {
                MessageBox.Show("У вас недостаточно баллов для использования этой скидки.");
                return;
            }

            // Удаление выбранных товаров из корзины
            foreach (var productId in selectedProductIds)
            {
                RemoveProductFromCart(productId);
            }

            // Обновление баллов пользователя
            UpdateUserPointsAfterOrder(totalPrice, pointsToRedeem);

            MessageBox.Show("Заказ успешно оформлен!");

            // Перезагружаем данные в корзине
            LoadCartData();
        }
        private void UpdateUserPointsAfterOrder(decimal totalPrice, decimal pointsToRedeem)
        {
            int earnedPoints = (int)Math.Floor(totalPrice * 0.05m); // Начисляем 5% от стоимости
            int pointsToDeduct = (int)Math.Floor(pointsToRedeem); // Списываем целые баллы

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    UPDATE Users 
                    SET points = points - @pointsToDeduct + @earnedPoints 
                    WHERE id = @userId";

                using (var command = new NpgsqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("pointsToDeduct", pointsToDeduct);
                    command.Parameters.AddWithValue("earnedPoints", earnedPoints);
                    command.Parameters.AddWithValue("userId", userId);
                    command.ExecuteNonQuery();
                }
            }

            // Обновляем метку с баллами пользователя
            UpdateUserPointsLabel();
        }
        private int GetUserPoints()
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT points FROM Users WHERE id = @userId";
                using (var command = new NpgsqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("userId", userId);
                    return (int)command.ExecuteScalar();
                }
            }
        }
        private void RemoveProductFromCart(int productId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM DesiredProduct WHERE id = @id";
                using (var command = new NpgsqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("id", productId);
                    command.ExecuteNonQuery();
                }
            }

            // Обновляем количество товаров в подсказке
            UpdateSelectButtonTooltip();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            List<int> productIdsToRemove = new List<int>();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                var checkCell = row.Cells["Select"] as DataGridViewCheckBoxCell;
                if (checkCell != null && checkCell.Value != null && (bool)checkCell.Value)
                {
                    int productId = (int)cartData.Rows[row.Index]["id"];
                    productIdsToRemove.Add(productId);
                }
            }

            if (productIdsToRemove.Count > 0)
            {
                // Подтверждение перед удалением товаров
                var result = MessageBox.Show("Вы уверены, что хотите удалить выбранные товары?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                {
                    return; // Отменить удаление
                }

                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    using (var command = new NpgsqlCommand("DELETE FROM DesiredProduct WHERE id = ANY(@ids)", conn))
                    {
                        command.Parameters.AddWithValue("ids", productIdsToRemove.ToArray());
                        command.ExecuteNonQuery();
                    }
                }

                // Обновляем данные в DataGridView
                LoadCartData();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите хотя бы один товар для удаления.");
            }
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
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            // Получаем текущую стоимость всех товаров в корзине
            decimal totalPrice = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Select"].Value != null && (bool)row.Cells["Select"].Value)
                {
                    totalPrice += Convert.ToDecimal(row.Cells["Price"].Value.ToString().Replace(" ₽", ""));
                }
            }

            // Рассчитываем количество баллов для списания
            int userPoints = GetUserPoints(); // Получаем количество баллов у пользователя
            decimal pointsToRedeem = checkBox1.Checked ? totalPrice * 0.3m : 0;
            pointsToRedeem = Math.Min(pointsToRedeem, userPoints); // Не можем списать больше, чем у пользователя

            label2.Text = $"Спишется {(int)Math.Floor(pointsToRedeem)} баллов";
            //(int)Math.Floor
        }
    }
}
