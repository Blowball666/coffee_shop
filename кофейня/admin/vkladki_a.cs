namespace кофейня.admin
{
    public partial class vkladki_a : Form
    {
        private Form previousForm;

        public vkladki_a(Form previousForm)
        {
            InitializeComponent();
            this.previousForm = previousForm;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(125, 70);
        }

        private void OpenAndClose(Form newForm)
        {            
            // Открываем новую форму
            newForm.Show();
            // Скрываем текущую форму
            this.Hide();
            this.previousForm.Hide();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            vhod vhodForm = new vhod();
            OpenAndClose(vhodForm);
        }

        private void label1_Click(object sender, EventArgs e)
        {
            admin_menu form1 = new admin_menu();
            OpenAndClose(form1);
        }

        private void label2_Click(object sender, EventArgs e)
        {
            upr_barista form2 = new upr_barista();
            OpenAndClose(form2);
        }
    }
}
