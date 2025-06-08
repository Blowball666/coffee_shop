using System.Data;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Npgsql;

namespace кофейня.polzovatel
{
    public partial class info : Form
    {
        private int assortmentId;
        private int userId;
        private int userRoleId;
        private Form menuForm;
        public int quantity = 1;
        public decimal finalPrice;
        private string connectionString = "Host=localhost;Database=coffee_db;Username=postgres;Password=pwd";

        // Инициализация формы
        public info(int assortmentId, int userId, int userRoleId, Form menuForm)
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(270, 50);
            this.assortmentId = assortmentId;
            this.userId = userId;
            this.userRoleId = userRoleId;
            this.menuForm = menuForm;

            // Загрузка данных
            LoadSizes();
            LoadSyrupsToDataGridView();
            LoadProductDetails();
            UpdateWishlistButtonImage();
            LoadUserPoints();

            // Установка начальных значений
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0; // Выбор первого размера по умолчанию
            }
            label6.Text = "1"; // Начальное количество напитков
            quantity = 1; // Инициализация переменной для количества

            // Обновление цены и начисляемых баллов при загрузке формы
            UpdatePrice();
            UpdateEarnedPoints(); // Новый метод для обновления начисляемых баллов
        }
        private void UpdateEarnedPoints()
        {
            decimal totalPriceForOrder = finalPrice - (checkBox1.Checked ? Math.Min(finalPrice * 0.3m, GetUserPoints()) : 0);
            int newPointsEarned = CalculateNewPoints(totalPriceForOrder);
            label5.Text = $"Начислится: {newPointsEarned} баллов";
        }
        // Загрузка размеров
        internal void LoadSizes()
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
            SELECT s.id, s.name, s.volume, s.price_multiplier
            FROM Sizes s
            JOIN Assortment_Sizes a ON s.id = a.size_id
            WHERE a.assortment_id = @assortmentId";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@assortmentId", assortmentId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            comboBox1.Items.Add(new
                            {
                                Id = reader.GetInt32(0),
                                Name = $"{reader.GetString(1)} ({reader.GetDecimal(2):0.00} л)",
                                PriceMultiplier = reader.GetDecimal(3)
                            });
                        }
                    }
                }
            }
            comboBox1.DisplayMember = "Name";
            comboBox1.ValueMember = "Id";
        }
        internal Image ResizeImage(Image image, int maxWidth, int maxHeight)
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
        // Загрузка сиропов в DataGridView
        internal void LoadSyrupsToDataGridView()
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
            SELECT sy.id, sy.name, sy.price
            FROM Syrups sy
            JOIN Assortment_Syrups a ON sy.id = a.syrup_id
            WHERE a.assortment_id = @assortmentId";
                using (var adapter = new NpgsqlDataAdapter(query, connection))
                {
                    adapter.SelectCommand.Parameters.AddWithValue("@assortmentId", assortmentId);
                    var dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    // Настройка DataGridView
                    dataGridView1.DataSource = dataTable;
                    dataGridView1.Columns["id"].Visible = false;
                    dataGridView1.Columns["name"].HeaderText = "Сироп";
                    dataGridView1.Columns["name"].Width = 140;
                    dataGridView1.Columns["price"].HeaderText = "Цена";

                    // Уменьшение шрифта заголовков и ширины столбцов
                    dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Roboto", 14, FontStyle.Bold);
                    dataGridView1.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(202, 57, 0);
                    dataGridView1.Columns["price"].Width = 70;
                    Font robotoFont = new Font("Roboto", 12, FontStyle.Bold);
                    dataGridView1.DefaultCellStyle.Font = robotoFont;
                    dataGridView1.RowTemplate.DefaultCellStyle.Padding = new Padding(0, 5, 0, 5);
                    dataGridView1.ClearSelection();
                    dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    dataGridView1.MultiSelect = false;
                    dataGridView1.DefaultCellStyle.SelectionBackColor = dataGridView1.DefaultCellStyle.BackColor;
                    dataGridView1.DefaultCellStyle.SelectionForeColor = dataGridView1.DefaultCellStyle.ForeColor;
                    dataGridView1.ReadOnly = true;

                    // Добавляем кнопку "-"
                    var minusButtonColumn = new DataGridViewImageColumn
                    {
                        Name = "Minus",
                        HeaderText = "",
                        Image = Image.FromFile(Path.Combine(Application.StartupPath, "Resources", "кнопки", "минус.png")),
                        Width = 45
                    };
                    dataGridView1.Columns.Add(minusButtonColumn);

                    // Добавляем колонку "Количество"
                    var quantityColumn = new DataGridViewTextBoxColumn
                    {
                        Name = "Quantity",
                        HeaderText = "Кол-во",
                        Width = 70,
                        DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
                    };
                    dataGridView1.Columns.Add(quantityColumn);

                    // Добавляем кнопку "+"
                    var plusButtonColumn = new DataGridViewImageColumn
                    {
                        Name = "Plus",
                        HeaderText = "",
                        Image = Image.FromFile(Path.Combine(Application.StartupPath, "Resources", "кнопки", "плюс.png")),
                        Width = 45
                    };
                    dataGridView1.Columns.Add(plusButtonColumn);

                    // Инициализация количества через событие DataBindingComplete
                    dataGridView1.DataBindingComplete += (sender, e) =>
                    {
                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            if (!row.IsNewRow) // Пропускаем новую пустую строку
                            {
                                row.Cells["Quantity"].Value = 0; // Начальное значение количества
                            }
                        }
                    };
                }
            }
        }
        // Обновление цены при изменении размера
        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePrice();
            UpdateEarnedPoints(); // Обновление начисляемых баллов
        }

        // Обработка нажатий кнопок "+" и "-" в DataGridView
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            var row = dataGridView1.Rows[e.RowIndex];
            int currentQuantity = Convert.ToInt32(row.Cells["Quantity"].Value);

            if (dataGridView1.Columns[e.ColumnIndex].Name == "Plus")
            {
                row.Cells["Quantity"].Value = currentQuantity + 1;
                UpdatePrice();
                UpdateEarnedPoints(); // Обновление начисляемых баллов
            }
            else if (dataGridView1.Columns[e.ColumnIndex].Name == "Minus" && currentQuantity > 0)
            {
                row.Cells["Quantity"].Value = currentQuantity - 1;
                UpdatePrice();
                UpdateEarnedPoints(); // Обновление начисляемых баллов
            }
        }
        // Пересчет итоговой цены
        public void UpdatePrice()
        {
            decimal basePrice = GetBasePrice();
            decimal sizePrice = GetSizePrice();
            decimal syrupPrice = GetSyrupPrice();
            decimal totalPrice = basePrice + sizePrice + syrupPrice;

            decimal preciseFinalPrice = CalculateFinalPrice(totalPrice) * quantity;
            finalPrice = Math.Round(preciseFinalPrice, 0);

            label3.Text = $"{finalPrice:C}";

            if (checkBox1.Checked)
            {
                int userPoints = GetUserPoints();
                decimal pointsToRedeem = Math.Min(finalPrice * 0.3m, userPoints);
                label11.Text = $"Спишется: {Math.Round(pointsToRedeem)} баллов";
            }
            else
            {
                label11.Text = "Спишется: 0 баллов";
            }

            UpdateEarnedPoints();
        }

        // Получение базовой цены товара
        internal decimal GetBasePrice()
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT price FROM Assortment WHERE id = @id";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", assortmentId);
                    return (decimal)command.ExecuteScalar();
                }
            }
        }

        // Получение цены размера (с учетом множителя)
        internal decimal GetSizePrice()
        {
            if (comboBox1.SelectedItem != null)
            {
                decimal multiplier = ((dynamic)comboBox1.SelectedItem).PriceMultiplier;
                return (multiplier - 1) * GetBasePrice(); // Только дополнительная стоимость
            }
            return 0;
        }
        // Получение цены сиропов
        internal decimal GetSyrupPrice()
        {
            decimal totalSyrupPrice = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);
                decimal price = Convert.ToDecimal(row.Cells["price"].Value);
                totalSyrupPrice += quantity * price;
            }
            return totalSyrupPrice;
        }

        // Расчет итоговой цены
        internal decimal CalculateFinalPrice(decimal totalPrice)
        {
            return Math.Round(totalPrice * quantity, 2);
        }
        internal void LoadProductDetails()
        {
            string query = @"
        SELECT name, description, price, photo 
        FROM Assortment 
        WHERE id = @assortmentId";

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@assortmentId", assortmentId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            label1.Text = reader.GetString(0); // Название товара
                            string description = reader.IsDBNull(1) ? "Описание отсутствует" : reader.GetString(1);
                            decimal basePrice = reader.GetDecimal(2); // Базовая цена
                            label10.Text = $"Цена: от {basePrice:C}"; // Базовая цена

                            // Вывод описания в TextBox с форматированием
                            textBox1.Text = FormatDescription(description);

                            var imageData = reader["photo"] as byte[];
                            if (imageData != null)
                            {
                                using (var ms = new MemoryStream(imageData))
                                using (var img = Image.FromStream(ms))
                                {
                                    pictureBox1.Image = ResizeImage(img, pictureBox1.Width, pictureBox1.Height);
                                }
                            }
                        }
                    }
                }
            }
            UpdatePrice(); // Обновление цены после загрузки данных
        }
        private string FormatDescription(string description)
        {
            // Проверяем, начинается ли описание с "Состав:"
            if (description.StartsWith("Состав:"))
            {
                // Разбиваем текст на части по точкам
                string[] parts = description.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                List<string> paragraphs = new List<string>();

                foreach (var part in parts)
                {
                    var trimmed = part.Trim();

                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        // Если это первая часть и она содержит "Состав", добавляем двоеточие и начинаем новый абзац
                        if (paragraphs.Count == 0 && trimmed.StartsWith("Состав"))
                        {
                            paragraphs.Add("Состав:" + Environment.NewLine + trimmed.Substring("Состав:".Length).Trim());
                        }
                        else
                        {
                            paragraphs.Add(trimmed); // Остальные части добавляем без изменений
                        }
                    }
                }

                // Объединяем абзацы с новой строки
                return string.Join(Environment.NewLine + Environment.NewLine, paragraphs);
            }
            else
            {
                // Если нет "Состав:", просто разбиваем по точкам
                string[] parts = description.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                List<string> paragraphs = new List<string>();

                foreach (var part in parts)
                {
                    var trimmed = part.Trim();

                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        paragraphs.Add(trimmed);
                    }
                }

                // Объединяем абзацы с новой строки
                return "Состав:" + Environment.NewLine + string.Join(Environment.NewLine, paragraphs);
            }
        }
        private void UpdateWishlistButtonImage()
        {
            button2.BackgroundImage = GetWishlistImage(assortmentId);
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
        internal void LoadUserPoints()
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
            SELECT SUM(remaining_points) 
            FROM get_user_unexpired_earnings(@userId)";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    int points = Convert.ToInt32(command.ExecuteScalar() ?? 0);
                    label9.Text = $"У вас {points} баллов";
                }
            }
        }
        private void Info_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true; // Отменяем закрытие формы, чтобы она только скрывалась
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (userRoleId == 3) // Проверка, если пользователь - гость
            {
                PromptForRegistration();
                return;
            }

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // Проверяем, есть ли товар в wishlist
                    string checkQuery = @"
                SELECT COUNT(*) 
                FROM Wishlist 
                WHERE user_id = @userId AND assortment_id = @assortmentId";
                    using (var checkCmd = new NpgsqlCommand(checkQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@userId", userId);
                        checkCmd.Parameters.AddWithValue("@assortmentId", assortmentId);

                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (count > 0)
                        {
                            // Удаляем товар из wishlist
                            string deleteQuery = @"
                        DELETE FROM Wishlist 
                        WHERE user_id = @userId AND assortment_id = @assortmentId";
                            using (var deleteCmd = new NpgsqlCommand(deleteQuery, connection))
                            {
                                deleteCmd.Parameters.AddWithValue("@userId", userId);
                                deleteCmd.Parameters.AddWithValue("@assortmentId", assortmentId);
                                deleteCmd.ExecuteNonQuery();
                                ShowNotification("Товар удален из отложенных.");
                            }
                        }
                        else
                        {
                            // Добавляем товар в wishlist
                            string insertQuery = @"
                        INSERT INTO Wishlist (user_id, assortment_id)
                        VALUES (@userId, @assortmentId)";
                            using (var insertCmd = new NpgsqlCommand(insertQuery, connection))
                            {
                                insertCmd.Parameters.AddWithValue("@userId", userId);
                                insertCmd.Parameters.AddWithValue("@assortmentId", assortmentId);
                                insertCmd.ExecuteNonQuery();
                                ShowNotification("Товар добавлен в отложенные.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при работе с wishlist: {ex.Message}");
            }
            UpdateWishlistButtonImage();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (userRoleId == 3) // Проверка, если пользователь - гость
            {
                PromptForRegistration();
                return;
            }

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    int userPoints = GetUserPoints();
                    decimal maxRedeemable = finalPrice * 0.3m;
                    decimal pointsToRedeem = 0;

                    if (checkBox1.Checked)
                    {
                        // Используем либо доступные баллы, либо 30% от стоимости
                        pointsToRedeem = Math.Min(maxRedeemable, userPoints);
                    }

                    decimal totalPriceForOrder = finalPrice - pointsToRedeem;
                    int newPointsEarned = CalculateNewPoints(totalPriceForOrder);

                    var confirmationResult = MessageBox.Show(
                        "Вы уверены, что хотите оформить заказ?",
                        "Подтверждение заказа",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );

                    if (confirmationResult != DialogResult.Yes)
                    {
                        return;
                    }

                    // Создание заказа
                    string insertOrderQuery = @"
            INSERT INTO Orders (user_id, order_date, status_id)
            VALUES (@userId, CURRENT_TIMESTAMP, 0)
            RETURNING id";

                    int orderId;
                    using (var cmd = new NpgsqlCommand(insertOrderQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        orderId = (int)cmd.ExecuteScalar();
                    }

                    // Добавление товара в заказ
                    int sizeId = comboBox1.SelectedItem != null ? ((dynamic)comboBox1.SelectedItem).Id : 0;
                    string insertDesiredProductQuery = @"
            INSERT INTO Desired_Product (assortment_id, size_id, quantity, order_id, item_price)
            VALUES (@assortmentId, @sizeId, @quantity, @orderId, @itemPrice)
            RETURNING id";

                    int desiredProductId;
                    using (var cmd = new NpgsqlCommand(insertDesiredProductQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@assortmentId", assortmentId);
                        cmd.Parameters.AddWithValue("@sizeId", sizeId);
                        cmd.Parameters.AddWithValue("@quantity", quantity);
                        cmd.Parameters.AddWithValue("@orderId", orderId);
                        cmd.Parameters.AddWithValue("@itemPrice", finalPrice);
                        desiredProductId = (int)cmd.ExecuteScalar();
                    }

                    // Добавление сиропов к заказу
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        int syrupId = Convert.ToInt32(row.Cells["id"].Value);
                        int syrupQuantity = Convert.ToInt32(row.Cells["Quantity"].Value);
                        if (syrupQuantity > 0)
                        {
                            string insertSyrupQuery = @"
                    INSERT INTO Desired_Product_Syrups (desired_product_id, syrup_id, quantity)
                    VALUES (@desiredProductId, @syrupId, @quantity)";

                            using (var syrupCmd = new NpgsqlCommand(insertSyrupQuery, connection))
                            {
                                syrupCmd.Parameters.AddWithValue("@desiredProductId", desiredProductId);
                                syrupCmd.Parameters.AddWithValue("@syrupId", syrupId);
                                syrupCmd.Parameters.AddWithValue("@quantity", syrupQuantity);
                                syrupCmd.ExecuteNonQuery();
                            }
                        }
                    }

                    // Списание баллов
                    if (pointsToRedeem > 0)
                    {
                        string spendPointsFunction = "SELECT spend_points(@userId, @pointsToRedeem, @orderId)";
                        using (var cmd = new NpgsqlCommand(spendPointsFunction, connection))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.Parameters.AddWithValue("@pointsToRedeem", (int)pointsToRedeem);
                            cmd.Parameters.AddWithValue("@orderId", orderId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Начисление баллов за заказ
                    if (newPointsEarned > 0)
                    {
                        string earnPointsQuery = @"
                INSERT INTO Points_Transactions (user_id, order_id, transaction_type, points)
                VALUES (@userId, @orderId, 'earn', @newPointsEarned)";

                        using (var cmd = new NpgsqlCommand(earnPointsQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.Parameters.AddWithValue("@orderId", orderId);
                            cmd.Parameters.AddWithValue("@newPointsEarned", newPointsEarned);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Заказ успешно создан!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании заказа: {ex.Message}");
            }
        }

        private void button1_Click(object sender, EventArgs e) //корзина
        {
            if (userRoleId == 3) // Проверка, если пользователь - гость
            {
                PromptForRegistration();
                return;
            }

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    int sizeId = comboBox1.SelectedItem != null ? ((dynamic)comboBox1.SelectedItem).Id : 0;
                    int desiredProductId = ExecuteScalarQuery(
                        "INSERT INTO Desired_Product (assortment_id, size_id, quantity, item_price) " +
                        "VALUES (@assortmentId, @sizeId, @quantity, @itemPrice) RETURNING id",
                        new NpgsqlParameter("@assortmentId", assortmentId),
                        new NpgsqlParameter("@sizeId", sizeId),
                        new NpgsqlParameter("@quantity", quantity),
                        new NpgsqlParameter("@itemPrice", finalPrice));

                    ExecuteNonQuery(
                        "INSERT INTO Carts (user_id, desired_product_id) VALUES (@userId, @desiredProductId)",
                        new NpgsqlParameter("@userId", userId),
                        new NpgsqlParameter("@desiredProductId", desiredProductId));

                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        int currentSyrupId = Convert.ToInt32(row.Cells["id"].Value);
                        int syrupQuantity = Convert.ToInt32(row.Cells["Quantity"].Value);
                        if (syrupQuantity > 0)
                        {
                            ExecuteNonQuery(
                                "INSERT INTO Desired_Product_Syrups (desired_product_id, syrup_id, quantity) " +
                                "VALUES (@desiredProductId, @syrupId, @syrupQuantity)",
                                new NpgsqlParameter("@desiredProductId", desiredProductId),
                                new NpgsqlParameter("@syrupId", currentSyrupId),
                                new NpgsqlParameter("@syrupQuantity", syrupQuantity));
                        }
                    }

                    ShowNotification("Товар добавлен в корзину.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении товара в корзину: {ex.Message}");
            }
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
                menuForm.Hide();
            }
        }
        private void ShowNotification(string message)
        {
            MessageBox.Show(message, "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private int ExecuteScalarQuery(string query, params NpgsqlParameter[] parameters)
        {
            using (var conn = new NpgsqlConnection("Host=localhost;Database=coffee_db;Username=postgres;Password=pwd"))
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
            using (var conn = new NpgsqlConnection("Host=localhost;Database=coffee_db;Username=postgres;Password=pwd"))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddRange(parameters);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            quantity++;
            label6.Text = quantity.ToString();
            UpdatePrice();
            UpdateEarnedPoints(); // Обновление начисляемых баллов
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (quantity > 1)
            {
                quantity--;
                label6.Text = quantity.ToString();
                UpdatePrice();
                UpdateEarnedPoints(); // Обновление начисляемых баллов
            }
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            int userPoints = GetUserPoints();
            decimal maxRedeemable = finalPrice * 0.3m;

            if (checkBox1.Checked)
            {
                // Если отмечено, используем либо 30%, либо все доступные баллы
                decimal pointsToRedeem = Math.Min(maxRedeemable, userPoints);
                label11.Text = $"Спишется: {Math.Round(pointsToRedeem)} баллов";
            }
            else
            {
                // Если не отмечено, обнуляем списание
                label11.Text = "Спишется: 0 баллов";
            }

            UpdatePrice();
            UpdateEarnedPoints();
        }
        private int GetUserPoints()
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
            SELECT SUM(remaining_points) 
            FROM get_user_unexpired_earnings(@userId)";
                using (var command = new NpgsqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("userId", userId);
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }
        private void UpdateUserPoints(decimal pointsSpent)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
            INSERT INTO Points_Transactions (user_id, transaction_type, points)
            VALUES (@userId, 'spend', @pointsSpent)";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@pointsSpent", pointsSpent);
                    command.ExecuteNonQuery();
                }
            }

            LoadUserPoints(); // Обновляем метку с текущими баллами
        }
        private int CalculateNewPoints(decimal orderFinalPrice)
        {
            return (int)(orderFinalPrice * 0.1m); // Начисляем 10% от стоимости заказа
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}