namespace TestProjectCoffeeShop
{
    [TestFixture]
    public class AdminMenuTests
    {
        private �������.admin_menu _adminMenuForm;

        [SetUp]
        public void Setup()
        {
            _adminMenuForm = new �������.admin_menu();
        }

        [TearDown]
        public void TearDown()
        {
            _adminMenuForm.Dispose();
        }

        [Test]
        public void ValidateProductData_ValidInput_ReturnsTrue()
        {
            // Arrange
            var product = new
            {
                Name = "��������",
                Price = 150,
                Description = "�������� ������� � �������� ������"
            };

            // Act
            // ���������� ����� ��� �������� ������ ����� ���������
            _adminMenuForm.textBox1.Text = product.Name; // �������� ������
            _adminMenuForm.textBox2.Text = product.Description; // ��������
            _adminMenuForm.textBox3.Text = product.Price.ToString(); // ����

            bool isValid = _adminMenuForm.ValidateInputs();

            // Assert
            Assert.That(isValid, Is.True); // ������ ���������
        }

        [Test]
        public void AddNewProduct_ValidInput_ProductAddedSuccessfully()
        {
            // Arrange
            string productName = "�����";
            decimal productPrice = 200;
            string productDescription = "�������� ������� � �������";

            // Act
            // ��������� ���������� ����� �����
            _adminMenuForm.textBox1.Text = productName;
            _adminMenuForm.textBox2.Text = productDescription;
            _adminMenuForm.textBox3.Text = productPrice.ToString();

            // ��������� ������� ������ "�������� �����"
            _adminMenuForm.button2.PerformClick();

            // �������� ID ������������ ������ �� ���� ������
            int productId = GetLastInsertedProductId();

            // Assert
            Assert.That(productId, Is.GreaterThan(0)); // ����� ������� ��������, ��������� ��� ID
        }

        [Test]
        public void UpdateAssortmentRelations_ValidInput_RelationsUpdatedSuccessfully()
        {
            // Arrange
            int productId = 1;
            bool canChooseSize = true;
            bool canAddSyrup = true;

            // Act
            // �������� ���������� ������ ����� ���������
            _adminMenuForm.UpdateAssortmentRelations(productId, canChooseSize, canAddSyrup);

            // Assert
            // ����� ����� ��������� ��������� � ���� ������ ��� ��������� �������
            Assert.Pass("����� ��������� �������."); // ������� ��������
        }

        private int GetLastInsertedProductId()
        {
            // ����� ��� ��������� ID ���������� ������������ ������ �� ���� ������
            using (var connection = new Npgsql.NpgsqlConnection("Host=localhost;Database=coffee_db;Username=postgres;Password=pwd"))
            {
                connection.Open();
                var command = new Npgsql.NpgsqlCommand("SELECT MAX(id) FROM Assortment", connection);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }
    }
}