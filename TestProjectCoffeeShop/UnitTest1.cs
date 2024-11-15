using кофейня;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace CoffeeShopTests
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
            // Освобождаем ресурсы
            _registr.Dispose();
        }

        [Test]
        public void HashPassword_ValidPassword_ReturnsHashedString()
        {
            string password = "securePassword123";
            string hashedPassword = _registr.HashPassword(password);
            Assert.That(hashedPassword, Is.Not.Null);
            Assert.That(hashedPassword, Is.Not.EqualTo(password)); // Проверяем, что пароль зашифрован
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
    }
}
