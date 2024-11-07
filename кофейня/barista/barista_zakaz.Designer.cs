namespace кофейня
{
    partial class barista_zakaz
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(barista_zakaz));
            label4 = new Label();
            button8 = new Button();
            label2 = new Label();
            pictureBox1 = new PictureBox();
            dataGridView1 = new DataGridView();
            dataGridView2 = new DataGridView();
            button6 = new Button();
            textBox4 = new TextBox();
            button7 = new Button();
            pictureBox5 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).BeginInit();
            SuspendLayout();
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.BackColor = Color.Transparent;
            label4.Font = new Font("Roboto", 24F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label4.ForeColor = Color.FromArgb(202, 57, 0);
            label4.Location = new Point(85, 4);
            label4.Name = "label4";
            label4.Size = new Size(135, 43);
            label4.TabIndex = 63;
            label4.Text = "Заказы";
            // 
            // button8
            // 
            button8.BackColor = Color.Transparent;
            button8.BackgroundImageLayout = ImageLayout.None;
            button8.FlatStyle = FlatStyle.Flat;
            button8.ForeColor = Color.Transparent;
            button8.Image = (Image)resources.GetObject("button8.Image");
            button8.Location = new Point(25, 4);
            button8.Name = "button8";
            button8.Size = new Size(40, 40);
            button8.TabIndex = 62;
            button8.UseVisualStyleBackColor = false;
            button8.Click += button8_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Roboto", 24F);
            label2.ForeColor = Color.FromArgb(202, 57, 0);
            label2.Location = new Point(12, 557);
            label2.Name = "label2";
            label2.Size = new Size(117, 43);
            label2.TabIndex = 64;
            label2.Text = "Выход";
            label2.Click += label2_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.InitialImage = null;
            pictureBox1.Location = new Point(416, 82);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(350, 350);
            pictureBox1.TabIndex = 66;
            pictureBox1.TabStop = false;
            // 
            // dataGridView1
            // 
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(12, 82);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(365, 351);
            dataGridView1.TabIndex = 65;
            // 
            // dataGridView2
            // 
            dataGridView2.BackgroundColor = Color.White;
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2.Location = new Point(805, 82);
            dataGridView2.Name = "dataGridView2";
            dataGridView2.Size = new Size(533, 351);
            dataGridView2.TabIndex = 67;
            // 
            // button6
            // 
            button6.BackColor = Color.Transparent;
            button6.BackgroundImageLayout = ImageLayout.None;
            button6.FlatStyle = FlatStyle.Flat;
            button6.ForeColor = Color.Transparent;
            button6.Image = (Image)resources.GetObject("button6.Image");
            button6.Location = new Point(1228, -1);
            button6.Name = "button6";
            button6.Size = new Size(80, 54);
            button6.TabIndex = 71;
            button6.UseVisualStyleBackColor = false;
            // 
            // textBox4
            // 
            textBox4.BorderStyle = BorderStyle.None;
            textBox4.Font = new Font("Roboto", 14F, FontStyle.Bold);
            textBox4.ForeColor = Color.FromArgb(132, 154, 157);
            textBox4.Location = new Point(858, 14);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(275, 25);
            textBox4.TabIndex = 69;
            textBox4.Text = "ПОИСК";
            // 
            // button7
            // 
            button7.BackColor = Color.Transparent;
            button7.BackgroundImageLayout = ImageLayout.None;
            button7.FlatStyle = FlatStyle.Flat;
            button7.ForeColor = Color.Transparent;
            button7.Image = (Image)resources.GetObject("button7.Image");
            button7.Location = new Point(1163, 6);
            button7.Name = "button7";
            button7.Size = new Size(59, 42);
            button7.TabIndex = 68;
            button7.UseVisualStyleBackColor = false;
            // 
            // pictureBox5
            // 
            pictureBox5.Image = (Image)resources.GetObject("pictureBox5.Image");
            pictureBox5.Location = new Point(845, 5);
            pictureBox5.Name = "pictureBox5";
            pictureBox5.Size = new Size(312, 47);
            pictureBox5.TabIndex = 70;
            pictureBox5.TabStop = false;
            // 
            // barista_zakaz
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1350, 621);
            Controls.Add(button6);
            Controls.Add(textBox4);
            Controls.Add(button7);
            Controls.Add(pictureBox5);
            Controls.Add(dataGridView2);
            Controls.Add(pictureBox1);
            Controls.Add(dataGridView1);
            Controls.Add(label2);
            Controls.Add(label4);
            Controls.Add(button8);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximumSize = new Size(1366, 660);
            MinimumSize = new Size(1364, 660);
            Name = "barista_zakaz";
            Text = "Кофейня";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label4;
        private Button button8;
        private Label label2;
        private PictureBox pictureBox1;
        private DataGridView dataGridView1;
        private DataGridView dataGridView2;
        private Button button6;
        private TextBox textBox4;
        private Button button7;
        private PictureBox pictureBox5;
    }
}