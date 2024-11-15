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
            button1 = new Button();
            dataGridView1 = new DataGridView();
            dataGridView2 = new DataGridView();
            radioButton1 = new RadioButton();
            radioButton2 = new RadioButton();
            radioButton3 = new RadioButton();
            button3 = new Button();
            button4 = new Button();
            pictureBox1 = new PictureBox();
            radioButton4 = new RadioButton();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
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
            // button1
            // 
            button1.BackColor = Color.Transparent;
            button1.BackgroundImageLayout = ImageLayout.None;
            button1.FlatStyle = FlatStyle.Flat;
            button1.ForeColor = Color.Transparent;
            button1.Image = (Image)resources.GetObject("button1.Image");
            button1.Location = new Point(25, 4);
            button1.Name = "button1";
            button1.Size = new Size(40, 40);
            button1.TabIndex = 62;
            button1.UseVisualStyleBackColor = false;
            button1.Click += button8_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(742, 45);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(581, 351);
            dataGridView1.TabIndex = 65;
            dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
            // 
            // dataGridView2
            // 
            dataGridView2.BackgroundColor = Color.White;
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2.Location = new Point(352, 45);
            dataGridView2.Name = "dataGridView2";
            dataGridView2.Size = new Size(365, 351);
            dataGridView2.TabIndex = 67;
            // 
            // radioButton1
            // 
            radioButton1.AutoSize = true;
            radioButton1.Font = new Font("Roboto", 18F, FontStyle.Bold);
            radioButton1.ForeColor = Color.FromArgb(202, 57, 0);
            radioButton1.Location = new Point(418, 461);
            radioButton1.Name = "radioButton1";
            radioButton1.Size = new Size(175, 36);
            radioButton1.TabIndex = 72;
            radioButton1.Text = "В обработке";
            radioButton1.UseVisualStyleBackColor = true;
            radioButton1.CheckedChanged += radioButton_CheckedChanged;
            // 
            // radioButton2
            // 
            radioButton2.AutoSize = true;
            radioButton2.Font = new Font("Roboto", 18F, FontStyle.Bold);
            radioButton2.ForeColor = Color.FromArgb(202, 57, 0);
            radioButton2.Location = new Point(418, 502);
            radioButton2.Name = "radioButton2";
            radioButton2.Size = new Size(310, 36);
            radioButton2.TabIndex = 73;
            radioButton2.Text = "В процессе выполнения";
            radioButton2.UseVisualStyleBackColor = true;
            radioButton2.CheckedChanged += radioButton_CheckedChanged;
            // 
            // radioButton3
            // 
            radioButton3.AutoSize = true;
            radioButton3.Font = new Font("Roboto", 18F, FontStyle.Bold);
            radioButton3.ForeColor = Color.FromArgb(202, 57, 0);
            radioButton3.Location = new Point(418, 419);
            radioButton3.Name = "radioButton3";
            radioButton3.Size = new Size(133, 36);
            radioButton3.TabIndex = 74;
            radioButton3.Text = "Готовые";
            radioButton3.UseVisualStyleBackColor = true;
            radioButton3.CheckedChanged += radioButton_CheckedChanged;
            // 
            // button3
            // 
            button3.BackColor = Color.Transparent;
            button3.BackgroundImageLayout = ImageLayout.None;
            button3.FlatStyle = FlatStyle.Flat;
            button3.ForeColor = Color.Transparent;
            button3.Image = (Image)resources.GetObject("button3.Image");
            button3.Location = new Point(958, 432);
            button3.Name = "button3";
            button3.Size = new Size(365, 74);
            button3.TabIndex = 75;
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
            button4.Location = new Point(958, 512);
            button4.Name = "button4";
            button4.Size = new Size(365, 74);
            button4.TabIndex = 76;
            button4.UseVisualStyleBackColor = false;
            button4.Click += button4_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.InitialImage = null;
            pictureBox1.Location = new Point(1, 124);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(350, 350);
            pictureBox1.TabIndex = 79;
            pictureBox1.TabStop = false;
            // 
            // radioButton4
            // 
            radioButton4.AutoSize = true;
            radioButton4.Checked = true;
            radioButton4.Font = new Font("Roboto", 18F, FontStyle.Bold);
            radioButton4.ForeColor = Color.FromArgb(202, 57, 0);
            radioButton4.Location = new Point(418, 544);
            radioButton4.Name = "radioButton4";
            radioButton4.Size = new Size(72, 36);
            radioButton4.TabIndex = 80;
            radioButton4.TabStop = true;
            radioButton4.Text = "Все";
            radioButton4.UseVisualStyleBackColor = true;
            radioButton4.CheckedChanged += radioButton_CheckedChanged;
            // 
            // barista_zakaz
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1350, 621);
            Controls.Add(radioButton4);
            Controls.Add(pictureBox1);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(radioButton3);
            Controls.Add(radioButton2);
            Controls.Add(radioButton1);
            Controls.Add(dataGridView2);
            Controls.Add(dataGridView1);
            Controls.Add(label4);
            Controls.Add(button1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximumSize = new Size(1366, 660);
            MinimumSize = new Size(1364, 660);
            Name = "barista_zakaz";
            Text = "Кофейня";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label4;
        private Button button1;
        private DataGridView dataGridView1;
        private DataGridView dataGridView2;
        private RadioButton radioButton1;
        private RadioButton radioButton2;
        private RadioButton radioButton3;
        private Button button3;
        private Button button4;
        private PictureBox pictureBox1;
        private RadioButton radioButton4;
    }
}