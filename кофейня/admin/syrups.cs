using System;
using System.Windows.Forms;

namespace кофейня.admin
{
    public partial class syrups : Form
    {
        public string SyrupName { get; private set; }
        public decimal SyrupPrice { get; private set; }
        public bool IsConfirmed { get; private set; } = false;

        public syrups(string initialName = "", decimal initialPrice = 0)
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;

            // Установка начальных значений
            textBox1.Text = initialName;
            textBox2.Text = initialPrice > 0 ? initialPrice.ToString() : "";
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            // Отмена операции
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void buttonConfirm_Click(object sender, EventArgs e)
        {
            // Проверка корректности ввода
            string name = textBox1.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Название сиропа не может быть пустым.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(textBox2.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Цена должна быть положительным числом.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Сохранение данных
            SyrupName = name;
            SyrupPrice = price;
            IsConfirmed = true;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}