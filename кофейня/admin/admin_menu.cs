using System.Data;
using Npgsql;
using OfficeOpenXml;
using кофейня.admin;

namespace кофейня
{
    public partial class admin_menu : Form
    {
        private const string ConnectionString = "Host=localhost;Database=coffee_db;Username=postgres;Password=pwd";
        private string defaultImagePath = Path.Combine(Application.StartupPath, "Resources", "значки", "фото_не_найдено.png");
        private string selectedImagePath;
        private int selectedProductId = -1;
        private vkladki_a vkladkiForm;

        public admin_menu()
        {
            InitializeComponent();
            SetupDataGridView();
            LoadAllProducts();
            selectedImagePath = defaultImagePath;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);
            LoadAllSyrups();
        }

        private void SetupDataGridView()
        {
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // Установка стиля выделения
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.FromArgb(245, 226, 218);
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;

            // Установка шрифта
            Font robotoFont = new Font("Roboto", 14);
            dataGridView1.DefaultCellStyle.Font = robotoFont;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;

            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // Установка стиля выделения
            dataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView2.DefaultCellStyle.SelectionBackColor = Color.FromArgb(245, 226, 218);
            dataGridView2.DefaultCellStyle.SelectionForeColor = Color.Black;

            // Установка шрифта
            dataGridView2.DefaultCellStyle.Font = robotoFont;
            dataGridView2.ReadOnly = true;
            dataGridView2.AllowUserToAddRows = false;

        }
        // Обработчик для кнопки сохранения изменений
        public void button1_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                try
                {
                    // Save or update product details
                    int newProductId = SaveOrUpdateProduct();

                    // Update relationships for size and syrup options
                    UpdateAssortmentRelations(newProductId, checkBox2.Checked, checkBox3.Checked);

                    MessageBox.Show("Изменения успешно сохранены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadAllProducts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        public void UpdateAssortmentRelations(int productId, bool canChooseSize, bool canAddSyrup)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    // Удаляем старые связи
                    var deleteSizesQuery = "DELETE FROM Assortment_Sizes WHERE assortment_id = @ProductId";
                    var deleteSyrupsQuery = "DELETE FROM Assortment_Syrups WHERE assortment_id = @ProductId";
                    using (var deleteSizesCommand = new NpgsqlCommand(deleteSizesQuery, connection))
                    {
                        deleteSizesCommand.Parameters.AddWithValue("@ProductId", productId);
                        deleteSizesCommand.ExecuteNonQuery();
                    }
                    using (var deleteSyrupsCommand = new NpgsqlCommand(deleteSyrupsQuery, connection))
                    {
                        deleteSyrupsCommand.Parameters.AddWithValue("@ProductId", productId);
                        deleteSyrupsCommand.ExecuteNonQuery();
                    }
                    // Создаем новые связи
                    if (canChooseSize)
                    {
                        var insertSizesQuery = "INSERT INTO Assortment_Sizes (assortment_id, size_id) " +
                                               "SELECT @ProductId, id FROM Sizes ON CONFLICT DO NOTHING";
                        using (var insertSizesCommand = new NpgsqlCommand(insertSizesQuery, connection))
                        {
                            insertSizesCommand.Parameters.AddWithValue("@ProductId", productId);
                            insertSizesCommand.ExecuteNonQuery();
                        }
                    }
                    if (canAddSyrup)
                    {
                        var insertSyrupsQuery = "INSERT INTO Assortment_Syrups (assortment_id, syrup_id) " +
                                                "SELECT @ProductId, id FROM Syrups ON CONFLICT DO NOTHING";
                        using (var insertSyrupsCommand = new NpgsqlCommand(insertSyrupsQuery, connection))
                        {
                            insertSyrupsCommand.Parameters.AddWithValue("@ProductId", productId);
                            insertSyrupsCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления связей: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private int SaveOrUpdateProduct()
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                // Если фото не указано, использовать фото по умолчанию
                if (string.IsNullOrEmpty(selectedImagePath))
                {
                    selectedImagePath = defaultImagePath;
                }
                if (selectedProductId != -1)
                {
                    // Обновление товара
                    var updateQuery = "UPDATE Assortment SET name = @Name, description = @Description, price = @Price, " +
                                      "in_stock = @InStock, can_choose_size = @CanChooseSize, can_add_syrup = @CanAddSyrup" +
                                      (string.IsNullOrEmpty(selectedImagePath) ? "" : ", photo = @Photo") +
                                      " WHERE id = @ProductId";
                    using (var command = new NpgsqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Name", textBox1.Text);
                        command.Parameters.AddWithValue("@Description", textBox2.Text);
                        command.Parameters.AddWithValue("@Price", decimal.Parse(textBox3.Text));
                        command.Parameters.AddWithValue("@InStock", checkBox1.Checked);
                        command.Parameters.AddWithValue("@CanChooseSize", checkBox2.Checked);
                        command.Parameters.AddWithValue("@CanAddSyrup", checkBox3.Checked);
                        command.Parameters.AddWithValue("@ProductId", selectedProductId);
                        byte[] photoBytes = File.ReadAllBytes(selectedImagePath);
                        command.Parameters.AddWithValue("@Photo", photoBytes);
                        command.ExecuteNonQuery();
                    }
                    return selectedProductId;
                }
                else
                {
                    // Добавление нового товара
                    var insertQuery = "INSERT INTO Assortment (name, description, price, in_stock, can_choose_size, can_add_syrup, photo) " +
                                      "VALUES (@Name, @Description, @Price, @InStock, @CanChooseSize, @CanAddSyrup, @Photo) RETURNING id";
                    using (var command = new NpgsqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Name", textBox1.Text);
                        command.Parameters.AddWithValue("@Description", textBox2.Text);
                        command.Parameters.AddWithValue("@Price", decimal.Parse(textBox3.Text));
                        command.Parameters.AddWithValue("@InStock", checkBox1.Checked);
                        command.Parameters.AddWithValue("@CanChooseSize", checkBox2.Checked);
                        command.Parameters.AddWithValue("@CanAddSyrup", checkBox3.Checked);
                        byte[] photoBytes = File.ReadAllBytes(selectedImagePath);
                        command.Parameters.AddWithValue("@Photo", photoBytes);
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
        }
        // Проверка введенных данных
        public bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) ||
                string.IsNullOrWhiteSpace(textBox2.Text) ||
                !decimal.TryParse(textBox3.Text, out decimal price) ||
                price <= 0)
            {
                MessageBox.Show("Пожалуйста, заполните все поля корректно.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
        // Кнопка добавления нового товара
        private void button2_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                try
                {
                    using (var connection = new NpgsqlConnection(ConnectionString))
                    {
                        connection.Open();
                        var query = "INSERT INTO Assortment (name, description, price, in_stock, can_choose_size, can_add_syrup, photo) " +
                                    "VALUES (@Name, @Description, @Price, @InStock, @CanChooseSize, @CanAddSyrup, @Photo) RETURNING id";
                        int newProductId;
                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Name", textBox1.Text);
                            command.Parameters.AddWithValue("@Description", textBox2.Text);
                            command.Parameters.AddWithValue("@Price", decimal.Parse(textBox3.Text));
                            command.Parameters.AddWithValue("@InStock", checkBox1.Checked);
                            command.Parameters.AddWithValue("@CanChooseSize", checkBox2.Checked);
                            command.Parameters.AddWithValue("@CanAddSyrup", checkBox3.Checked);
                            byte[] photoBytes = File.ReadAllBytes(selectedImagePath);
                            command.Parameters.AddWithValue("@Photo", photoBytes);
                            newProductId = Convert.ToInt32(command.ExecuteScalar());
                        }

                        // Обновляем связи
                        UpdateAssortmentRelations(newProductId, checkBox2.Checked, checkBox3.Checked);

                        LoadAllProducts();
                        MessageBox.Show("Новый товар добавлен.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления товара: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Кнопка выбора фото для продукта
        private void button4_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedImagePath = openFileDialog.FileName;
                    pictureBox1.Image = Image.FromFile(selectedImagePath);
                }
            }
        }
        // Кнопка удаления фото
        private void button5_Click(object sender, EventArgs e)
        {
            selectedImagePath = null;
            pictureBox1.Image = null;
        }
        // Вызов формы vkladki_a
        private void button8_Click(object sender, EventArgs e)
        {
            // Проверка, открыт ли экземпляр окна навигации
            if (vkladkiForm == null || vkladkiForm.IsDisposed)
            {
                // Если окно не создано или было закрыто, создаём новый экземпляр и показываем его
                vkladkiForm = new vkladki_a(this);
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
        private void button6_Click(object sender, EventArgs e)
        {
            textBox4.Clear();
            textBox4.ForeColor = Color.FromArgb(132, 154, 157);
            textBox4.Text = "ПОИСК";
            LoadAllProducts();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedProductId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["id"].Value);
                using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (NpgsqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Удаление записей из связанных таблиц
                            string deleteWishlistItemsQuery = "DELETE FROM Wishlist WHERE assortment_id = @ProductId";
                            string deleteDesiredProductQuery = "DELETE FROM Desired_Product WHERE assortment_id = @ProductId";
                            string deleteAssortmentSizesQuery = "DELETE FROM Assortment_Sizes WHERE assortment_id = @ProductId";
                            string deleteAssortmentSyrupsQuery = "DELETE FROM Assortment_Syrups WHERE assortment_id = @ProductId";

                            using (NpgsqlCommand cmd = new NpgsqlCommand(deleteWishlistItemsQuery, connection))
                            {
                                cmd.Parameters.AddWithValue("@ProductId", selectedProductId);
                                cmd.ExecuteNonQuery();
                            }

                            using (NpgsqlCommand cmd = new NpgsqlCommand(deleteDesiredProductQuery, connection))
                            {
                                cmd.Parameters.AddWithValue("@ProductId", selectedProductId);
                                cmd.ExecuteNonQuery();
                            }

                            using (NpgsqlCommand cmd = new NpgsqlCommand(deleteAssortmentSizesQuery, connection))
                            {
                                cmd.Parameters.AddWithValue("@ProductId", selectedProductId);
                                cmd.ExecuteNonQuery();
                            }

                            using (NpgsqlCommand cmd = new NpgsqlCommand(deleteAssortmentSyrupsQuery, connection))
                            {
                                cmd.Parameters.AddWithValue("@ProductId", selectedProductId);
                                cmd.ExecuteNonQuery();
                            }

                            // Удаление записи из Assortment
                            string deleteAssortmentQuery = "DELETE FROM Assortment WHERE id = @ProductId";
                            using (NpgsqlCommand cmd = new NpgsqlCommand(deleteAssortmentQuery, connection))
                            {
                                cmd.Parameters.AddWithValue("@ProductId", selectedProductId);
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            MessageBox.Show("Товар успешно удален.");
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show("Ошибка при удалении товара: " + ex.Message);
                        }
                    }
                }
                LoadAllProducts();
            }
        }
        private void LoadAllProducts()
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    var query = "SELECT id, name, description, price FROM Assortment";
                    var adapter = new NpgsqlDataAdapter(query, connection);
                    var productTable = new DataTable();
                    adapter.Fill(productTable);
                    dataGridView1.DataSource = productTable;

                    if (productTable.Rows.Count == 0)
                    {
                        MessageBox.Show("Нет доступных товаров.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    // Настройка ширины столбцов после загрузки данных
                    dataGridView1.Columns["id"].Width = 50;
                    dataGridView1.Columns["name"].Width = 130;
                    dataGridView1.Columns["description"].Width = 310;
                    dataGridView1.Columns["price"].Width = 90;

                    // Скрыть первый столбец (ID)
                    dataGridView1.Columns[0].Visible = false;
                    dataGridView1.RowHeadersVisible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки продуктов: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                selectedProductId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["id"].Value);
                LoadProductDetails(selectedProductId);
                LoadAndDisplayImage(selectedProductId);
            }
        }
        private void LoadProductDetails(int productId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    var query = "SELECT name, description, price, in_stock, can_choose_size, can_add_syrup FROM Assortment WHERE id = @ProductId";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProductId", productId);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                textBox1.Text = reader["name"].ToString();
                                textBox2.Text = reader["description"].ToString();
                                textBox3.Text = reader["price"].ToString();
                                checkBox1.Checked = Convert.ToBoolean(reader["in_stock"]);
                                checkBox2.Checked = Convert.ToBoolean(reader["can_choose_size"]);
                                checkBox3.Checked = Convert.ToBoolean(reader["can_add_syrup"]);
                            }
                            else
                            {
                                MessageBox.Show("Продукт не найден.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки деталей продукта: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadAndDisplayImage(int productId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    var query = "SELECT photo FROM Assortment WHERE id = @ProductId";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProductId", productId);
                        var photoData = command.ExecuteScalar() as byte[];
                        if (photoData != null)
                        {
                            using (var ms = new MemoryStream(photoData))
                            {
                                pictureBox1.Image = Image.FromStream(ms);
                            }
                        }
                        else
                        {
                            pictureBox1.Image = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фото: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void button7_Click(object sender, EventArgs e)
        {
            if (textBox4.Text == "ПОИСК" || string.IsNullOrWhiteSpace(textBox4.Text))
            {
                MessageBox.Show("Введите название для поиска.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            SearchProductsByName();
        }
        private void SearchProductsByName()
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT id, name, description, price FROM Assortment WHERE name ILIKE @Name";
                    using (var adapter = new NpgsqlDataAdapter(query, connection))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@Name", "%" + textBox4.Text + "%");
                        DataTable productTable = new DataTable();
                        adapter.Fill(productTable);
                        dataGridView1.DataSource = productTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void button11_Click(object sender, EventArgs e)
        {
            textBox5.Clear();
            textBox5.Text = "ЦЕНА ОТ...";
            textBox5.ForeColor = Color.FromArgb(132, 154, 157);
            LoadAllProducts();
        }
        private void button9_Click(object sender, EventArgs e)
        {
            if (textBox5.Text == "ЦЕНА ОТ..." || string.IsNullOrWhiteSpace(textBox5.Text))
            {
                MessageBox.Show("Введите минимальную цену для фильтрации.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            FilterProductsByPrice();
        }
        private void FilterProductsByPrice()
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT id, name, description, price FROM Assortment WHERE price >= @MinPrice";
                    using (var adapter = new NpgsqlDataAdapter(query, connection))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@MinPrice", decimal.Parse(textBox5.Text));
                        DataTable productTable = new DataTable();
                        adapter.Fill(productTable);
                        dataGridView1.DataSource = productTable;
                    }
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Введите корректное значение для цены.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void button10_Click(object sender, EventArgs e)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите месяц и год в формате MM/YYYY:",
                "Выбор месяца",
                DateTime.Now.ToString("MM/yyyy"));
            if (!string.IsNullOrEmpty(input))
            {
                try
                {
                    DateTime selectedDate;
                    if (DateTime.TryParseExact(input, "MM/yyyy", null, System.Globalization.DateTimeStyles.None, out selectedDate))
                    {
                        string selectedMonthYear = selectedDate.ToString("yyyy-MM");
                        var saveFileDialog = new SaveFileDialog
                        {
                            Filter = "Excel Files|*.xlsx",
                            Title = "Сохранить отчет",
                            FileName = $"Отчет_{selectedMonthYear}.xlsx"
                        };
                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            using (var connection = new NpgsqlConnection(ConnectionString))
                            {
                                connection.Open();
                                // Запрос для получения итогов
                                var summaryQuery = @"
                            SELECT * 
                            FROM MonthlyOrderReport
                            WHERE order_year::TEXT || '-' || LPAD(order_month::TEXT, 2, '0') = @selectedMonthYear";
                                using (var summaryCommand = new NpgsqlCommand(summaryQuery, connection))
                                {
                                    summaryCommand.Parameters.AddWithValue("@selectedMonthYear", selectedMonthYear);
                                    using (var summaryReader = summaryCommand.ExecuteReader())
                                    {
                                        // Создаем Excel файл
                                        using (var package = new ExcelPackage())
                                        {
                                            var worksheet = package.Workbook.Worksheets.Add("Отчет");
                                            // Заполняем заголовки
                                            worksheet.Cells[1, 1].Value = "Год";
                                            worksheet.Cells[1, 2].Value = "Месяц";
                                            worksheet.Cells[1, 3].Value = "Общее количество заказов";
                                            worksheet.Cells[1, 4].Value = "Общее количество товаров";
                                            worksheet.Cells[1, 5].Value = "Общий доход";
                                            worksheet.Cells[1, 6].Value = "Потраченные баллы";
                                            worksheet.Cells[1, 7].Value = "Заработанные баллы";
                                            int row = 2;
                                            while (summaryReader.Read())
                                            {
                                                worksheet.Cells[row, 1].Value = summaryReader["order_year"];
                                                worksheet.Cells[row, 2].Value = summaryReader["order_month"];
                                                worksheet.Cells[row, 3].Value = summaryReader["total_orders"];
                                                worksheet.Cells[row, 4].Value = summaryReader["total_items_sold"];
                                                worksheet.Cells[row, 5].Value = summaryReader["total_revenue"];
                                                worksheet.Cells[row, 6].Value = summaryReader["total_points_spent"];
                                                worksheet.Cells[row, 7].Value = summaryReader["points_earned"];
                                                row++;
                                            }
                                            // Добавляем границы для итогов
                                            worksheet.Cells[1, 1, row - 1, 7].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                            worksheet.Cells[1, 1, row - 1, 7].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                            worksheet.Cells[1, 1, row - 1, 7].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                            worksheet.Cells[1, 1, row - 1, 7].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                            // Закрытие ридера для итогов
                                            summaryReader.Close();
                                            // Переход к запросу заказов
                                            var ordersQuery = @"
                                       SELECT order_id, user_email, 
                                               order_date, 
                                               order_time,
                                               total_price, quantity, 
                                               points_spent, assortment_name, 
                                               size_name, syrup_names, 
                                               price, status
                                        FROM OrderDetails
                                        WHERE TO_CHAR(order_date, 'YYYY-MM') = @selectedMonthYear
                                        ORDER BY order_date";
                                            using (var ordersCommand = new NpgsqlCommand(ordersQuery, connection))
                                            {
                                                ordersCommand.Parameters.AddWithValue("@selectedMonthYear", selectedMonthYear);
                                                using (var ordersReader = ordersCommand.ExecuteReader())
                                                {
                                                    int orderRow = row + 2;
                                                    worksheet.Cells[orderRow, 1].Value = "ID заказа";
                                                    worksheet.Cells[orderRow, 2].Value = "Email клиента";
                                                    worksheet.Cells[orderRow, 3].Value = "Дата заказа";
                                                    worksheet.Cells[orderRow, 4].Value = "Время заказа";
                                                    worksheet.Cells[orderRow, 5].Value = "Итоговая цена";
                                                    worksheet.Cells[orderRow, 6].Value = "Количество товаров";
                                                    worksheet.Cells[orderRow, 7].Value = "Потраченные баллы";
                                                    worksheet.Cells[orderRow, 8].Value = "Название товара";
                                                    worksheet.Cells[orderRow, 9].Value = "Размер";
                                                    worksheet.Cells[orderRow, 10].Value = "Сиропы"; // Исправлено название столбца
                                                    worksheet.Cells[orderRow, 11].Value = "Цена";
                                                    worksheet.Cells[orderRow, 12].Value = "Статус";
                                                    orderRow++;
                                                    while (ordersReader.Read())
                                                    {
                                                        worksheet.Cells[orderRow, 1].Value = ordersReader["order_id"];
                                                        worksheet.Cells[orderRow, 2].Value = ordersReader["user_email"];

                                                        // Форматируем дату
                                                        DateTime orderDate = Convert.ToDateTime(ordersReader["order_date"]);
                                                        worksheet.Cells[orderRow, 3].Value = orderDate.ToString("yyyy-MM-dd"); // Только дата

                                                        // Форматируем время
                                                        TimeSpan orderTime = (TimeSpan)ordersReader["order_time"];
                                                        worksheet.Cells[orderRow, 4].Value = orderTime.ToString(@"hh\:mm\:ss"); // Только время

                                                        worksheet.Cells[orderRow, 5].Value = ordersReader["total_price"];
                                                        worksheet.Cells[orderRow, 6].Value = ordersReader["quantity"];
                                                        worksheet.Cells[orderRow, 7].Value = ordersReader["points_spent"];
                                                        worksheet.Cells[orderRow, 8].Value = ordersReader["assortment_name"];
                                                        worksheet.Cells[orderRow, 9].Value = ordersReader["size_name"];
                                                        worksheet.Cells[orderRow, 10].Value = ordersReader["syrup_names"];
                                                        worksheet.Cells[orderRow, 11].Value = ordersReader["price"];
                                                        worksheet.Cells[orderRow, 12].Value = ordersReader["status"];
                                                        worksheet.Cells[orderRow, 3].Style.Numberformat.Format = "yyyy-MM-dd";
                                                        worksheet.Cells[orderRow, 4].Style.Numberformat.Format = "hh:mm:ss";
                                                        orderRow++;
                                                    }
                                                }
                                            }
                                            // Сохранение Excel файла
                                            package.SaveAs(new FileInfo(saveFileDialog.FileName));
                                        }
                                    }
                                }
                                MessageBox.Show("Отчет успешно сохранен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Неверный формат даты. Пожалуйста, введите в формате MM/YYYY.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении отчета: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void textBox4_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox4.Text))
            {
                textBox4.Text = "ПОИСК";
            }
        }
        private void textBox4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                button7.PerformClick();
            }
        }
        private void textBox4_Enter(object sender, EventArgs e)
        {
            if (textBox4.Text == "ПОИСК")
            {
                textBox4.Clear();
            }
        }
        private void textBox5_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox5.Text))
            {
                textBox5.Text = "ЦЕНА ОТ...";
            }
        }
        private void textBox5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                button9.PerformClick();
            }
        }
        private void textBox5_Enter(object sender, EventArgs e)
        {
            if (textBox5.Text == "ЦЕНА ОТ...")
            {
                textBox5.Clear();
            }
        }

        private void LoadAllSyrups()
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    var query = "SELECT id, name, price FROM Syrups";
                    var adapter = new NpgsqlDataAdapter(query, connection);
                    var syrupTable = new DataTable();
                    adapter.Fill(syrupTable);
                    dataGridView2.DataSource = syrupTable;

                    // Настройка ширины столбцов
                    dataGridView2.Columns["id"].Width = 50;
                    dataGridView2.Columns["name"].Width = 200;
                    dataGridView2.Columns["price"].Width = 97;

                    // Скрыть первый столбец (ID)
                    dataGridView2.Columns[0].Visible = false;
                    dataGridView2.ReadOnly = true; // Запретить редактирование
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сиропов: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            using (var form = new syrups())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Добавление нового сиропа в базу данных
                        using (var connection = new NpgsqlConnection(ConnectionString))
                        {
                            connection.Open();
                            var insertCommand = new NpgsqlCommand(
                                "INSERT INTO Syrups (name, price) VALUES (@Name, @Price)", connection);
                            insertCommand.Parameters.AddWithValue("@Name", form.SyrupName);
                            insertCommand.Parameters.AddWithValue("@Price", form.SyrupPrice);
                            insertCommand.ExecuteNonQuery();
                        }

                        MessageBox.Show("Сироп успешно добавлен.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadAllSyrups(); // Обновляем список сиропов
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка добавления сиропа: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите сироп для изменения.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int selectedSyrupId = Convert.ToInt32(dataGridView2.SelectedRows[0].Cells["id"].Value);
            string currentName = dataGridView2.SelectedRows[0].Cells["name"].Value.ToString();
            decimal currentPrice = Convert.ToDecimal(dataGridView2.SelectedRows[0].Cells["price"].Value);

            using (var form = new syrups(currentName, currentPrice))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Обновление данных сиропа в базе данных
                        using (var connection = new NpgsqlConnection(ConnectionString))
                        {
                            connection.Open();
                            var updateCommand = new NpgsqlCommand(
                                "UPDATE Syrups SET name = @Name, price = @Price WHERE id = @Id", connection);
                            updateCommand.Parameters.AddWithValue("@Name", form.SyrupName);
                            updateCommand.Parameters.AddWithValue("@Price", form.SyrupPrice);
                            updateCommand.Parameters.AddWithValue("@Id", selectedSyrupId);
                            updateCommand.ExecuteNonQuery();
                        }

                        MessageBox.Show("Сироп успешно изменен.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadAllSyrups(); // Обновляем список сиропов
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка изменения сиропа: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void button12_Click(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите сироп для удаления.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int selectedSyrupId = Convert.ToInt32(dataGridView2.SelectedRows[0].Cells["id"].Value);

            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Удаление связей
                            var deleteRelationsQuery = "DELETE FROM Assortment_Syrups WHERE syrup_id = @SyrupId";
                            using (var deleteRelationsCommand = new NpgsqlCommand(deleteRelationsQuery, connection))
                            {
                                deleteRelationsCommand.Parameters.AddWithValue("@SyrupId", selectedSyrupId);
                                deleteRelationsCommand.ExecuteNonQuery();
                            }

                            // Удаление сиропа
                            var deleteSyrupQuery = "DELETE FROM Syrups WHERE id = @SyrupId";
                            using (var deleteSyrupCommand = new NpgsqlCommand(deleteSyrupQuery, connection))
                            {
                                deleteSyrupCommand.Parameters.AddWithValue("@SyrupId", selectedSyrupId);
                                deleteSyrupCommand.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            MessageBox.Show("Сироп успешно удален.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadAllSyrups(); // Обновление списка сиропов
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"Ошибка удаления сиропа: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления сиропа: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void admin_menu_Load(object sender, EventArgs e)
        {

        }
    }
}
