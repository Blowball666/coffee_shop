namespace кофейня.barista
{
    public partial class vkladki_b : Form
    {
        private int userId;
        private Form previousForm;

        public vkladki_b(Form previousForm, int userId)
        {
            InitializeComponent();
            this.previousForm = previousForm;
            this.userId = userId;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(105, 50);

            // Подписка на событие закрытия предыдущей формы
            this.previousForm.FormClosed += (s, args) => this.Close();
        }

        private void OpenAndClose(Form newForm)
        {
            // Открываем новую форму
            newForm.Show();

            // Закрываем текущую навигацию и предыдущую форму
            this.previousForm.Hide();
            this.Hide();
        }
        private void label2_Click(object sender, EventArgs e)
        {
            vhod vhodForm = new vhod();
            OpenAndClose(vhodForm);
        }

        private void label1_Click(object sender, EventArgs e)
        {
            barista_zakaz form1 = new barista_zakaz(userId);
            OpenAndClose(form1);
        }
    }
}
