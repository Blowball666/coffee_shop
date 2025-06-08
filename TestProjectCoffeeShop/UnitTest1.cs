using System.Text.RegularExpressions;
using System.Windows.Forms;
using �������;

namespace TestProjectCoffeeShop
{
    [TestFixture]
    public class RegistrTests
    {
        private registr _registr;

        [SetUp]
        public void Setup()
        {
            _registr = new registr();
        }

        [TearDown]
        public void TearDown()
        {
            // ����������� �������
            _registr.Dispose();
        }

        [Test]
        public void HashPassword_ValidPassword_ReturnsHashedString()
        {
            string password = "securePassword123";
            string hashedPassword = _registr.HashPassword(password);
            Assert.That(hashedPassword, Is.Not.Null);
            Assert.That(hashedPassword, Is.Not.EqualTo(password)); // ���������, ��� ������ ����������
        }

        [Test]
        public void ValidateEmail_CorrectFormat_ReturnsTrue()
        {
            string validEmail = "test@example.com";
            bool isValid = Regex.IsMatch(validEmail, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void ValidatePhoneNumber_ValidFormat_ReturnsTrue()
        {
            string validPhone = "8(123)456-78-90";
            bool isValid = Regex.IsMatch(validPhone, @"^8\(\d{3}\)\d{3}-\d{2}-\d{2}$");
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void ValidatePhoneNumber_InvalidFormat_ReturnsFalse()
        {
            string invalidPhone = "1234567890";
            bool isValid = Regex.IsMatch(invalidPhone, @"^8\(\d{3}\)\d{3}-\d{2}-\d{2}$");
            Assert.That(isValid, Is.False);
        }
        [Test]
        public void Test_EmptyFields_ShowWarningMessage()
        {
            // ��������� ������ �������� � ���� �����
            _registr.richTextBox1.Text = "";
            _registr.maskedTextBox1.Text = "";
            _registr.maskedTextBox2.Text = "";
            _registr.maskedTextBox3.Text = "";
            _registr.textBox1.Text = "";
            // ������� ����� ������ ��� �������� ������������
            _registr.button1.PerformClick();
            // ���������, ��� �������� ��������� �� ������
            MessageBoxTestHelper.AssertMessageBoxShown("��������� ��� ���� ����� ����������� � ���� ������.", MessageBoxIcon.Warning);
        }

        [Test]
        public void Test_InvalidEmail_ShowErrorMessage()
        {
            _registr.richTextBox1.Text = "��� �������";
            _registr.maskedTextBox1.Text = "invalidemail.com"; // ������������ email
            _registr.maskedTextBox2.Text = "8(123)456-78-90";
            _registr.maskedTextBox3.Text = "2000/01/01";
            _registr.textBox1.Text = "Password123";
            _registr.button1.PerformClick();
            // ���������, ��� �������� ��������� �� ������
            MessageBoxTestHelper.AssertMessageBoxShown("������������ email.", MessageBoxIcon.Error);
        }

        [Test]
        public void Test_InvalidPhone_ShowErrorMessage()
        {
            _registr.richTextBox1.Text = "��� �������";
            _registr.maskedTextBox1.Text = "test@example.com";
            _registr.maskedTextBox2.Text = "123456789"; // ������������ ����� ��������
            _registr.maskedTextBox3.Text = "2000/01/01";
            _registr.textBox1.Text = "Password123";
            _registr.button1.PerformClick();
            // ���������, ��� �������� ��������� �� ������
            MessageBoxTestHelper.AssertMessageBoxShown("������������ ����� ��������.", MessageBoxIcon.Error);
        }
    }
}
