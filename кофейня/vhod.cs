using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Npgsql;

namespace кофейня
{
    public partial class vhod : Form
    {
        private const string ConnectionString = "Host=172.20.7.6;Database=krezhowa_coffee;Username=st;Password=pwd";
        private string user_email;
        private int user_id;

        public vhod()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);
            //настройка поля ввода пароля
            textBox1.PasswordChar = '*';
            textBox1.Text = "Пароль";
            textBox1.PasswordChar = '\0';

            // Обработчики событий KeyDown для перемещения фокуса
            maskedTextBox1.KeyDown += maskedTextBox1_KeyDown;
            textBox1.KeyDown += textBox1_KeyDown;
        }

        private void maskedTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                textBox1.Focus();
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                button2.PerformClick();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            registr form1 = new registr();
            form1.Closed += (s, args) => this.Close();
            form1.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string enteredEmail = maskedTextBox1.Text;
            string enteredPassword = textBox1.Text;

            if (!Regex.IsMatch(enteredEmail, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                MessageBox.Show("Некорректный email.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (ValidateLogin(enteredEmail, enteredPassword))
            {
                user_email = enteredEmail;
                OpenMenuForm();
            }
            else
            {
                MessageBox.Show("Неверный пароль.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateLogin(string email, string password)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();

                    string query = "SELECT id, password, role_id FROM Users WHERE email = @Email";
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user_id = reader.GetInt32(0); // Получаем ID пользователя
                                string hashedPasswordFromDB = reader.GetString(1);
                                string hashedPassword = HashPassword(password);
                                return hashedPasswordFromDB == hashedPassword;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не удается подключиться к базе данных.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            return false;
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private void OpenMenuForm()
        {
            int userRoleId = GetUserRole(user_email);

            this.Hide();
            Form formToOpen;

            // Определяем, какую форму открывать в зависимости от роли
            switch (userRoleId)
            {
                case 0:
                    formToOpen = new admin_menu();
                    break;
                case 1:
                    formToOpen = new barista_zakaz(user_id);
                    break;
                case 2:
                case 3:
                default:
                    formToOpen = new menu(user_id);
                    break;
            }

            formToOpen.Closed += (s, args) => this.Close();
            formToOpen.Show();
        }

        private int GetUserRole(string email)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();

                    string query = "SELECT role_id FROM Users WHERE email = @Email";
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        return (int)command.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не удается подключиться к базе данных.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return -1;
                }
            }
        }

        // Добавим логику для исчезновения и появления текста в полях
        private void maskedTextBox1_Enter(object sender, EventArgs e)
        {
            if (maskedTextBox1.Text == "E-Mail")
            {
                maskedTextBox1.Clear();
            }
        }

        private void maskedTextBox1_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(maskedTextBox1.Text))
            {
                maskedTextBox1.Text = "E-Mail";
            }
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (textBox1.Text == "Пароль")
            {
                textBox1.Clear();
                textBox1.PasswordChar = '*';
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                textBox1.Text = "Пароль";
                textBox1.PasswordChar = '\0';
            }
        }
    }
}
