namespace кофейня.admin
{
    partial class vkladki_a
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(vkladki_a));
            label1 = new Label();
            label3 = new Label();
            label2 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Roboto", 22F);
            label1.ForeColor = Color.FromArgb(202, 57, 0);
            label1.Location = new Point(14, 45);
            label1.MaximumSize = new Size(246, 271);
            label1.Name = "label1";
            label1.Size = new Size(204, 39);
            label1.TabIndex = 34;
            label1.Text = "Ассортимент";
            label1.Click += label1_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = Color.Transparent;
            label3.Font = new Font("Roboto", 22F);
            label3.ForeColor = Color.FromArgb(202, 57, 0);
            label3.Location = new Point(14, 126);
            label3.MaximumSize = new Size(246, 271);
            label3.Name = "label3";
            label3.Size = new Size(110, 39);
            label3.TabIndex = 33;
            label3.Text = "Выход";
            label3.Click += label3_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Roboto", 22F);
            label2.ForeColor = Color.FromArgb(202, 57, 0);
            label2.Location = new Point(14, 86);
            label2.MaximumSize = new Size(246, 271);
            label2.Name = "label2";
            label2.Size = new Size(135, 39);
            label2.TabIndex = 35;
            label2.Text = "Бариста";
            label2.Click += label2_Click;
            // 
            // vkladki_a
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(230, 206);
            ControlBox = false;
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(label3);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "vkladki_a";
            ShowIcon = false;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label3;
        private Label label2;
    }
}