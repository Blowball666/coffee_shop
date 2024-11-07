using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using кофейня.admin;

namespace кофейня
{
    public partial class barista_zakaz : Form
    {
        private int userId;
        private vkladki_a vkladkiForm;
        public barista_zakaz(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);
        }

        private void button8_Click(object sender, EventArgs e)
        {


        }

        private void label2_Click(object sender, EventArgs e)
        {
            vhod formVhod = new vhod();
            formVhod.Show();
            this.Close();
        }
    }
}
