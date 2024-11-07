
namespace кофейня.polzovatel
{
    public partial class vkladki : Form
    {
        private int userId;
        private Form previousForm;

        public vkladki(int userId, Form previousForm)
        {
            InitializeComponent();
            this.userId = userId;
            this.previousForm = previousForm;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(125, 70);
        }

        private void OpenAndClose(Form newForm)
        {
            // Открываем новую форму
            newForm.Show();

            // Закрываем текущую навигацию и предыдущую форму
            this.previousForm.Hide();
            this.Hide();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            akk form1 = new akk(userId);
            OpenAndClose(form1);
        }

        private void label4_Click(object sender, EventArgs e)
        {
            menu form2 = new menu(userId);
            OpenAndClose(form2);
        }

        private void label2_Click(object sender, EventArgs e)
        {
            korz form3 = new korz(userId);
            OpenAndClose(form3);
        }

        private void label3_Click(object sender, EventArgs e)
        {
            otl form4 = new otl(userId);
            OpenAndClose(form4);
        }
    }
}
