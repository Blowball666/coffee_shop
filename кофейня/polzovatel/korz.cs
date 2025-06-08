using System.Data;
using Npgsql;

namespace кофейня.polzovatel
{
    public partial class korz : Form
    {
        private int userId;
        private vkladki vkladkiForm;
        private DataTable cartData;
        private ToolTip toolTip1;
        private string connectionString = "Host=localhost;Database=coffee_db;Username=postgres;Password=pwd";

        public korz(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);
            toolTip1 = new ToolTip();
            SetupDataGridView();
            LoadCartData();
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
            dataGridView1.Columns["Size"].Width = 130;

            dataGridView1.Columns.Add("Syrup", "Сироп");
            dataGridView1.Columns["Syrup"].Width = 200;
            dataGridView1.Columns["Syrup"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;

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
            Font robotoFont = new Font("Roboto", 15, FontStyle.Bold);
            dataGridView1.DefaultCellStyle.Font = robotoFont;

            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                if (column.Name != "Select") // Разрешаем редактирование только для чекбокса
                {
                    column.ReadOnly = true;
                }
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private void LoadCartData()
        {
            // Сохраняем текущую позицию прокрутки
            int firstDisplayedScrollingRowIndex = dataGridView1.FirstDisplayedScrollingRowIndex;

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
            SELECT 
                dp.id,
                a.name AS Name,
                s.name AS Size,
                COALESCE(STRING_AGG(CASE 
                    WHEN sy.name IS NOT NULL AND dps.quantity IS NOT NULL 
                    THEN CONCAT(sy.name, ' ', dps.quantity, ' шт.') 
                    ELSE NULL 
                END, ', '), '-') AS Syrup,
                dp.quantity,
                dp.item_price AS Price, -- Убедитесь, что этот столбец возвращается
                a.photo
            FROM Desired_Product dp
            JOIN Carts c ON dp.id = c.desired_product_id
            JOIN Assortment a ON dp.assortment_id = a.id
            LEFT JOIN Sizes s ON dp.size_id = s.id
            LEFT JOIN Desired_Product_Syrups dps ON dp.id = dps.desired_product_id
            LEFT JOIN Syrups sy ON dps.syrup_id = sy.id
            WHERE c.user_id = @userId
            GROUP BY dp.id, a.name, s.name, dp.quantity, dp.item_price, a.photo";

                using (var command = new NpgsqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("userId", userId);
                    var adapter = new NpgsqlDataAdapter(command);
                    cartData = new DataTable();
                    adapter.Fill(cartData);
                }
            }
            UpdateGrid(); // Обновляем интерфейс
        }
        private void UpdateGrid()
        {
            dataGridView1.Rows.Clear();
            foreach (DataRow row in cartData.Rows)
            {
                var photo = row["photo"] != DBNull.Value
                    ? Image.FromStream(new MemoryStream((byte[])row["photo"]))
                    : null;
                decimal price = Convert.ToDecimal(row["Price"]);
                int quantity = Convert.ToInt32(row["quantity"]);
                decimal totalPrice = price * quantity;

                // Проверка на наличие сиропов
                string syrupInfo = row["Syrup"] != DBNull.Value && !string.IsNullOrEmpty(row["Syrup"].ToString())
                    ? row["Syrup"].ToString()
                    : "-";

                dataGridView1.Rows.Add(
                    false,
                    row["Name"],
                    row["Size"],
                    syrupInfo, // Отображаем сиропы или "-"
                    null, // Minus Button
                    quantity,
                    null, // Plus Button
                    $"{(int)Math.Floor(totalPrice)} ₽",
                    photo
                );
            }
        }
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["Select"].Index && e.RowIndex >= 0)
            {
                UpdateTotalPrice(); // Обновляем итоговую стоимость
                UpdateSelectButtonImage(); // Обновляем изображение кнопки "Выделить все"
                UpdateSelectButtonTooltip(); // Обновляем подсказку для кнопки "Выделить все"
                UpdateUserPointsLabel(); // Обновляем баллы пользователя
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

            string imagePath = Path.Combine(
                Application.StartupPath,
                "Resources",
                "значки",
                allSelected ? "снять выделение.png" : "выделить все.png"
            );

            button4.Image = Image.FromFile(imagePath);
        }
        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Форматирование цены
            if (e.ColumnIndex == dataGridView1.Columns["Price"].Index && e.Value != null)
            {
                if (decimal.TryParse(e.Value.ToString(), out decimal price))
                {
                    e.Value = $"{Math.Round(price):0} ₽";
                    e.FormattingApplied = true;
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
            double ratio = Math.Min((double)maxWidth / image.Width, (double)maxHeight / image.Height);
            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);

            var resizedImage = new Bitmap(newWidth, newHeight);
            using (var g = Graphics.FromImage(resizedImage))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.Clear(Color.White);
                g.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            return resizedImage;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            // Проверка, открыт ли экземпляр окна навигации
            if (vkladkiForm == null || vkladkiForm.IsDisposed)
            {
                vkladkiForm = new vkladki(userId, this);
                vkladkiForm.Show();
            }
            else
            {
                vkladkiForm.Visible = !vkladkiForm.Visible;
                if (vkladkiForm.Visible) vkladkiForm.Focus();
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            List<int> productIdsToRemove = new List<int>();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                var checkCell = row.Cells["Select"] as DataGridViewCheckBoxCell;
                if (checkCell.Value != null && (bool)checkCell.Value)
                {
                    productIdsToRemove.Add((int)cartData.Rows[row.Index]["id"]);
                }
            }

            if (productIdsToRemove.Count > 0)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить выбранные товары?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No) return;

                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();

