using System.Data;
using System.Text;
using Npgsql;
using System.Security.Cryptography;

namespace кофейня.admin
{
    public partial class upr_barista : Form
    {
        private const string ConnectionString = "Host=localhost;Database=coffee_db;Username=postgres;Password=pwd";
        private vkladki_a vkladkiForm;

        public upr_barista()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);
            maskedTextBox3.Mask = "0000/00/00";
            maskedTextBox2.Mask = "8(999)000-00-00";
            textBox2.PasswordChar = '*';
            ConfigureDataGridView();
            LoadBaristas();
        }

        private void ConfigureDataGridView()
        {
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.FromArgb(245, 226, 218);
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;
            dataGridView1.DefaultCellStyle.Font = new Font("Roboto", 14);
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
        }
        private void LoadBaristas()
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var query = "SELECT id, first_name, last_name, email, phone, birth_date FROM Users WHERE role_id = 1";
                var adapter = new NpgsqlDataAdapter(query, connection);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);
                dataGridView1.DataSource = dataTable;

                dataGridView1.Columns["first_name"].Width = 120;
                dataGridView1.Columns["last_name"].Width = 110;
                dataGridView1.Columns["email"].Width = 175;
                dataGridView1.Columns["phone"].Width = 100;
                dataGridView1.Columns["birth_date"].Width = 115;

                dataGridView1.Columns[0].Visible = false;
                dataGridView1.RowHeadersVisible = false;
            }
        }
        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var row = dataGridView1.SelectedRows[0];
                textBox1.Text = $"{row.Cells["last_name"].Value} {row.Cells["first_name"].Value}";
                maskedTextBox1.Text = row.Cells["email"].Value.ToString();
                maskedTextBox2.Text = row.Cells["phone"].Value.ToString();
                maskedTextBox3.Text = Convert.ToDateTime(row.Cells["birth_date"].Value).ToString("yyyy.MM.dd");
                textBox2.Clear();
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
            {
                return;
            }

            if (EmailExists(maskedTextBox1.Text, (int)dataGridView1.SelectedRows[0].Cells["id"].Value))
            {
                MessageBox.Show("Пользователь с данной почтой уже существует.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var selectedId = (int)dataGridView1.SelectedRows[0].Cells["id"].Value;
                var hashedPassword = !string.IsNullOrEmpty(textBox2.Text) ? HashPassword(textBox2.Text) : null;
                var query = hashedPassword != null
                    ? $"UPDATE Users SET last_name=@lastName, first_name=@firstName, email=@Email, phone=@Phone, birth_date=@BirthDate, password=@Password WHERE id=@id"
                    : $"UPDATE Users SET last_name=@lastName, first_name=@firstName, email=@Email, phone=@Phone, birth_date=@BirthDate WHERE id=@id";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    var names = textBox1.Text.Split(' ');
                    command.Parameters.AddWithValue("@lastName", names[0]);
                    command.Parameters.AddWithValue("@firstName", names.Length > 1 ? names[1] : "");
                    command.Parameters.AddWithValue("@Email", maskedTextBox1.Text);
                    command.Parameters.AddWithValue("@Phone", maskedTextBox2.Text);
                    command.Parameters.AddWithValue("@BirthDate", DateTime.Parse(maskedTextBox3.Text));
                    command.Parameters.AddWithValue("@id", selectedId);
                    if (hashedPassword != null)
                        command.Parameters.AddWithValue("@Password", hashedPassword);

                    command.ExecuteNonQuery();
                }
            }
            LoadBaristas();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
            {
                return;
            }

            if (EmailExists(maskedTextBox1.Text, null))
            {
                MessageBox.Show("Пользователь с данной почтой уже существует.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var query = "INSERT INTO Users (role_id, first_name, last_name, email, password, phone, birth_date) VALUES (1, @firstName, @lastName, @Email, @Password, @Phone, @BirthDate)";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    var names = textBox1.Text.Split(' ');
                    command.Parameters.AddWithValue("@lastName", names[0]);
                    command.Parameters.AddWithValue("@firstName", names.Length > 1 ? names[1] : "");
                    command.Parameters.AddWithValue("@Email", maskedTextBox1.Text);
                    command.Parameters.AddWithValue("@Phone", maskedTextBox2.Text);
                    command.Parameters.AddWithValue("@BirthDate", DateTime.Parse(maskedTextBox3.Text));
                    command.Parameters.AddWithValue("@Password", HashPassword(textBox2.Text));

                    command.ExecuteNonQuery();
                }
            }
            LoadBaristas();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите пользователя для удаления.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var confirmResult = MessageBox.Show("Вы уверены, что хотите удалить данного пользователя?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmResult == DialogResult.No) return;

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var selectedId = (int)dataGridView1.SelectedRows[0].Cells["id"].Value;
                var query = "DELETE FROM Users WHERE id=@id";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", selectedId);
                    command.ExecuteNonQuery();
                }
            }
            LoadBaristas();
        }
        private bool ValidateInputs()
        {
            var errorMessage = new StringBuilder();

            // Проверка, что текстовое поле не пустое
            if (string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrEmpty(maskedTextBox1.Text) ||
                string.IsNullOrEmpty(maskedTextBox2.Text) || string.IsNullOrEmpty(maskedTextBox3.Text))
            {
                errorMessage.AppendLine("Все поля должны быть заполнены.");
            }

            // Проверка, что имя и фамилия введены через пробел
            var names = textBox1.Text.Split(' ');
            if (names.Length < 2)
            {
                errorMessage.AppendLine("Введите фамилию и имя (через пробел).");
            }

            // Проверка даты рождения
            if (!DateTime.TryParse(maskedTextBox3.Text, out DateTime birthDate) || birthDate > DateTime.Now)
            {
                errorMessage.AppendLine("Некорректная дата рождения.");
            }

            // Проверка email
            if (!IsValidEmail(maskedTextBox1.Text))
            {
                errorMessage.AppendLine("Некорректный формат электронной почты.");
            }

            // Проверка телефона
            if (!System.Text.RegularExpressions.Regex.IsMatch(maskedTextBox2.Text, @"^8\(\d{3}\)\d{3}-\d{2}-\d{2}$"))
            {
                errorMessage.AppendLine("Некорректный формат телефона.");
            }

            // Проверка пароля
            if (!string.IsNullOrEmpty(textBox2.Text))
            {
                if (textBox2.Text.Length < 8)
                {
                    errorMessage.AppendLine("Пароль должен содержать не менее 8 символов.");
                }

                // Проверка на наличие цифры, маленькой и большой латинской буквы и отсутствие русских букв
                if (!System.Text.RegularExpressions.Regex.IsMatch(textBox2.Text, @"^(?=.*[a-zA-Z])(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?!.*[А-Яа-яЁё]).{8,}$"))
                {
                    errorMessage.AppendLine("Пароль должен содержать цифры, строчные и заглавные латинские буквы.");
                }
            }


            if (errorMessage.Length > 0)
            {
                MessageBox.Show(errorMessage.ToString(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }
        private bool IsValidEmail(string email)
        {
            try
            {
                var mail = new System.Net.Mail.MailAddress(email);
                return mail.Address == email;
            }
            catch
            {
                return false;
            }
        }
        private bool EmailExists(string email, int? userId)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                string query;

                if (userId.HasValue)
                {
                    query = "SELECT COUNT(*) FROM Users WHERE email = @Email AND id <> @Id";
                }
                else
                {
                    query = "SELECT COUNT(*) FROM Users WHERE email = @Email";
                }

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);

                    if (userId.HasValue)
                    {
                        command.Parameters.AddWithValue("@Id", userId.Value);
                    }

                    return (long)command.ExecuteScalar() > 0;
                }
            }
        }
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (vkladkiForm == null || vkladkiForm.IsDisposed)
            {
                vkladkiForm = new vkladki_a(this);
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
        }
    }
}
