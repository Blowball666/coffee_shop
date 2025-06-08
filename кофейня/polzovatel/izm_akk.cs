using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace кофейня.polzovatel
{
    public partial class izm_akk : Form
    {
        private int userId;
        public izm_akk(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(430, 50);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //если пользователь изменил почту (которая пишется в maskedTextBox1),
            //то надо отправить ему на почту одноразовый код (как при создании аккаунта)
            //и открыть страницу verificationForm для ввода этого кода
            //и изменить почту на новую, если верно
            //если изменены имя фамилия в richTextBox1 или телефон в maskedTextBox2,
            //то просто сохранить изменения в БД
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //открытие страницы pas_editing
        }
    }
}
