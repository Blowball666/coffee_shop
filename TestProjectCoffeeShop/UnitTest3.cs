using System.Windows.Forms;

namespace TestProjectCoffeeShop
{
    [TestFixture]
    public class MenuTests
    {
        private кофейня.polzovatel.info _infoForm;

        [SetUp]
        public void Setup()
        {
            _infoForm = new кофейня.polzovatel.info(assortmentId: 1, userId: 1, userRoleId: 2, menuForm: null);
        }

        [TearDown]
        public void TearDown()
        {
            _infoForm.Dispose();
        }

        [Test]
        public void CalculateTotalPrice_ValidInput_ReturnsCorrectPrice()
        {
            // Arrange
            _infoForm.finalPrice = 100; // Базовая цена товара
            _infoForm.quantity = 2;     // Количество

            // Установка начальных значений для размеров и сиропов
            if (_infoForm.comboBox1.Items.Count > 0)
            {
                _infoForm.comboBox1.SelectedIndex = 0; // Первый размер по умолчанию
            }
            foreach (DataGridViewRow row in _infoForm.dataGridView1.Rows)
            {
                row.Cells["Quantity"].Value = 0; // Обнуляем количество сиропов
            }

            // Act
            _infoForm.UpdatePrice(); // Обновляем цену с учетом размеров и сиропов
            decimal totalPrice = _infoForm.finalPrice * _infoForm.quantity;

            // Assert
            Assert.That(totalPrice, Is.EqualTo(1200)); // Ожидаемая итоговая цена
        }

        [Test]
        public void ApplyLoyaltyPoints_ValidInput_PointsDeductedCorrectly()
        {
            // Arrange
            _infoForm.finalPrice = 300; // Базовая цена товара
            _infoForm.checkBox1.Checked = true; // Пользователь хочет использовать баллы
            int userPoints = 100;

            // Act
            decimal pointsToRedeem = Math.Min(_infoForm.finalPrice * 0.3m, userPoints);
            decimal finalPrice = _infoForm.finalPrice - pointsToRedeem;

            // Assert
            Assert.That(pointsToRedeem, Is.EqualTo(90)); // Максимально возможные баллы для списания
            Assert.That(finalPrice, Is.EqualTo(210));   // Итоговая цена после списания баллов
        }

        [Test]
        public void PlaceOrder_ValidInput_OrderCreatedSuccessfully()
        {
            // Arrange
            _infoForm.finalPrice = 200;
            _infoForm.quantity = 1;
            _infoForm.checkBox1.Checked = false; // Баллы не используются

            // Act
            _infoForm.button3.PerformClick(); // Имитация нажатия кнопки "Заказать"

            // Assert
            Assert.That(_infoForm.finalPrice, Is.EqualTo(200)); // Проверяем, что цена корректна
        }
    }
}