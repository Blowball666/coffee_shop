using Npgsql;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Security.Cryptography;

namespace кофейня
{
    public partial class email : Form
    {
        private const string ConnectionString = "Host=localhost;Database=coffee_db;Username=postgres;Password=pwd";
        public enum EmailFormMode
        {
            EnterEmail,
            EnterNewPassword
        }

        public EmailFormMode Mode { get; set; } = EmailFormMode.EnterEmail;

        public string UserEmail { get; private set; } // Сохраняем email пользователя

        public email()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private async void buttonConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                if (Mode == EmailFormMode.EnterEmail)
                {
                    // Режим ввода email
                    string userEmail = textBox1.Text.Trim();

                    if (!IsValidEmail(userEmail))
                    {
                        MessageBox.Show("Некорректный email.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (!await CheckUserExistsAsync(userEmail))
                    {
                        MessageBox.Show("Пользователь с таким email не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Генерация и отправка кода подтверждения
                    string verificationCode = GenerateVerificationCode();
                    await SendVerificationCodeAsync(userEmail, verificationCode);

                    // Сохраняем email и код для последующей проверки
                    UserEmail = userEmail;
                    this.Tag = new { Email = userEmail, Code = verificationCode };

                    MessageBox.Show("Код подтверждения отправлен на вашу почту.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else if (Mode == EmailFormMode.EnterNewPassword)
                {
                    // Режим ввода нового пароля
                    var tagData = this.Tag as dynamic;
                    if (tagData == null || string.IsNullOrEmpty(tagData.Email))
                    {
                        MessageBox.Show("Email не может быть пустым.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    UserEmail = tagData.Email; // Устанавливаем UserEmail из Tag

                    string newPassword = textBox1.Text.Trim();

                    if (string.IsNullOrEmpty(newPassword))
                    {
                        MessageBox.Show("Пароль не может быть пустым.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (!await UpdateUserPasswordAsync(UserEmail, newPassword))
                    {
                        MessageBox.Show("Ошибка при изменении пароля.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    MessageBox.Show("Пароль успешно изменен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> CheckUserExistsAsync(string email)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    var command = new NpgsqlCommand("SELECT COUNT(*) FROM Users WHERE email = @Email", connection);
                    command.Parameters.AddWithValue("@Email", email);
                    int count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке пользователя: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private string GenerateVerificationCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private async Task SendVerificationCodeAsync(string email, string code)
        {
            try
            {
                using (SmtpClient client = new SmtpClient("smtp.mail.ru"))
                {
                    client.Port = 587;
                    client.Credentials = new NetworkCredential("test_coffee13@mail.ru", "iqAv1Kfkttji5952SGfq");
                    client.EnableSsl = true;

                    MailMessage mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress("test_coffee13@mail.ru");
                    mailMessage.To.Add(email);
                    mailMessage.Subject = "Код подтверждения";
                    mailMessage.Body = $"Ваш код подтверждения: {code}";

                    await client.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки письма: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<bool> UpdateUserPasswordAsync(string email, string newPassword)
        {
            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Email не может быть пустым.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var command = new NpgsqlCommand(
                    "UPDATE Users SET password = @Password WHERE email = @Email",
                    connection);

                // Устанавливаем явный тип данных для параметра
                command.Parameters.Add("@Password", NpgsqlTypes.NpgsqlDbType.Varchar).Value = HashPassword(newPassword);
                command.Parameters.Add("@Email", NpgsqlTypes.NpgsqlDbType.Varchar).Value = email;

                int rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public void SetMode(EmailFormMode mode)
        {
            Mode = mode;
            if (mode == EmailFormMode.EnterEmail)
            {
                label1.Text = "Введите ваш email:";
                textBox1.Text = "";
                this.Text = "Восстановление пароля";
            }
            else if (mode == EmailFormMode.EnterNewPassword)
            {
                label1.Text = "Введите новый пароль:";
                textBox1.Text = "";
                this.Text = "Смена пароля";
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}