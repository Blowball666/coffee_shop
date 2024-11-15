using System.Windows.Forms;
using кофейня;

namespace TestProjectCoffeeShop
{
    [TestFixture]
    public class RegistrTests
    {
        private registr _form;

        [SetUp]
        public void SetUp()
        {
            _form = new registr(); // Инициализация формы
        }
        [TearDown]
        public void TearDown()
        {
            // Освобождаем ресурсы
            _form.Dispose();
        }
        [Test]
        public void Test_EmptyFields_ShowWarningMessage()
        {
            // Установим пустые значения в поля ввода
            _form.richTextBox1.Text = "";
            _form.maskedTextBox1.Text = "";
            _form.maskedTextBox2.Text = "";
            _form.maskedTextBox3.Text = "";
            _form.textBox1.Text = "";
            // Слушаем вызов кнопки для создания пользователя
            _form.button1.PerformClick();
            // Проверяем, что показано сообщение об ошибке
            MessageBoxTestHelper.AssertMessageBoxShown("Заполните все поля перед сохранением в базу данных.", MessageBoxIcon.Warning);
        }

        [Test]
        public void Test_InvalidEmail_ShowErrorMessage()
        {
            _form.richTextBox1.Text = "Имя Фамилия";
            _form.maskedTextBox1.Text = "invalidemail.com"; // Некорректный email
            _form.maskedTextBox2.Text = "8(123)456-78-90";
            _form.maskedTextBox3.Text = "2000/01/01";
            _form.textBox1.Text = "Password123";
            _form.button1.PerformClick();
            // Проверяем, что показано сообщение об ошибке
            MessageBoxTestHelper.AssertMessageBoxShown("Некорректный email.", MessageBoxIcon.Error);
        }

        [Test]
        public void Test_InvalidPhone_ShowErrorMessage()
        {
            _form.richTextBox1.Text = "Имя Фамилия";
            _form.maskedTextBox1.Text = "test@example.com";
            _form.maskedTextBox2.Text = "123456789"; // Некорректный номер телефона
            _form.maskedTextBox3.Text = "2000/01/01";
            _form.textBox1.Text = "Password123";
            _form.button1.PerformClick();
            // Проверяем, что показано сообщение об ошибке
            MessageBoxTestHelper.AssertMessageBoxShown("Некорректный номер телефона.", MessageBoxIcon.Error);
        }

    }
}
