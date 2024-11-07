using System.Data;
using Microsoft.VisualBasic.ApplicationServices;
using Npgsql;
using OfficeOpenXml;
using кофейня.admin;
using кофейня.polzovatel;

namespace кофейня
{
    public partial class admin_menu : Form
    {
        private const string ConnectionString = "Host=172.20.7.6;Database=krezhowa_coffee;Username=st;Password=pwd";
        private string selectedImagePath = null;
        private int selectedProductId = -1;
        private vkladki_a vkladkiForm;
        private PrintDialog printDialog;

        public admin_menu()
        {
            InitializeComponent();
            SetupDataGridView();
            LoadAllProducts();
            printDialog = new PrintDialog();
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);           
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
        }

        // Обработчик для кнопки сохранения изменений
        private void button1_Click(object sender, EventArgs e)
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

        private void UpdateAssortmentRelations(int productId, bool canChooseSize, bool canAddSyrup)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();

                    // Обязательные связи для обычного размера и сиропа
                    var ensureDefaultSizeQuery = "INSERT INTO Assortment_Sizes (assortment_id, size_id) VALUES (@ProductId, 1) ON CONFLICT DO NOTHING";
                    var ensureDefaultSyrupQuery = "INSERT INTO Assortment_Syrups (assortment_id, syrup_id) VALUES (@ProductId, 2) ON CONFLICT DO NOTHING";

                    using (var ensureDefaultSizeCommand = new NpgsqlCommand(ensureDefaultSizeQuery, connection))
                    {
                        ensureDefaultSizeCommand.Parameters.AddWithValue("@ProductId", productId);
                        ensureDefaultSizeCommand.ExecuteNonQuery();
                    }

                    using (var ensureDefaultSyrupCommand = new NpgsqlCommand(ensureDefaultSyrupQuery, connection))
                    {
                        ensureDefaultSyrupCommand.Parameters.AddWithValue("@ProductId", productId);
                        ensureDefaultSyrupCommand.ExecuteNonQuery();
                    }

