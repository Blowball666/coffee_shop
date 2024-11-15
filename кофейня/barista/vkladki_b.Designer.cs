namespace кофейня.barista
{
    partial class vkladki_b
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(vkladki_b));
            label1 = new Label();
            label2 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Roboto", 24F);
            label1.ForeColor = Color.FromArgb(202, 57, 0);
            label1.Location = new Point(38, 94);
            label1.Name = "label1";
            label1.Size = new Size(135, 43);
            label1.TabIndex = 37;
            label1.Text = "Заказы";
            label1.Click += label1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Roboto", 24F);
            label2.ForeColor = Color.FromArgb(202, 57, 0);
            label2.Location = new Point(38, 166);
            label2.Name = "label2";
            label2.Size = new Size(117, 43);
            label2.TabIndex = 36;
            label2.Text = "Выход";
            label2.Click += label2_Click;
            // 
            // vkladki_b
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(230, 317);
            ControlBox = false;
            Controls.Add(label1);
            Controls.Add(label2);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximumSize = new Size(246, 333);
            MinimumSize = new Size(246, 333);
            Name = "vkladki_b";
            ShowIcon = false;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
    }
}