using coffee;
using Npgsql;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Tulpep.NotificationWindow;

namespace кофейня
{
    public partial class vhod : Form
    {
        private const string ConnectionString = "Host=localhost;Database=coffee_db;Username=postgres;Password=pwd";
        private string user_email;
        private int user_id;

        public vhod()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);

            // Настройка поля ввода пароля
            textBox1.PasswordChar = '*';
            textBox1.Text = "Пароль";
            textBox1.PasswordChar = '\0';
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

        private async void button2_Click(object sender, EventArgs e)
        {
            string enteredEmail = maskedTextBox1.Text.Trim();
            string enteredPassword = textBox1.Text;

            if (!Regex.IsMatch(enteredEmail, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                MessageBox.Show("Некорректный email.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int roleId = ValidateLogin(enteredEmail, enteredPassword);
            if (roleId != -1)
            {
                user_email = enteredEmail;
                user_id = GetUserId(enteredEmail);

                // Проверяем баллы при входе
                await CheckPointsExpirationForCurrentUser(user_id);

                // Отправляем приветственное письмо
                await SendWelcomeEmailAsync(user_email);

                // Открываем соответствующую форму
                OpenMenuForm(roleId);
            }
            else
            {
                MessageBox.Show("Неверный email или пароль.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task CheckPointsExpirationForCurrentUser(int userId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                string query = @"
            SELECT points, transaction_date 
            FROM Points_Transactions 
            WHERE user_id = @UserId AND transaction_type = 'earn'
            ORDER BY transaction_date ASC";
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        List<string> expiringPointsMessages = new List<string>();

                        while (await reader.ReadAsync())
                        {
                            int points = reader.GetInt32(0);
                            DateTime earnedDate = reader.GetDateTime(1);
                            DateTime expirationDate = earnedDate.AddMonths(2);

                            if (expirationDate <= DateTime.Now)
                            {
                                // Баллы уже просрочены
                                continue;
                            }
                            else if ((expirationDate - DateTime.Now).Days <= 10)
                            {
                                // Собираем информацию о баллах, которые скоро истекут
                                int daysLeft = (expirationDate - DateTime.Now).Days;
                                string message = $"{points} баллов истекают через {daysLeft} дней ({expirationDate.ToShortDateString()})";
                                expiringPointsMessages.Add(message);
                            }
                        }

                        // Если есть баллы, которые скоро истекут, показываем одно уведомление
                        if (expiringPointsMessages.Count > 0)
                        {
                            string fullMessage = "Скоро истекут следующие баллы:\n" + string.Join("\n", expiringPointsMessages);
                            ShowPopupNotification(fullMessage);
                        }
                    }
                }
            }
        }
        private int ValidateLogin(string email, string password)
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
                                user_id = reader.GetInt32(0);
                                string hashedPasswordFromDB = reader.GetString(1);
                                int roleId = reader.GetInt32(2);
                                string hashedPassword = HashPassword(password);

                                return hashedPasswordFromDB == hashedPassword ? roleId : -1;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка подключения к БД: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return -1;
        }

        private int GetUserId(string email)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT id FROM Users WHERE email = @Email";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка получения ID пользователя: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return -1;
                }
            }
        }

        private async Task SendWelcomeEmailAsync(string toEmail)
        {
            try
            {
                string smtpServer = "smtp.mail.ru";
                int smtpPort = 587;
                string smtpUsername = "test_coffee13@mail.ru";
                string smtpPassword = "iqAv1Kfkttji5952SGfq";

                using (MailMessage mail = new MailMessage())
                using (SmtpClient smtp = new SmtpClient(smtpServer, smtpPort))
                {
                    mail.From = new MailAddress(smtpUsername, "Кофейня");
                    mail.To.Add(toEmail);
                    mail.Subject = "Добро пожаловать в кофейню!";
                    mail.Body = "Приветствуем вас снова в нашей кофейне! Желаем вам приятных покупок и отличного настроения! ☕";
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

        private void OpenMenuForm(int userRoleId)
        {
            this.Hide();
            Form formToOpen;

            switch (userRoleId)
            {
                case 0:
                    formToOpen = new admin_menu();
                    break;
                case 1:
                    formToOpen = new barista_zakaz(user_id);
                    break;
                default:
                    formToOpen = new menu(user_id, userRoleId);
                    break;
            }

            formToOpen.Closed += (s, args) => this.Close();
            formToOpen.Show();
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
            string guestEmail = "g@gmail.com";
            string guestPassword = "000";

            if (ValidateLogin(guestEmail, guestPassword) != -1)
            {
                user_email = guestEmail;
                user_id = GetUserId(guestEmail);
                OpenMenuForm(3); // 3 - роль гостя
            }
            else
            {
                MessageBox.Show("Не удается войти как гость.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void ShowPopupNotification(string message)
        {
            PopupNotifier popup = new PopupNotifier
            {
                TitleText = "Важное напоминание",
                ContentText = message,
                Delay = 5000 // 5 секунд
            };
            popup.Popup();
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            using (var emailForm = new email())
            {
                emailForm.SetMode(email.EmailFormMode.EnterEmail); // Устанавливаем режим ввода email
                if (emailForm.ShowDialog() == DialogResult.OK)
                {
                    using (var verificationFormInstance = new verificationForm())
                    {
                        var tagData = emailForm.Tag as dynamic;
                        if (tagData != null && tagData.Code != null)
                        {
                            verificationFormInstance.Code = tagData.Code;
                        }

                        if (verificationFormInstance.ShowDialog() == DialogResult.OK)
                        {
                            using (var newPasswordForm = new email())
                            {
                                newPasswordForm.Tag = emailForm.Tag; // Передаем Tag с email и кодом
                                newPasswordForm.SetMode(email.EmailFormMode.EnterNewPassword);
                                if (newPasswordForm.ShowDialog() == DialogResult.OK)
                                {
                                    MessageBox.Show("Пароль успешно изменен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    // Обновляем данные пользователя
                                    string updatedEmail = (string)((dynamic)emailForm.Tag).Email;
                                    int updatedUserId = GetUserId(updatedEmail);

                                    if (updatedUserId > 0)
                                    {
                                        user_id = updatedUserId; // Обновляем глобальный user_id
                                        user_email = updatedEmail; // Обновляем глобальный user_email

                                        // Открываем меню для пользователя
                                        OpenMenuForm(2); // Пример: роль пользователь
                                    }
                                    else
                                    {
                                        MessageBox.Show("Не удалось найти пользователя.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
