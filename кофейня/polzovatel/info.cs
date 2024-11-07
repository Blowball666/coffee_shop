using System.Drawing.Drawing2D;
using Npgsql;
using System.Globalization;

namespace кофейня.polzovatel
{
    public partial class info : Form
    {
        private int assortmentId;
        private int userId;
        private int quantity = 1;
        private decimal finalPrice;
        private string connectionString = "Host=172.20.7.6; Database=krezhowa_coffee; Username=st; Password=pwd";

        public info(int assortmentId, int userId)
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(50, 50);
            this.assortmentId = assortmentId;
            this.userId = userId;

            LoadSizes();
            LoadSyrups();
            LoadProductDetails();
            UpdateWishlistButtonImage();
            LoadUserPoints();

            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }
            if (comboBox2.Items.Count > 0)
            {
                comboBox2.SelectedIndex = 0;
            }
            this.FormClosing += Info_FormClosing;
        }
        private void Info_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }
        private void LoadUserPoints()
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT points FROM Users WHERE id = @userId";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("userId", userId);
                    int points = Convert.ToInt32(command.ExecuteScalar());
                    label9.Text = $"У вас {points} баллов";
                }
            }
        }
        private void LoadSizes()
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
            SELECT s.id, s.name, s.price
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
                                Name = $"{reader.GetString(1)} {reader.GetDecimal(2).ToString("0.00")} ₽"
                            });
                        }
                    }
                }
            }

            comboBox1.DisplayMember = "Name";
            comboBox1.ValueMember = "Id";
        }
        private void LoadSyrups()
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
            SELECT sy.id, sy.name, sy.price
            FROM Syrups sy
            JOIN Assortment_Syrups a ON sy.id = a.syrup_id
            WHERE a.assortment_id = @assortmentId";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@assortmentId", assortmentId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            comboBox2.Items.Add(new
                            {
                                Id = reader.GetInt32(0),
                                Name = $"{reader.GetString(1)} {reader.GetDecimal(2).ToString("0.00")} ₽"
                            });
                        }
                    }
                }
            }

            comboBox2.DisplayMember = "Name";
            comboBox2.ValueMember = "Id";
        }
        private void LoadProductDetails()
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT name, description, price, photo FROM Assortment WHERE id = @assortmentId";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@assortmentId", assortmentId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            label1.Text = reader.GetString(0);
                            label2.Text = reader.GetString(1);
                            decimal basePrice = reader.GetDecimal(2);
                            finalPrice = CalculateFinalPrice(basePrice);
                            label3.Text = $"Итоговая цена: {finalPrice:C}";

                            label10.Text = $"Цена: от {basePrice:C}";

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
            UpdatePrice();
        }
        private Image GetWishlistImage(int assortmentId)
        {
            string filledHeartPath = Path.Combine(Application.StartupPath, "Resources", "значки", "сердце.png");
            string emptyHeartPath = Path.Combine(Application.StartupPath, "Resources", "значки", "пуст_сердце.png");

            Image filledHeart = File.Exists(filledHeartPath) ? Image.FromFile(filledHeartPath) : null;
            Image emptyHeart = File.Exists(emptyHeartPath) ? Image.FromFile(emptyHeartPath) : null;

            return CheckIfInWishlist(assortmentId) ? filledHeart : emptyHeart;
        }
        private void UpdateWishlistButtonImage()
        {
            button2.BackgroundImage = GetWishlistImage(assortmentId);
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
        private void button2_Click(object sender, EventArgs e)
        {
            // Логика для добавления/удаления из wishlist
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

            UpdateWishlistButtonImage();
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
        private decimal CalculateFinalPrice(decimal basePrice)
        {
            decimal syrupPrice = GetSyrupPrice();
            decimal sizePrice = GetSizePrice();
            decimal markup = 0.0M; // наценка
            decimal totalPrice = (basePrice + syrupPrice + sizePrice) * quantity * (1 + markup);
            return Math.Round(totalPrice, 2);
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
        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePrice();
        }
        private decimal GetBasePrice()
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT price FROM Assortment WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("id", assortmentId);
                    return (decimal)command.ExecuteScalar();
                }
            }
        }
        private decimal GetSyrupPrice()
        {
            if (comboBox2.SelectedItem != null)
            {
                int syrupId = ((dynamic)comboBox2.SelectedItem).Id;
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new NpgsqlCommand("SELECT price FROM Syrups WHERE id = @id", connection))
                    {
                        command.Parameters.AddWithValue("id", syrupId);
                        return (decimal)command.ExecuteScalar();
                    }
                }
            }
            return 0;
        }
        private decimal GetSizePrice()
        {
            if (comboBox1.SelectedItem != null)
            {
                int sizeId = ((dynamic)comboBox1.SelectedItem).Id;
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new NpgsqlCommand("SELECT price FROM Sizes WHERE id = @id", connection))
                    {
                        command.Parameters.AddWithValue("id", sizeId);
                        return (decimal)command.ExecuteScalar();
                    }
                }
            }
            return 0;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // Расчет списываемых баллов
                int userPoints = GetUserPoints();
                decimal pointsToRedeem = checkBox1.Checked ? finalPrice * 0.3m : 0;
                if (userPoints < pointsToRedeem)
                {
                    pointsToRedeem = userPoints; // Списать все доступные баллы
                }
                int pointsSpent = (int)pointsToRedeem;
                decimal totalPriceForOrder = finalPrice - pointsSpent; // Изначальная цена без вычета баллов

                // Вставка заказа в таблицу
                using (var command = new NpgsqlCommand("INSERT INTO Orders (user_id, total_price, points_spent, order_date, status_id) VALUES (@userId, @totalPrice, @pointsSpent, @orderDate, @statusId) RETURNING id", connection))
                {
                    command.Parameters.AddWithValue("userId", userId);
                    command.Parameters.AddWithValue("totalPrice", totalPriceForOrder); // Используем итоговую цену с вычетом баллов
                    command.Parameters.AddWithValue("pointsSpent", pointsSpent);
                    command.Parameters.AddWithValue("orderDate", DateTime.Now);
                    command.Parameters.AddWithValue("statusId", 0);

                    try
                    {
                        int orderId = (int)command.ExecuteScalar();
                        AddOrderDetails(orderId); // Передаем orderId в метод добавления деталей заказа
                        UpdateUserPoints(userPoints - pointsSpent + CalculateNewPoints(finalPrice));
                        MessageBox.Show("Заказ успешно создан!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при создании заказа: {ex.Message}");
                    }
                }
            }
            LoadUserPoints();
        }
        private void UpdatePrice()
        {
            decimal basePrice = GetBasePrice();
            decimal syrupPrice = GetSyrupPrice();
            decimal sizePrice = GetSizePrice();
            finalPrice = CalculateFinalPrice(basePrice);
            label3.Text = finalPrice.ToString("C");
            label6.Text = quantity.ToString();

            int userPoints = GetUserPoints();
            decimal pointsToRedeem = checkBox1.Checked ? finalPrice * 0.3m : 0;

            if (userPoints < pointsToRedeem)
            {
                pointsToRedeem = userPoints;
            }

            label11.Text = $"Спишется: {pointsToRedeem} баллов";
        }
        private void AddOrderDetails(int orderId)
        {
            int syrupId = comboBox2.SelectedItem != null ? ((dynamic)comboBox2.SelectedItem).Id : 0;
            int sizeId = comboBox1.SelectedItem != null ? ((dynamic)comboBox1.SelectedItem).Id : 0;

            // Используем итоговую стоимость заказа (с учетом размера, сиропа и т.д.)
            decimal pricePerItem = CalculateFinalPrice(GetBasePrice());

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("INSERT INTO DesiredProduct (order_id, assortment_id, size_id, syrup_id, quantity, price) VALUES (@orderId, @assortmentId, @sizeId, @syrupId, @quantity, @price)", connection))
                {
                    command.Parameters.AddWithValue("@orderId", orderId);
                    command.Parameters.AddWithValue("@assortmentId", assortmentId);
                    command.Parameters.AddWithValue("@sizeId", sizeId);
                    command.Parameters.AddWithValue("@syrupId", syrupId);
                    command.Parameters.AddWithValue("@quantity", quantity);
                    command.Parameters.AddWithValue("@price", pricePerItem); // Здесь добавляем итоговую стоимость с учетом всех факторов
                    command.ExecuteNonQuery();
                }
            }
        }
        private int GetUserPoints()
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT points FROM Users WHERE id = @userId", connection))
                {
                    command.Parameters.AddWithValue("userId", userId);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        private void UpdateUserPoints(int newPoints)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("UPDATE Users SET points = @points WHERE id = @userId", connection))
                {
                    command.Parameters.AddWithValue("points", newPoints);
                    command.Parameters.AddWithValue("userId", userId);
                    command.ExecuteNonQuery();
                }
            }
        }
        private int CalculateNewPoints(decimal orderFinalPrice)
        {
            return (int)(orderFinalPrice * 0.1m);
        }
        private decimal GetTotalPrice()
        {
            return CalculateFinalPrice(GetBasePrice() + GetSyrupPrice() + GetSizePrice()) * quantity;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // Получаем id корзины для пользователя
                int basketId = ExecuteScalarQuery(
                    "SELECT id FROM Carts WHERE user_id = @userId",
                    new NpgsqlParameter("@userId", userId)
                );

                // Если корзины нет, создаем новую
                if (basketId == 0)
                {
                    ExecuteNonQuery("INSERT INTO Carts (user_id) VALUES (@userId)", new NpgsqlParameter("@userId", userId));
                    basketId = ExecuteScalarQuery("SELECT id FROM Carts WHERE user_id = @userId", new NpgsqlParameter("@userId", userId));
                }

                // Теперь создаем запись в таблице желаемых товаров, указывая внешний ключ на корзину
                ExecuteNonQuery(
                    "INSERT INTO DesiredProduct (cart_id, assortment_id, size_id, syrup_id, quantity, price) VALUES (@cartId, @assortmentId, @sizeId, @syrupId, @quantity, @price)",
                    new NpgsqlParameter("@cartId", basketId),
                    new NpgsqlParameter("@assortmentId", assortmentId),
                    new NpgsqlParameter("@sizeId", comboBox1.SelectedItem != null ? ((dynamic)comboBox1.SelectedItem).Id : (object)DBNull.Value),
                    new NpgsqlParameter("@syrupId", comboBox2.SelectedItem != null ? ((dynamic)comboBox2.SelectedItem).Id : (object)DBNull.Value),
                    new NpgsqlParameter("@quantity", quantity),
                    new NpgsqlParameter("@price", finalPrice) // предполагается, что finalPrice - это итоговая цена
                );

                ShowNotification("Товар добавлен в корзину."); // Показываем уведомление об успешном добавлении
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (quantity > 1)
            {
                quantity--;
                UpdatePrice();
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            quantity++;
            UpdatePrice();
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePrice();
        }
    }
}