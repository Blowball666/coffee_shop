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
    public partial class pas_editing : Form
    {
        public pas_editing()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void buttonConfirm_Click(object sender, EventArgs e)
        {
            //подтверждение изменения пароля с помощью кода с почты,
            //после ввода правильного одноразового кода(открывается страница verificationForm),
            //хэширование и сохранение нового пароля, закрытие страницы
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            //отмена
        }
    }
}
