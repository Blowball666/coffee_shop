namespace coffee
{
    public partial class verificationForm : Form
    {
        public string Code { get; set; } // Публичное свойство для кода подтверждения

        public verificationForm()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void buttonConfirm_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == Code)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный код подтверждения!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
