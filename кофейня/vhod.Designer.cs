namespace кофейня
{
    partial class vhod
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(vhod));
            pictureBoxFon = new PictureBox();
            maskedTextBox1 = new MaskedTextBox();
            button2 = new Button();
            button1 = new Button();
            pictureBox2 = new PictureBox();
            pictureBox1 = new PictureBox();
            label1 = new Label();
            textBox1 = new TextBox();
            button3 = new Button();
            button4 = new Button();
            pictureBox3 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBoxFon).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            SuspendLayout();
            // 
            // pictureBoxFon
            // 
            pictureBoxFon.Image = (Image)resources.GetObject("pictureBoxFon.Image");
            pictureBoxFon.Location = new Point(-7, -30);
            pictureBoxFon.Name = "pictureBoxFon";
            pictureBoxFon.Size = new Size(1366, 659);
            pictureBoxFon.TabIndex = 3;
            pictureBoxFon.TabStop = false;
            // 
            // maskedTextBox1
            // 
            maskedTextBox1.BorderStyle = BorderStyle.None;
            maskedTextBox1.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Bold);
            maskedTextBox1.ForeColor = Color.FromArgb(132, 154, 157);
            maskedTextBox1.Location = new Point(487, 206);
            maskedTextBox1.Name = "maskedTextBox1";
            maskedTextBox1.Size = new Size(373, 31);
            maskedTextBox1.TabIndex = 18;
            maskedTextBox1.Text = "E-Mail";
            maskedTextBox1.Enter += maskedTextBox1_Enter;
            maskedTextBox1.KeyDown += maskedTextBox1_KeyDown;
            maskedTextBox1.Leave += maskedTextBox1_Leave;
            // 
            // button2
            // 
            button2.BackColor = Color.Transparent;
            button2.BackgroundImageLayout = ImageLayout.None;
            button2.FlatStyle = FlatStyle.Flat;
            button2.ForeColor = Color.Transparent;
            button2.Image = (Image)resources.GetObject("button2.Image");
            button2.Location = new Point(693, 375);
            button2.Name = "button2";
            button2.Size = new Size(187, 67);
            button2.TabIndex = 16;
            button2.UseVisualStyleBackColor = false;
            button2.Click += button2_Click;
            // 
            // button1
            // 
            button1.BackColor = Color.Transparent;
            button1.BackgroundImageLayout = ImageLayout.None;
            button1.FlatStyle = FlatStyle.Flat;
            button1.ForeColor = Color.Transparent;
            button1.Image = (Image)resources.GetObject("button1.Image");
            button1.Location = new Point(470, 375);
            button1.Name = "button1";
            button1.Size = new Size(187, 67);
            button1.TabIndex = 15;
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // pictureBox2
            // 
            pictureBox2.Image = (Image)resources.GetObject("pictureBox2.Image");
            pictureBox2.Location = new Point(470, 284);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(410, 66);
            pictureBox2.TabIndex = 14;
            pictureBox2.TabStop = false;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(470, 193);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(410, 66);
            pictureBox1.TabIndex = 13;
            pictureBox1.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Microsoft Sans Serif", 44F);
            label1.ForeColor = Color.FromArgb(202, 57, 0);
            label1.Location = new Point(586, 86);
            label1.Name = "label1";
            label1.Size = new Size(165, 67);
            label1.TabIndex = 12;
            label1.Text = "Вход";
            // 
            // textBox1
            // 
            textBox1.BorderStyle = BorderStyle.None;
            textBox1.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Bold);
            textBox1.ForeColor = Color.FromArgb(132, 154, 157);
            textBox1.Location = new Point(487, 298);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(373, 31);
            textBox1.TabIndex = 19;
            textBox1.Text = "Пароль";
            textBox1.Enter += textBox1_Enter;
            textBox1.KeyDown += textBox1_KeyDown;
            textBox1.Leave += textBox1_Leave;
            // 
            // button3
            // 
            button3.BackColor = Color.Transparent;
            button3.BackgroundImageLayout = ImageLayout.None;
            button3.FlatStyle = FlatStyle.Flat;
            button3.ForeColor = Color.Transparent;
            button3.Image = (Image)resources.GetObject("button3.Image");
            button3.Location = new Point(473, 466);
            button3.Name = "button3";
            button3.Size = new Size(184, 30);
            button3.TabIndex = 20;
            button3.UseVisualStyleBackColor = false;
            button3.Click += button3_Click;
            // 
            // button4
            // 
            button4.BackColor = Color.Transparent;
            button4.BackgroundImage = (Image)resources.GetObject("button4.BackgroundImage");
            button4.BackgroundImageLayout = ImageLayout.None;
            button4.FlatStyle = FlatStyle.Flat;
            button4.ForeColor = Color.Transparent;
            button4.Location = new Point(691, 470);
            button4.Name = "button4";
            button4.Size = new Size(199, 19);
            button4.TabIndex = 21;
            button4.UseVisualStyleBackColor = false;
            button4.Click += button4_Click;
            // 
            // pictureBox3
            // 
            pictureBox3.Image = (Image)resources.GetObject("pictureBox3.Image");
            pictureBox3.Location = new Point(673, 467);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(10, 29);
            pictureBox3.TabIndex = 22;
            pictureBox3.TabStop = false;
            // 
            // vhod
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1350, 621);
            Controls.Add(button4);
            Controls.Add(pictureBox3);
            Controls.Add(button3);
            Controls.Add(textBox1);
            Controls.Add(maskedTextBox1);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(pictureBox2);
            Controls.Add(pictureBox1);
            Controls.Add(label1);
            Controls.Add(pictureBoxFon);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximumSize = new Size(1366, 660);
            MinimumSize = new Size(1364, 660);
            Name = "vhod";
            Text = "Кофейня";
            ((System.ComponentModel.ISupportInitialize)pictureBoxFon).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBoxFon;
        private MaskedTextBox maskedTextBox1;
        private Button button2;
        private Button button1;
        private PictureBox pictureBox2;
        private PictureBox pictureBox1;
        private Label label1;
        private TextBox textBox1;
        private Button button3;
        private Button button4;
        private PictureBox pictureBox3;
    }
}