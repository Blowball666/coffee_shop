using System;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;

namespace кофейня.polzovatel
{
    public partial class akk : Form
    {
        private int userId;
        private vkladki vkladkiForm;

        public akk(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);

            LoadUserData();
        }
        private void LoadUserData()
        {
            string connectionString = "Host=localhost;Database=coffee_db;Username=postgres;Password=pwd";
            string query = @"
                SELECT 
                    Users.last_name, 
                    Users.first_name,
                    Users.email,
                    Users.phone,
                    Users.birth_date
                FROM 
                    Users
                JOIN 
                    Roles ON Users.role_id = Roles.id
                WHERE 
                    Users.id = @userId";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("userId", userId);
                connection.Open();
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        label8.Text = reader["last_name"].ToString() + " " + reader["first_name"].ToString();
                        label9.Text = reader["email"].ToString();
                        label10.Text = reader["phone"].ToString();
                        label11.Text = ((DateTime)reader["birth_date"]).ToShortDateString();
                    }
                }
            }
            LoadUserPoints();
        }
        private void LoadUserPoints()
        {
            string connectionString = "Host=localhost;Database=coffee_db;Username=postgres;Password=pwd";
            string pointsQuery = @"
                SELECT 
                    COALESCE(SUM(CASE WHEN transaction_type = 'earn' THEN points ELSE 0 END), 0) -
                    COALESCE(SUM(CASE WHEN transaction_type = 'spend' THEN points ELSE 0 END), 0) -
                    COALESCE(SUM(CASE WHEN transaction_type = 'expire' THEN points ELSE 0 END), 0) 
                AS active_points
                FROM Points_Transactions
                WHERE user_id = @userId";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            using (NpgsqlCommand command = new NpgsqlCommand(pointsQuery, connection))
            {
                command.Parameters.AddWithValue("userId", userId);
                connection.Open();
                var result = command.ExecuteScalar();
                label12.Text = result != DBNull.Value ? result.ToString() : "0";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (vkladkiForm == null || vkladkiForm.IsDisposed)
            {
                vkladkiForm = new vkladki(userId, this);
                vkladkiForm.Show();
            }
            else
            {
                vkladkiForm.Visible = !vkladkiForm.Visible;
                if (vkladkiForm.Visible)
                {
                    vkladkiForm.Focus();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            vhod vhodForm = new vhod();
            vhodForm.Show();
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Вы уверены, что хотите выйти из программы?",
                "Подтверждение выхода",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                foreach (Form form in Application.OpenForms)
                {
                    form.Close();
                }
                Application.Exit();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            izm_akk izm_akkForm = new izm_akk(userId);
            izm_akkForm.Show();

            // Подписываемся на событие закрытия формы info
            izm_akkForm.FormClosing += (s, args) =>
            {
                LoadUserData(); // Обновляем данные в
            };
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //тут будет код для удаления аккаунта пользователя,
            //естественно перед этим спросив, точно ли пользователь уверен, что хочет удалить свой аккаунт,
            //а также наверно подтвердив удаление с помощью кода с почты (открывается страница verificationForm),
            //после ввода правильного одноразового кода, аккаунт этого пользователя удаляется,
            //зыкрываются все страницы и открывается страница входа
        }
    }
}