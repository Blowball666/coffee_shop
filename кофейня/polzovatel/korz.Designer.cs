namespace кофейня.polzovatel
{
    partial class korz
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(korz));
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            label1 = new Label();
            button1 = new Button();
            dataGridView1 = new DataGridView();
            button2 = new Button();
            checkBox1 = new CheckBox();
            label9 = new Label();
            button3 = new Button();
            button4 = new Button();
            pictureBox1 = new PictureBox();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Roboto", 24F, FontStyle.Bold);
            label1.ForeColor = Color.FromArgb(202, 57, 0);
            label1.Location = new Point(86, 26);
            label1.Name = "label1";
            label1.Size = new Size(169, 43);
            label1.TabIndex = 31;
            label1.Text = "КОРЗИНА";
            // 
            // button1
            // 
            button1.BackColor = Color.Transparent;
            button1.BackgroundImageLayout = ImageLayout.None;
            button1.FlatStyle = FlatStyle.Flat;
            button1.ForeColor = Color.Transparent;
            button1.Image = (Image)resources.GetObject("button1.Image");
            button1.Location = new Point(26, 26);
            button1.Name = "button1";
            button1.Size = new Size(40, 40);
            button1.TabIndex = 32;
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dataGridView1.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;
            dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = Color.White;
            dataGridViewCellStyle1.Font = new Font("Roboto", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 204);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = Color.White;
            dataGridViewCellStyle1.SelectionForeColor = Color.Black;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dataGridView1.ColumnHeadersHeight = 40;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.GridColor = Color.White;
            dataGridView1.Location = new Point(343, 4);
            dataGridView1.Margin = new Padding(0);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.TopCenter;
            dataGridViewCellStyle2.BackColor = Color.White;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = Color.White;
            dataGridViewCellStyle2.SelectionForeColor = Color.Black;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToFirstHeader;
            dataGridView1.ShowEditingIcon = false;
            dataGridView1.Size = new Size(1001, 524);
            dataGridView1.TabIndex = 33;
            dataGridView1.TabStop = false;
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;
            dataGridView1.CellFormatting += DataGridView1_CellFormatting;
            dataGridView1.CellValueChanged += dataGridView1_CellValueChanged;
            // 
            // button2
            // 
            button2.BackColor = Color.Transparent;
            button2.BackgroundImageLayout = ImageLayout.None;
            button2.FlatStyle = FlatStyle.Flat;
            button2.ForeColor = Color.Transparent;
            button2.Image = (Image)resources.GetObject("button2.Image");
            button2.Location = new Point(1049, 532);
            button2.Name = "button2";
            button2.Size = new Size(289, 74);
            button2.TabIndex = 72;
            button2.UseVisualStyleBackColor = false;
            button2.Click += button2_Click;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Font = new Font("Roboto", 18F);
            checkBox1.ForeColor = Color.FromArgb(202, 57, 0);
            checkBox1.Location = new Point(697, 536);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(127, 36);
            checkBox1.TabIndex = 75;
            checkBox1.Text = "Списать";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.BackColor = Color.Transparent;
            label9.Font = new Font("Roboto", 18F);
            label9.ForeColor = Color.FromArgb(202, 57, 0);
            label9.Location = new Point(415, 537);
            label9.Name = "label9";
            label9.Size = new Size(185, 32);
            label9.TabIndex = 74;
            label9.Text = "У вас ... баллов";
            // 
            // button3
            // 
            button3.BackColor = Color.Transparent;
            button3.BackgroundImageLayout = ImageLayout.None;
            button3.FlatStyle = FlatStyle.Flat;
            button3.ForeColor = Color.Transparent;
            button3.Image = (Image)resources.GetObject("button3.Image");
            button3.Location = new Point(11, 536);
            button3.Name = "button3";
            button3.Size = new Size(357, 78);
            button3.TabIndex = 73;
            button3.UseVisualStyleBackColor = false;
            button3.Click += button3_Click;
            // 
            // button4
            // 
            button4.BackColor = Color.Transparent;
            button4.BackgroundImageLayout = ImageLayout.None;
            button4.FlatStyle = FlatStyle.Flat;
            button4.ForeColor = Color.Transparent;
            button4.Image = (Image)resources.GetObject("button4.Image");
            button4.Location = new Point(282, 18);
            button4.Name = "button4";
            button4.Size = new Size(53, 56);
            button4.TabIndex = 76;
            button4.UseVisualStyleBackColor = false;
            button4.Click += button4_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.InitialImage = null;
            pictureBox1.Location = new Point(6, 96);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(331, 350);
            pictureBox1.TabIndex = 78;
            pictureBox1.TabStop = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Roboto", 18F);
            label2.ForeColor = Color.FromArgb(202, 57, 0);
            label2.Location = new Point(415, 576);
            label2.Name = "label2";
            label2.Size = new Size(238, 32);
            label2.TabIndex = 79;
            label2.Text = "Спишется ... баллов";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = Color.Transparent;
            label3.Font = new Font("Roboto", 20F, FontStyle.Bold);
            label3.ForeColor = Color.FromArgb(202, 57, 0);
            label3.Location = new Point(12, 458);
            label3.Name = "label3";
            label3.Size = new Size(317, 35);
            label3.TabIndex = 80;
            label3.Text = "Итоговая стоимость: ...";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.BackColor = Color.Transparent;
            label4.Font = new Font("Roboto", 18F);
            label4.ForeColor = Color.FromArgb(202, 57, 0);
            label4.Location = new Point(15, 498);
            label4.Name = "label4";
            label4.Size = new Size(260, 32);
            label4.TabIndex = 81;
            label4.Text = "Начислится ... баллов";
            // 
            // korz
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1350, 621);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(pictureBox1);
            Controls.Add(button4);
            Controls.Add(checkBox1);
            Controls.Add(label9);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(dataGridView1);
            Controls.Add(button1);
            Controls.Add(label1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximumSize = new Size(1366, 660);
            MinimumSize = new Size(1364, 660);
            Name = "korz";
            Text = "Кофейня";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button button1;
        private Label label1;
        private DataGridView dataGridView1;
        private Button button2;
        private CheckBox checkBox1;
        internal Label label9;
        private Button button3;
        private Button button4;
        private PictureBox pictureBox1;
        internal Label label2;
        internal Label label3;
        internal Label label4;
    }
}