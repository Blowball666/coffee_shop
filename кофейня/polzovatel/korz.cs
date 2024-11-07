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
    public partial class korz : Form
    {
        private int userId;
        private vkladki vkladkiForm;
        public korz(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Проверка, открыт ли экземпляр окна навигации
            if (vkladkiForm == null || vkladkiForm.IsDisposed)
            {
                // Если окно не создано или было закрыто, создаём новый экземпляр и показываем его
                vkladkiForm = new vkladki(userId, this);
                vkladkiForm.Show();
            }
            else
            {
                // Если окно уже создано, переключаем видимость
                vkladkiForm.Visible = !vkladkiForm.Visible;

                // Переводим фокус на окно, если оно отображено
                if (vkladkiForm.Visible)
                {
                    vkladkiForm.Focus();
                }
            }
        }

    }
}
