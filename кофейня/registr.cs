using Npgsql;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Mail;
[assembly: InternalsVisibleTo("TestProjectCoffeeShop")]

namespace кофейня
{
    public partial class registr : Form
    {
        private const string ConnectionString = "Host=localhost;Database=coffee_db;Username=postgres;Password=pwd";

        public registr()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);
            
            maskedTextBox2.Text = "Телефон";
            maskedTextBox3.Text = "Дата рождения";

            //настройка поля ввода пароля
            textBox1.Text = "Пароль";
            textBox1.PasswordChar = '\0';            
        }
        // Обработчики событий KeyDown для перемещения фокуса на следующее поле ввода при нажатии Enter
        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                maskedTextBox1.Focus();
            }
        }
        private void maskedTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                maskedTextBox2.Focus();
            }
        }
        private void maskedTextBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                maskedTextBox3.Focus();
            }
        }
        private void maskedTextBox3_KeyDown(object sender, KeyEventArgs e)
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
                button1.PerformClick();
            }
        }
        private async Task SendVerificationEmailAsync(string toEmail, string verificationCode)
        {
            try
            {
                string smtpServer = "smtp.mail.ru";
                int smtpPort = 587;
                string smtpUsername = "test_coffee13@mail.ru"; // Email отправителя
                string smtpPassword = "iqAv1Kfkttji5952SGfq"; // Пароль для внешних приложений

                using (MailMessage mail = new MailMessage())
                using (SmtpClient smtp = new SmtpClient(smtpServer, smtpPort))
                {
                    mail.From = new MailAddress(smtpUsername, "Кофейня");
                    mail.To.Add(toEmail);
                    mail.Subject = "Добро пожаловать в кофейню!";
                    mail.Body = $"Ваш код подтверждения: {verificationCode}";
                    mail.IsBodyHtml = false;

                    smtp.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    smtp.EnableSsl = true;

                    await smtp.SendMailAsync(mail);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке письма: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private async void button1_Click(object sender, EventArgs e)
        {
            var errorMessage = new StringBuilder();

            string fullName = richTextBox1.Text.Trim();
            string[] names = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            string userLastname = names.Length > 0 ? names[0] : string.Empty;
            string userName = names.Length > 1 ? names[1] : string.Empty;

            if (string.IsNullOrWhiteSpace(userLastname) || string.IsNullOrWhiteSpace(userName))
            {
                errorMessage.AppendLine("Введите фамилию и имя (через пробел).");
            }

            string userEmail = maskedTextBox1.Text;
            string userPhone = maskedTextBox2.Text;
            string userBirthday = maskedTextBox3.Text;
            string userPassword = textBox1.Text;

            if (string.IsNullOrWhiteSpace(userEmail) || string.IsNullOrWhiteSpace(userPhone) ||
                string.IsNullOrWhiteSpace(userPassword) || userBirthday == "Дата рождения")
            {
                errorMessage.AppendLine("Заполните все поля перед регистрацией.");
            }

            if (!Regex.IsMatch(userEmail, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                errorMessage.AppendLine("Некорректный email.");
            }

            if (!Regex.IsMatch(userPhone, @"^8\(\d{3}\)\d{3}-\d{2}-\d{2}$"))
            {
                errorMessage.AppendLine("Некорректный номер телефона.");
            }

            if (!DateTime.TryParse(userBirthday, out DateTime birthday) || birthday > DateTime.Now)
            {
                errorMessage.AppendLine("Введите корректную дату рождения.");
            }

            if (userPassword.Length < 8 || !Regex.IsMatch(userPassword, @"^(?=.*[a-zA-Z])(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?!.*[А-Яа-яЁё]).{8,}$"))
            {
                errorMessage.AppendLine("Пароль должен содержать не менее 8 символов, включая цифры, строчные и заглавные латинские буквы.");
            }

            if (errorMessage.Length > 0)
            {
                MessageBox.Show(errorMessage.ToString(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Генерируем код подтверждения
            string verificationCode = GenerateVerificationCode();

            // Отправляем код на email в отдельном потоке
            await Task.Run(() => SendVerificationEmailAsync(userEmail, verificationCode));

            // Использование созданной формы verificationForm
            using (var verificationForm = new coffee.verificationForm())
            {
                verificationForm.Code = verificationCode; // Передаем код подтверждения

                var result = verificationForm.ShowDialog();

                if (result == DialogResult.Cancel)
                {
                    MessageBox.Show("Регистрация отменена.", "Отмена", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            // Сохраняем пользователя в БД
            int newUserId = await SaveUserToDatabaseAsync(userName, userLastname, userEmail, userPhone, birthday, userPassword);

            if (newUserId > 0)
            {
                MessageBox.Show("Регистрация завершена! Добро пожаловать!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                menu form2 = new menu(newUserId, 2);
                this.Hide();
                form2.Closed += (s, args) => this.Close();
                form2.ShowDialog();
                this.Close();
            }
            else
            {
                MessageBox.Show("Ошибка при сохранении данных.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<int> SaveUserToDatabaseAsync(string userName, string userLastname, string userEmail, string userPhone,
    DateTime userBirthday, string userPassword)
        {
            string hashedPassword = HashPassword(userPassword);

            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    using (var transaction = await connection.BeginTransactionAsync()) // Начинаем транзакцию
                    {
                        try
                        {
                            // Проверка на существование email
                            string checkEmailQuery = "SELECT COUNT(*) FROM Users WHERE email = @userEmail";
                            using (NpgsqlCommand checkCommand = new NpgsqlCommand(checkEmailQuery, connection, transaction))
                            {
                                checkCommand.Parameters.AddWithValue("@userEmail", userEmail);
                                int emailCount = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

                                if (emailCount > 0)
                                {
                                    MessageBox.Show("Почта уже занята. Попробуйте войти или используйте другой адрес электронной почты.",
                                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return -1;
                                }
                            }

                            // Вставка нового пользователя
                            string query = "INSERT INTO Users (first_name, last_name, email, password, phone, birth_date, role_id) " +
                                           "VALUES (@userName, @userLastname, @userEmail, @userPassword, @userPhone, @userBirthday, 2) " +
                                           "RETURNING id";
                            int newUserId;
                            using (NpgsqlCommand command = new NpgsqlCommand(query, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@userLastname", userLastname);
                                command.Parameters.AddWithValue("@userName", userName);
                                command.Parameters.AddWithValue("@userEmail", userEmail);
                                command.Parameters.AddWithValue("@userPassword", hashedPassword);
                                command.Parameters.AddWithValue("@userPhone", userPhone);
                                command.Parameters.AddWithValue("@userBirthday", userBirthday);

                                newUserId = Convert.ToInt32(await command.ExecuteScalarAsync());
                            }

                            // Вставка бонусных 200 баллов
                            string insertTransactionQuery = "INSERT INTO Points_Transactions (user_id, transaction_type, points, transaction_date) " +
                                                            "VALUES (@userId, 'earn', 200, @transactionDate)";
                            using (NpgsqlCommand transactionCommand = new NpgsqlCommand(insertTransactionQuery, connection, transaction))
                            {
                                transactionCommand.Parameters.AddWithValue("@userId", newUserId);
                                transactionCommand.Parameters.AddWithValue("@transactionDate", DateTime.UtcNow);

                                await transactionCommand.ExecuteNonQueryAsync();
                            }

                            // Подтверждение транзакции
                            await transaction.CommitAsync();

                            return newUserId;
                        }
                        catch (Exception)
                        {
                            await transaction.RollbackAsync(); // Откат в случае ошибки
                            throw;
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return -1;
                }
            }
        }
        private string GenerateVerificationCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        internal string HashPassword(string password) // хэширование пароля
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            vhod form1 = new vhod();
            form1.Closed += (s, args) => this.Close();
            form1.Show();
        }
        private void richTextBox1_Enter(object sender, EventArgs e)
        {
            if (richTextBox1.Text == "Фамилия Имя")
            {
                richTextBox1.Clear();
            }
        }
        private void richTextBox1_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(richTextBox1.Text))
            {
                richTextBox1.Text = "Фамилия Имя";
            }
        }
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
        private void maskedTextBox2_Enter(object sender, EventArgs e)
        {
            if (maskedTextBox2.Text == "Телефон")
            {
                maskedTextBox2.Clear();
                maskedTextBox2.Mask = "8(000)000-00-00";
            }
        }
        private void maskedTextBox3_Enter(object sender, EventArgs e)
        {
            if (maskedTextBox3.Text == "Дата рождения")
            {
                maskedTextBox3.Clear();
                maskedTextBox3.Mask = "0000/00/00";
            }
        }
    }
}