                    // Удаляем связанные записи из Desired_Product_Syrups
                    string deleteSyrupsQuery = "DELETE FROM Desired_Product_Syrups WHERE desired_product_id = ANY(@ids)";
                    using (var deleteSyrupsCommand = new NpgsqlCommand(deleteSyrupsQuery, conn))
                    {
                        deleteSyrupsCommand.Parameters.AddWithValue("ids", productIdsToRemove.ToArray());
                        deleteSyrupsCommand.ExecuteNonQuery();
                    }

                    // Удаляем связанные записи из Carts
                    string deleteCartsQuery = "DELETE FROM Carts WHERE desired_product_id = ANY(@ids)";
                    using (var deleteCartsCommand = new NpgsqlCommand(deleteCartsQuery, conn))
                    {
                        deleteCartsCommand.Parameters.AddWithValue("ids", productIdsToRemove.ToArray());
                        deleteCartsCommand.ExecuteNonQuery();
                    }

                    // Удаляем товары из Desired_Product
                    string deleteProductsQuery = "DELETE FROM Desired_Product WHERE id = ANY(@ids)";
                    using (var deleteProductsCommand = new NpgsqlCommand(deleteProductsQuery, conn))
                    {
                        deleteProductsCommand.Parameters.AddWithValue("ids", productIdsToRemove.ToArray());
                        deleteProductsCommand.ExecuteNonQuery();
                    }
                }

                LoadCartData(); // Перезагружаем данные корзины
                UpdateTotalPrice(); // Обновляем итоговую стоимость
                UpdateUserPointsLabel(); // Обновляем баллы пользователя
                UpdateSelectButtonImage(); // Обновляем изображение кнопки "Выделить все"
                UpdateSelectButtonTooltip(); // Обновляем подсказку для кнопки "Выделить все"
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите хотя бы один товар для удаления.");
            }
        }
        private void UpdateSelectButtonTooltip()
        {
            int totalItems = dataGridView1.Rows.Count;
            int selectedItems = dataGridView1.Rows.Cast<DataGridViewRow>()
                .Count(row => row.Cells["Select"].Value != null && (bool)row.Cells["Select"].Value);

            if (selectedItems == totalItems)
            {
                toolTip1.SetToolTip(button4, $"Снять выделение со всех {totalItems} товаров");
            }
            else
            {
                toolTip1.SetToolTip(button4, $"Выделить все {totalItems} товаров");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            bool allSelected = dataGridView1.Rows.Cast<DataGridViewRow>()
                .All(row => row.Cells["Select"].Value != null && (bool)row.Cells["Select"].Value);

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.Cells["Select"].Value = !allSelected;
            }
            UpdateSelectButtonImage();
            UpdateSelectButtonTooltip();
            UpdateTotalPrice();
        }

        private void UpdateTotalPrice()
        {
            decimal totalPrice = 0;
            int userPoints = GetUserPoints(); // Получаем текущие баллы пользователя

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Select"]?.Value != null && (bool)row.Cells["Select"].Value)
                {
                    decimal itemPrice = Convert.ToDecimal(row.Cells["Price"].Value.ToString().Replace(" ₽", ""));
                    int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);
                    totalPrice += itemPrice * quantity;
                }
            }

            decimal pointsToRedeem = checkBox1.Checked ? Math.Min(totalPrice * 0.3m, userPoints) : 0;
            decimal finalPrice = totalPrice - pointsToRedeem;

            label3.Text = $"Итоговая стоимость: {finalPrice:C}"; // Обновляем итоговую стоимость
            label2.Text = checkBox1.Checked ? $"Спишется {Math.Round(pointsToRedeem)} баллов" : string.Empty; // Обновляем списываемые баллы

            // Расчет начисляемых баллов (10% от стоимости после списания баллов)
            int earnedPoints = (int)Math.Floor((totalPrice - pointsToRedeem) * 0.1m);
            label4.Text = $"Начислится {earnedPoints} баллов"; // Обновляем начисляемые баллы
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTotalPrice();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<int> selectedProductIds = new List<int>();
            decimal totalPrice = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Select"]?.Value != null && (bool)row.Cells["Select"].Value)
                {
                    int productId = Convert.ToInt32(cartData.Rows[row.Index]["id"]);
                    decimal itemPrice = Convert.ToDecimal(cartData.Rows[row.Index]["Price"]);
                    int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);

                    selectedProductIds.Add(productId);
                    totalPrice += itemPrice * quantity;
                }
            }

            if (selectedProductIds.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы один товар для оформления заказа.");
                return;
            }

            int userPoints = GetUserPoints();
            decimal pointsToRedeem = checkBox1.Checked ? Math.Min(totalPrice * 0.3m, userPoints) : 0;
            decimal finalPrice = totalPrice - pointsToRedeem;

            var confirmationResult = MessageBox.Show(
                $"Оформить заказ на сумму {finalPrice:C}?\n" +
                (pointsToRedeem > 0 ? $"Списать {pointsToRedeem} баллов\n" : "") +
                $"Начислить {(int)Math.Floor(finalPrice * 0.1m)} баллов",
                "Подтверждение заказа",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirmationResult != DialogResult.Yes)
            {
                return;
            }

            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Создаем заказ
                            int orderId = CreateOrder(finalPrice, pointsToRedeem, conn, transaction);

                            // Переносим выбранные товары из корзины в заказ
                            foreach (var productId in selectedProductIds)
                            {
                                RemoveProductFromCart(productId, orderId, conn, transaction);
                            }

                            // Если используем баллы - списываем их
                            if (pointsToRedeem > 0)
                            {
                                SpendPoints((int)pointsToRedeem, orderId, conn, transaction);
                            }

                            // Начисляем баллы за заказ
                            int earnedPoints = (int)Math.Floor(finalPrice * 0.1m);
                            if (earnedPoints > 0)
                            {
                                EarnPoints(earnedPoints, orderId, conn, transaction);
                            }

                            transaction.Commit();

                            MessageBox.Show("Заказ успешно оформлен!");
                            LoadCartData(); // Обновляем данные корзины
                            UpdateUserPointsLabel(); // Обновляем баллы пользователя
                            UpdateTotalPrice(); // Обновляем итоговую стоимость
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}");
            }
        }
        private int CreateOrder(decimal totalAmount, decimal pointsToRedeem, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            string insertOrderQuery = @"
    INSERT INTO Orders (user_id, status_id)
    VALUES (@userId, 0)
    RETURNING id";

            using (var orderCommand = new NpgsqlCommand(insertOrderQuery, conn, transaction))
            {
                orderCommand.Parameters.AddWithValue("@userId", userId);
                return Convert.ToInt32(orderCommand.ExecuteScalar());
            }
        }

        private void SpendPoints(int pointsToRedeem, int orderId, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            string spendPointsFunctionCall = "SELECT spend_points(@userId, @pointsToSpend, @orderId)";

            using (var cmd = new NpgsqlCommand(spendPointsFunctionCall, conn, transaction))
            {
                cmd.Parameters.AddWithValue("userId", userId);
                cmd.Parameters.AddWithValue("pointsToSpend", pointsToRedeem);
                cmd.Parameters.AddWithValue("orderId", orderId);
                cmd.ExecuteNonQuery();
            }
        }

        private void EarnPoints(int earnedPoints, int orderId, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            string earnPointsQuery = @"
    INSERT INTO Points_Transactions (user_id, order_id, transaction_type, points)
    VALUES (@userId, @orderId, 'earn', @earnedPoints)";

            using (var cmd = new NpgsqlCommand(earnPointsQuery, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@orderId", orderId);
                cmd.Parameters.AddWithValue("@earnedPoints", earnedPoints);
                cmd.ExecuteNonQuery();
            }
        }
        private void RemoveProductFromCart(int productId, int orderId, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            string query = @"
                UPDATE Desired_Product
                SET order_id = @orderId
                WHERE id = @productId;
                
                DELETE FROM Carts
                WHERE desired_product_id = @productId;";

            using (var cmd = new NpgsqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("orderId", orderId);
                cmd.Parameters.AddWithValue("productId", productId);
                cmd.ExecuteNonQuery();
            }
        }

        private void UpdateUserPointsAfterOrder(decimal totalPrice, decimal pointsToRedeem)
        {
            // Начисляем баллы за заказ (10% от стоимости после использования баллов)
            int earnedPoints = (int)Math.Floor((totalPrice - pointsToRedeem) * 0.1m);         
        }

        private int GetUserPoints()
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
            SELECT SUM(remaining_points) FROM get_user_unexpired_earnings(@userId)";
                using (var command = new NpgsqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("userId", userId);
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        private void UpdateUserPointsLabel()
        {
            int userPoints = GetUserPoints(); // Получаем текущие баллы пользователя
            label9.Text = $"У вас {userPoints} баллов"; // Обновляем текст метки
        }

        private void ChangeQuantity(int rowIndex, int delta)
        {
            // Получаем текущее количество и ID товара
            int quantity = Convert.ToInt32(dataGridView1.Rows[rowIndex].Cells["Quantity"].Value);
            int productId = (int)cartData.Rows[rowIndex]["id"];
            int newQuantity = Math.Max(1, quantity + delta); // Минимум 1

            // Обновляем количество в базе данных
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Desired_Product SET quantity = @quantity WHERE id = @id";
                using (var command = new NpgsqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("quantity", newQuantity);
                    command.Parameters.AddWithValue("id", productId);
                    command.ExecuteNonQuery();
                }
            }

            // Сохраняем состояние выделения и позицию прокрутки
            var selectedStates = dataGridView1.Rows.Cast<DataGridViewRow>()
                .ToDictionary(row => (int)cartData.Rows[row.Index]["id"], row => row.Cells["Select"].Value != null && (bool)row.Cells["Select"].Value);
            int firstDisplayedScrollingRowIndex = dataGridView1.FirstDisplayedScrollingRowIndex;
            int scrollPosition = dataGridView1.VerticalScrollingOffset;

            // Перезагружаем данные
            LoadCartData();

            // Восстанавливаем состояние выделения и позицию прокрутки
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                int id = (int)cartData.Rows[row.Index]["id"];
                if (selectedStates.ContainsKey(id))
                {
                    row.Cells["Select"].Value = selectedStates[id];
                }
            }
            if (firstDisplayedScrollingRowIndex >= 0 && firstDisplayedScrollingRowIndex < dataGridView1.Rows.Count)
            {
                dataGridView1.FirstDisplayedScrollingRowIndex = firstDisplayedScrollingRowIndex;
            }
            // Восстанавливаем позицию прокрутки
            if (firstDisplayedScrollingRowIndex >= 0 && firstDisplayedScrollingRowIndex < dataGridView1.Rows.Count)
            {
                dataGridView1.FirstDisplayedScrollingRowIndex = firstDisplayedScrollingRowIndex;
            }

            // Обновляем интерфейс
            UpdateTotalPrice();
            UpdateSelectButtonImage();
            UpdateSelectButtonTooltip();
            UpdateUserPointsLabel();
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["Select"].Index && e.RowIndex >= 0)
            {
                // Обновляем состояние чекбокса
                var cell = dataGridView1.Rows[e.RowIndex].Cells["Select"] as DataGridViewCheckBoxCell;
                if (cell != null)
                {
                    cell.Value = !(cell.Value is bool val && val); // Инвертируем значение
                }

                // Обновляем интерфейс
                UpdateTotalPrice();
                UpdateSelectButtonImage();
                UpdateSelectButtonTooltip();
                UpdateUserPointsLabel();
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
                
    }
}