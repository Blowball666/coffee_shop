using System.Data;
using Npgsql;
using кофейня.barista;

namespace кофейня
{
    public partial class barista_zakaz : Form
    {
        private int userId;
        private vkladki_b vkladkiForm;
        private int selectedOrderId = -1;
        private int? currentStatusId = null;
        private string connectionString = "Host=localhost;Database=coffee_db;Username=postgres;Password=pwd";

        public barista_zakaz(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);
            ConfigureDataGridView();
            LoadOrders();
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked) currentStatusId = 0;
            else if (radioButton2.Checked) currentStatusId = 1;
            else if (radioButton3.Checked) currentStatusId = 2;
            else currentStatusId = null;
            LoadOrders(currentStatusId);
        }

        private void ConfigureDataGridView()
        {
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.FromArgb(245, 226, 218);
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;
            dataGridView1.DefaultCellStyle.Font = new Font("Roboto", 14);
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;

            dataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView2.DefaultCellStyle.SelectionBackColor = Color.FromArgb(245, 226, 218);
            dataGridView2.DefaultCellStyle.SelectionForeColor = Color.Black;
            dataGridView2.DefaultCellStyle.Font = new Font("Roboto", 14);
            dataGridView2.ReadOnly = true;
            dataGridView2.AllowUserToAddRows = false;
        }

        private void LoadOrders(int? statusId = null)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
        SELECT
            o.id AS OrderId,
            u.email AS Email, 
            o.order_date::date AS OrderDate, 
            to_char(o.order_date, 'HH24:MI') AS OrderTime,
            COALESCE(SUM(dp.item_price * dp.quantity), 0) AS TotalPrice, -- Расчет общей стоимости
            COALESCE(pt.points, 0) AS UsedPoints
        FROM Orders o
        JOIN Users u ON o.user_id = u.id
        LEFT JOIN Desired_Product dp ON o.id = dp.order_id
        LEFT JOIN Points_Transactions pt ON o.id = pt.order_id AND pt.transaction_type = 'spend'";
                if (statusId.HasValue)
                    query += " WHERE o.status_id = @statusId";
                query += " GROUP BY o.id, u.email, o.order_date, pt.points"; // Группировка данных
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    if (statusId.HasValue)
                        cmd.Parameters.AddWithValue("statusId", statusId.Value);
                    using (var reader = cmd.ExecuteReader())
                    {
                        var dt = new DataTable();
                        dt.Load(reader);
                        dataGridView1.DataSource = dt;
                    }
                }
            }
            dataGridView1.Columns["Email"].Width = 145;
            dataGridView1.Columns["OrderDate"].Width = 120;
            dataGridView1.Columns["OrderTime"].Width = 80;
            dataGridView1.Columns["TotalPrice"].Width = 90;
            dataGridView1.Columns["UsedPoints"].Width = 70;
            dataGridView1.Columns[0].Visible = false;
            dataGridView1.RowHeadersVisible = false;
        }
        private void LoadOrderItems(int orderId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                SELECT 
                    a.name AS ProductName, 
                    dp.item_price AS Price, 
                    s.name AS SizeName, 
                    a.description AS Description,
                    COALESCE(
                        STRING_AGG(DISTINCT CONCAT(sy.name, ' ', dps.quantity, ' шт.'), ', '), 
                        '-'
                    ) AS SyrupsInfo
                FROM Desired_Product dp
                JOIN Assortment a ON dp.assortment_id = a.id
                LEFT JOIN Sizes s ON dp.size_id = s.id
                LEFT JOIN Desired_Product_Syrups dps ON dp.id = dps.desired_product_id
                LEFT JOIN Syrups sy ON dps.syrup_id = sy.id
                WHERE dp.order_id = @orderId
                GROUP BY a.name, dp.item_price, s.name, a.description";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("orderId", orderId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        var dt = new DataTable();
                        dt.Load(reader);
                        dataGridView2.DataSource = dt;
                    }
                }
            }
            dataGridView2.Columns["ProductName"].Width = 110;
            dataGridView2.Columns["Price"].Width = 77;
            dataGridView2.Columns["SizeName"].Width = 80;
            dataGridView2.Columns["Description"].Width = 175;
            dataGridView2.Columns["SyrupsInfo"].Width = 200; // Новый столбец для сиропов
            dataGridView2.RowHeadersVisible = false;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (vkladkiForm == null || vkladkiForm.IsDisposed)
            {
                vkladkiForm = new vkladki_b(this, userId);
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

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                selectedOrderId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["OrderId"].Value);
                LoadOrderItems(selectedOrderId);
            }
        }

        private bool ConfirmStatusChange(string status)
        {
            return MessageBox.Show($"Вы уверены, что хотите изменить статус заказа на '{status}'?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (selectedOrderId != -1 && ConfirmStatusChange("в процессе"))
                UpdateOrderStatus(1);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (selectedOrderId != -1 && ConfirmStatusChange("выполнен"))
                UpdateOrderStatus(2);
        }

        private void UpdateOrderStatus(int statusId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Orders SET status_id = @statusId WHERE id = @orderId";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("statusId", statusId);
                    cmd.Parameters.AddWithValue("orderId", selectedOrderId);
                    cmd.ExecuteNonQuery();
                }
            }
            LoadOrders(currentStatusId);
            if (selectedOrderId != -1)
            {
                LoadOrderItems(selectedOrderId);
            }
        }
    }
}

