using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;

namespace кофейня.polzovatel
{
    public partial class ist : Form
    {
        private int userId;
        private string connString = "Host=localhost;Database=coffee_db;Username=postgres;Password=pwd";
        private vkladki vkladkiForm;

        public ist(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);           

            // Подписываемся на событие выбора строки
            dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;

            LoadOrdersHistory();
        }

        private void LoadOrdersHistory()
        {
            string query = @"
            SELECT 
                TO_CHAR(o.order_date, 'YYYY-MM-DD HH24:MI') AS date_time,
                FLOOR(dp.item_price * dp.quantity) AS total_amount,
                st.name AS status,
                COALESCE(SUM(CASE WHEN pt_earn.transaction_type = 'earn' THEN pt_earn.points END), 0) AS points_earned,
                COALESCE(SUM(CASE WHEN pt_spend.transaction_type = 'spend' THEN pt_spend.points END), 0) AS points_spent
            FROM Orders o
            JOIN Desired_Product dp ON o.id = dp.order_id
            JOIN Statuses st ON o.status_id = st.id
            LEFT JOIN Points_Transactions pt_earn ON o.id = pt_earn.order_id AND pt_earn.transaction_type = 'earn'
            LEFT JOIN Points_Transactions pt_spend ON o.id = pt_spend.order_id AND pt_spend.transaction_type = 'spend'
            WHERE o.user_id = @userId
            GROUP BY o.id, o.order_date, dp.item_price, dp.quantity, st.name
            ORDER BY o.order_date DESC;";

            using (var conn = new NpgsqlConnection(connString))
            {
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("userId", userId);
                    conn.Open();
                    var adapter = new NpgsqlDataAdapter(cmd);
                    var table = new DataTable();
                    adapter.Fill(table);

                    // Привязка данных
                    dataGridView1.DataSource = table;

                    // Настройка DataGridView после привязки данных
                    SetupDataGridViewAfterBinding();
                }
            }
        }

        private void SetupDataGridViewAfterBinding()
        {
            // Включаем заголовки столбцов
            dataGridView1.ColumnHeadersVisible = true;

            // Настройка шрифта
            Font robotoFont = new Font("Roboto", 15, FontStyle.Bold);
            dataGridView1.DefaultCellStyle.Font = robotoFont;

            // Установка русских названий столбцов
            if (dataGridView1.Columns.Contains("date_time"))
                dataGridView1.Columns["date_time"].HeaderText = "Дата и время";
            if (dataGridView1.Columns.Contains("total_amount"))
                dataGridView1.Columns["total_amount"].HeaderText = "Сумма";
            if (dataGridView1.Columns.Contains("status"))
                dataGridView1.Columns["status"].HeaderText = "Статус";
            if (dataGridView1.Columns.Contains("points_earned"))
                dataGridView1.Columns["points_earned"].HeaderText = "Начислено";
            if (dataGridView1.Columns.Contains("points_spent"))
                dataGridView1.Columns["points_spent"].HeaderText = "Списано";

            // Выравнивание текста по центру
            dataGridView1.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Установка шрифта для заголовков
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = robotoFont;

            // Убрать выделение заголовков столбцов
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = dataGridView1.BackgroundColor;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = dataGridView1.DefaultCellStyle.ForeColor;           

            // Настройка ширины столбцов
            dataGridView1.Columns["date_time"].Width = 185;
            dataGridView1.Columns["total_amount"].Width = 70;
            dataGridView1.Columns["status"].Width = 170;
            dataGridView1.Columns["points_earned"].Width = 110;
            dataGridView1.Columns["points_spent"].Width = 90;

            // Нейтральный цвет выделения строк
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.LightGray;
            dataGridView1.DefaultCellStyle.SelectionForeColor = dataGridView1.DefaultCellStyle.ForeColor;

            // Запрет изменения размеров
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.AllowUserToResizeRows = false;

            // Запрет добавления новых строк
            dataGridView1.AllowUserToAddRows = false;

            // Запрет редактирования
            dataGridView1.ReadOnly = true;           

            // Установка режима выделения
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;          
        }

        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Форматирование суммы с добавлением символа рублей
            if (e.ColumnIndex == dataGridView1.Columns["total_amount"].Index && e.Value != null)
            {
                if (int.TryParse(e.Value.ToString(), out int amount))
                {
                    e.Value = $"{amount} ₽"; // Добавляем символ рублей
                    e.FormattingApplied = true;
                }
            }

            // Форматирование начисленных баллов
            if (e.ColumnIndex == dataGridView1.Columns["points_earned"].Index && e.Value != null)
            {
                if (int.TryParse(e.Value.ToString(), out int pointsEarned))
                {
                    e.Value = $"{pointsEarned} б."; // Добавляем "б."
                    e.CellStyle.ForeColor = pointsEarned > 0 ? Color.Green : e.CellStyle.ForeColor;
                    e.FormattingApplied = true;
                }
            }

            // Форматирование списанных баллов
            if (e.ColumnIndex == dataGridView1.Columns["points_spent"].Index && e.Value != null)
            {
                if (int.TryParse(e.Value.ToString(), out int pointsSpent))
                {
                    e.Value = $"{pointsSpent} б."; // Добавляем "б."
                    e.CellStyle.ForeColor = pointsSpent > 0 ? Color.Red : e.CellStyle.ForeColor;
                    e.FormattingApplied = true;
                }
            }
        }

        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            // Проверяем, есть ли выбранная строка
            if (dataGridView1.SelectedRows.Count == 0)
                return;

            // Получаем ID выбранного заказа
            var selectedRow = dataGridView1.SelectedRows[0];
            if (selectedRow.Cells["date_time"].Value == null)
                return;

            string orderDate = selectedRow.Cells["date_time"].Value.ToString();

            // Загружаем детали заказа
            LoadOrderDetails(orderDate);
        }

        private void LoadOrderDetails(string orderDate)
        {
            string query = @"
            SELECT 
                a.name AS product_name,
                s.name AS size_name,
                STRING_AGG(DISTINCT sy.name, ', ') AS syrup_names,
                dp.quantity,
                dp.item_price * dp.quantity AS total_price
            FROM Orders o
            JOIN Desired_Product dp ON o.id = dp.order_id
            JOIN Assortment a ON dp.assortment_id = a.id
            LEFT JOIN Sizes s ON dp.size_id = s.id
            LEFT JOIN Desired_Product_Syrups dps ON dp.id = dps.desired_product_id
            LEFT JOIN Syrups sy ON dps.syrup_id = sy.id
            WHERE TO_CHAR(o.order_date, 'YYYY-MM-DD HH24:MI') = @orderDate
            GROUP BY a.name, s.name, dp.quantity, dp.item_price
            ORDER BY a.name;";

            using (var conn = new NpgsqlConnection(connString))
            {
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("orderDate", orderDate);
                    conn.Open();
                    var adapter = new NpgsqlDataAdapter(cmd);
                    var table = new DataTable();
                    adapter.Fill(table);

                    // Привязка данных к dataGridView2
                    dataGridView2.DataSource = table;

                    // Настройка внешнего вида dataGridView2
                    SetupDataGridView();
                }
            }
        }

        private void SetupDataGridView()
        {
            // Установка русских названий столбцов
            if (dataGridView2.Columns.Contains("product_name"))
                dataGridView2.Columns["product_name"].HeaderText = "Название";
            if (dataGridView2.Columns.Contains("size_name"))
                dataGridView2.Columns["size_name"].HeaderText = "Размер";
            if (dataGridView2.Columns.Contains("syrup_names"))
                dataGridView2.Columns["syrup_names"].HeaderText = "Сиропы";
            if (dataGridView2.Columns.Contains("quantity"))
                dataGridView2.Columns["quantity"].HeaderText = "Кол-во";
            if (dataGridView2.Columns.Contains("total_price"))
                dataGridView2.Columns["total_price"].HeaderText = "Цена";

            // Настройка шрифта
            Font robotoFont = new Font("Roboto", 15, FontStyle.Bold);
            dataGridView2.DefaultCellStyle.Font = robotoFont;
            dataGridView2.ColumnHeadersDefaultCellStyle.Font = robotoFont;

            // Выравнивание текста по центру
            dataGridView2.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView2.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Настройка ширины столбцов
            dataGridView2.Columns["product_name"].Width = 170;
            dataGridView2.Columns["size_name"].Width = 100;
            dataGridView2.Columns["syrup_names"].Width = 200;
            dataGridView2.Columns["quantity"].Width = 80;
            dataGridView2.Columns["total_price"].Width = 100;

            // Запрет изменения размеров
            dataGridView2.AllowUserToResizeColumns = false;
            dataGridView2.AllowUserToResizeRows = false;

            // Запрет добавления новых строк
            dataGridView2.AllowUserToAddRows = false;

            // Запрет редактирования
            dataGridView2.ReadOnly = true;

            // Установка режима выделения
            dataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView2.MultiSelect = false;
            dataGridView2.DefaultCellStyle.SelectionBackColor = Color.LightGray;
            dataGridView2.DefaultCellStyle.SelectionForeColor = dataGridView1.DefaultCellStyle.ForeColor;

            // Убрать выделение заголовков
            dataGridView2.EnableHeadersVisualStyles = false;
            dataGridView2.ColumnHeadersDefaultCellStyle.BackColor = dataGridView2.BackgroundColor;
            dataGridView2.ColumnHeadersDefaultCellStyle.ForeColor = dataGridView2.DefaultCellStyle.ForeColor;
        }
        private void DataGridView2_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == dataGridView2.Columns["total_price"].Index && e.Value != null)
            {
                if (int.TryParse(e.Value.ToString(), out int amount))
                {
                    e.Value = $"{amount} ₽"; // Добавляем символ рублей
                    e.FormattingApplied = true;
                }
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            // Открываем форму vkladki
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
    }
}