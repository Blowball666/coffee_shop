namespace кофейня.polzovatel
{
    partial class vkladki
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(vkladki));
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Roboto", 24F);
            label1.ForeColor = Color.FromArgb(202, 57, 0);
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(142, 43);
            label1.TabIndex = 29;
            label1.Text = "Аккаунт";
            label1.Click += label1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Roboto", 24F);
            label2.ForeColor = Color.FromArgb(202, 57, 0);
            label2.Location = new Point(12, 168);
            label2.Name = "label2";
            label2.Size = new Size(146, 43);
            label2.TabIndex = 30;
            label2.Text = "Корзина";
            label2.Click += label2_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = Color.Transparent;
            label3.Font = new Font("Roboto", 24F);
            label3.ForeColor = Color.FromArgb(202, 57, 0);
            label3.Location = new Point(12, 246);
            label3.Name = "label3";
            label3.Size = new Size(206, 43);
            label3.TabIndex = 31;
            label3.Text = "Отложенное";
            label3.Click += label3_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.BackColor = Color.Transparent;
            label4.Font = new Font("Roboto", 24F);
            label4.ForeColor = Color.FromArgb(202, 57, 0);
            label4.Location = new Point(12, 87);
            label4.Name = "label4";
            label4.Size = new Size(108, 43);
            label4.TabIndex = 32;
            label4.Text = "Меню";
            label4.Click += label4_Click;
            // 
            // vkladki
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(230, 317);
            ControlBox = false;
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximumSize = new Size(246, 333);
            MinimumSize = new Size(246, 333);
            Name = "vkladki";
            TransparencyKey = Color.OrangeRed;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
    }
}