                    // Обновление дополнительных связей для размеров
                    if (canChooseSize)
                    {
                        var deleteSizesQuery = "DELETE FROM Assortment_Sizes WHERE assortment_id = @ProductId AND size_id <> 1";
                        using (var deleteSizesCommand = new NpgsqlCommand(deleteSizesQuery, connection))
                        {
                            deleteSizesCommand.Parameters.AddWithValue("@ProductId", productId);
                            deleteSizesCommand.ExecuteNonQuery();
                        }

                        var insertAllSizesQuery = "INSERT INTO Assortment_Sizes (assortment_id, size_id) " +
                                                  "SELECT @ProductId, id FROM Sizes WHERE id <> 1 ON CONFLICT DO NOTHING";
                        using (var insertSizesCommand = new NpgsqlCommand(insertAllSizesQuery, connection))
                        {
                            insertSizesCommand.Parameters.AddWithValue("@ProductId", productId);
                            insertSizesCommand.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // Удаляем все, кроме обязательного размера
                        var deleteAllSizesQuery = "DELETE FROM Assortment_Sizes WHERE assortment_id = @ProductId AND size_id <> 1";
                        using (var deleteAllSizesCommand = new NpgsqlCommand(deleteAllSizesQuery, connection))
                        {
                            deleteAllSizesCommand.Parameters.AddWithValue("@ProductId", productId);
                            deleteAllSizesCommand.ExecuteNonQuery();
                        }
                    }

                    // Обновление дополнительных связей для сиропов
                    if (canAddSyrup)
                    {
                        var deleteSyrupsQuery = "DELETE FROM Assortment_Syrups WHERE assortment_id = @ProductId AND syrup_id <> 2";
                        using (var deleteSyrupsCommand = new NpgsqlCommand(deleteSyrupsQuery, connection))
                        {
                            deleteSyrupsCommand.Parameters.AddWithValue("@ProductId", productId);
                            deleteSyrupsCommand.ExecuteNonQuery();
                        }

                        var insertAllSyrupsQuery = "INSERT INTO Assortment_Syrups (assortment_id, syrup_id) " +
                                                   "SELECT @ProductId, id FROM Syrups WHERE id <> 2 ON CONFLICT DO NOTHING";
                        using (var insertSyrupsCommand = new NpgsqlCommand(insertAllSyrupsQuery, connection))
                        {
                            insertSyrupsCommand.Parameters.AddWithValue("@ProductId", productId);
                            insertSyrupsCommand.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // Удаляем все, кроме обязательного сиропа
                        var deleteAllSyrupsQuery = "DELETE FROM Assortment_Syrups WHERE assortment_id = @ProductId AND syrup_id <> 2";
                        using (var deleteAllSyrupsCommand = new NpgsqlCommand(deleteAllSyrupsQuery, connection))
                        {
                            deleteAllSyrupsCommand.Parameters.AddWithValue("@ProductId", productId);
                            deleteAllSyrupsCommand.ExecuteNonQuery();
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
                if (selectedProductId != -1)
                {
                    // Обновление существующего товара
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

                        // Добавление параметра фото только при наличии пути
                        if (!string.IsNullOrEmpty(selectedImagePath))
                        {
                            byte[] photoBytes = File.ReadAllBytes(selectedImagePath);
                            command.Parameters.AddWithValue("@Photo", photoBytes);
                        }

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

                        if (!string.IsNullOrEmpty(selectedImagePath))
                        {
                            byte[] photoBytes = File.ReadAllBytes(selectedImagePath);
                            command.Parameters.AddWithValue("@Photo", photoBytes);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@Photo", DBNull.Value);
                        }

                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
        }


        // Проверка введенных данных
        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) ||
                string.IsNullOrWhiteSpace(textBox2.Text) ||
                !decimal.TryParse(textBox3.Text, out _))
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

                        // Добавление нового товара
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

                            if (!string.IsNullOrEmpty(selectedImagePath))
                            {
                                byte[] photoBytes = File.ReadAllBytes(selectedImagePath);
                                command.Parameters.AddWithValue("@Photo", photoBytes);
                            }
                            else
                            {
                                command.Parameters.AddWithValue("@Photo", DBNull.Value);
                            }

                            // Получение ID нового товара
                            newProductId = Convert.ToInt32(command.ExecuteScalar());
                        }

                        // Вызов универсального метода для обновления/создания связей
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
                            string deleteWishlistItemsQuery = "DELETE FROM Wishlist_Items WHERE assortment_id = @ProductId";
                            string deleteDesiredProductQuery = "DELETE FROM DesiredProduct WHERE assortment_id = @ProductId";
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
                        MessageBox.Show("Нет доступных продуктов.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    // Настройка ширины столбцов после загрузки данных
                    dataGridView1.Columns["id"].Width = 50;
                    dataGridView1.Columns["name"].Width = 150;
                    dataGridView1.Columns["description"].Width = 250;
                    dataGridView1.Columns["price"].Width = 90;

                    // Скрыть первый столбец (ID)
                    dataGridView1.Columns[0].Visible = false;
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
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "Сохранить отчет",
                FileName = $"Отчет_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var connection = new NpgsqlConnection(ConnectionString))
                    {
                        connection.Open();

                        // Запрос к представлению для получения данных
                        var query = "SELECT * FROM MonthlyOrderReport";
                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            using (var reader = command.ExecuteReader())
                            {
                                // Создаем Excel пакет
                                using (var package = new ExcelPackage())
                                {
                                    var worksheet = package.Workbook.Worksheets.Add("Отчет");

                                    // Заполнение заголовков
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        worksheet.Cells[1, i + 1].Value = reader.GetName(i);
                                        worksheet.Cells[1, i + 1].Style.Font.Bold = true; // Жирный шрифт для заголовков
                                    }

                                    // Заполнение данных
                                    int row = 2; // Начинаем с первой строки под заголовками
                                    while (reader.Read())
                                    {
                                        for (int i = 0; i < reader.FieldCount; i++)
                                        {
                                            worksheet.Cells[row, i + 1].Value = reader[i];
                                        }
                                        row++;
                                    }

                                    // Сохранение файла
                                    FileInfo excelFile = new FileInfo(saveFileDialog.FileName);
                                    package.SaveAs(excelFile);
                                }
                            }
                        }
                    }

                    MessageBox.Show("Отчет успешно сохранен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
    }
}
