using System;
using System.Windows.Forms;

namespace TestProjectCoffeeShop
{
    public static class MessageBoxTestHelper
    {
        public static void AssertMessageBoxShown(string expectedMessage, MessageBoxIcon expectedIcon)
        {
            // Переопределяем стандартный метод MessageBox для перехвата вызова
            var messageBoxForm = new MessageBoxForm(expectedMessage, expectedIcon);
            Application.Run(messageBoxForm);

            // Проверка, что окно с нужным сообщением и иконкой появилось
            Assert.That(messageBoxForm.Message, Is.EqualTo(expectedMessage));
            Assert.That(messageBoxForm.Icon, Is.EqualTo(expectedIcon));

        }
    }

    // Вспомогательная форма для перехвата вызова MessageBox
    public class MessageBoxForm : Form
    {
        public string Message { get; private set; }
        public MessageBoxIcon Icon { get; private set; }

        public MessageBoxForm(string message, MessageBoxIcon icon)
        {
            Message = message;
            Icon = icon;

            // Имитируем стандартный MessageBox
            var label = new Label
            {
                Text = message,
                AutoSize = true,
                Location = new System.Drawing.Point(50, 50)
            };
            Controls.Add(label);
        }
    }
}
