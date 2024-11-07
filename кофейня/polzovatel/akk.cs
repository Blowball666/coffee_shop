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
            string connectionString = "Host=172.20.7.6; Database=krezhowa_coffee; Username=st; Password=pwd";
            string query = @"
                SELECT 
                    Roles.name AS RoleName,
                    Users.last_name, 
                    Users.first_name,
                    Users.email,
                    Users.phone,
                    Users.birth_date,
                    Users.points
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
                        label7.Text = reader["RoleName"].ToString();
                        label8.Text = reader["last_name"].ToString() + " " + reader["first_name"].ToString();
                        label9.Text = reader["email"].ToString();
                        label10.Text = reader["phone"].ToString();
                        label11.Text = ((DateTime)reader["birth_date"]).ToShortDateString();
                        label12.Text = reader["points"].ToString();
                    }
                }
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
    }
}
