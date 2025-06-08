namespace кофейня
{
    partial class email
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(email));
            buttonCancel = new Button();
            buttonConfirm = new Button();
            textBox1 = new TextBox();
            label1 = new Label();
            SuspendLayout();
            // 
            // buttonCancel
            // 
            buttonCancel.FlatStyle = FlatStyle.Flat;
            buttonCancel.Font = new Font("Roboto", 16F);
            buttonCancel.ForeColor = Color.FromArgb(202, 57, 0);
            buttonCancel.Location = new Point(211, 116);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(170, 50);
            buttonCancel.TabIndex = 7;
            buttonCancel.Text = "Отменить";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // buttonConfirm
            // 
            buttonConfirm.BackColor = Color.FromArgb(202, 57, 0);
            buttonConfirm.FlatStyle = FlatStyle.Flat;
            buttonConfirm.Font = new Font("Roboto", 16F);
            buttonConfirm.ForeColor = Color.White;
            buttonConfirm.Location = new Point(21, 116);
            buttonConfirm.Name = "buttonConfirm";
            buttonConfirm.Size = new Size(170, 50);
            buttonConfirm.TabIndex = 6;
            buttonConfirm.Text = "Далее";
            buttonConfirm.UseVisualStyleBackColor = false;
            buttonConfirm.Click += buttonConfirm_Click;
            // 
            // textBox1
            // 
            textBox1.BorderStyle = BorderStyle.FixedSingle;
            textBox1.Font = new Font("Roboto", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            textBox1.ForeColor = Color.FromArgb(132, 154, 157);
            textBox1.Location = new Point(21, 68);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(360, 33);
            textBox1.TabIndex = 5;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Roboto", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label1.ForeColor = Color.FromArgb(132, 154, 157);
            label1.Location = new Point(9, 14);
            label1.Name = "label1";
            label1.Size = new Size(207, 35);
            label1.TabIndex = 4;
            label1.Text = "Введите почту:";
            // 
            // email
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(404, 181);
            Controls.Add(buttonCancel);
            Controls.Add(buttonConfirm);
            Controls.Add(textBox1);
            Controls.Add(label1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximumSize = new Size(420, 220);
            MinimumSize = new Size(420, 220);
            Name = "email";
            Text = "Восстановление пароля";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button buttonCancel;
        private Button buttonConfirm;
        private TextBox textBox1;
        private Label label1;
    }
}