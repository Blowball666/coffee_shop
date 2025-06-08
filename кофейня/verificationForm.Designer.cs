namespace coffee
{
    partial class verificationForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(verificationForm));
            labelVerificationCode = new Label();
            textBox1 = new TextBox();
            buttonConfirm = new Button();
            buttonCancel = new Button();
            SuspendLayout();
            // 
            // labelVerificationCode
            // 
            labelVerificationCode.AutoSize = true;
            labelVerificationCode.Font = new Font("Roboto", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            labelVerificationCode.ForeColor = Color.FromArgb(132, 154, 157);
            labelVerificationCode.Location = new Point(10, 12);
            labelVerificationCode.Name = "labelVerificationCode";
            labelVerificationCode.Size = new Size(386, 35);
            labelVerificationCode.TabIndex = 0;
            labelVerificationCode.Text = "Введите код подтверждения:";
            // 
            // textBox1
            // 
            textBox1.BorderStyle = BorderStyle.FixedSingle;
            textBox1.Font = new Font("Roboto", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            textBox1.ForeColor = Color.FromArgb(132, 154, 157);
            textBox1.Location = new Point(22, 66);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(360, 33);
            textBox1.TabIndex = 1;
            // 
            // buttonConfirm
            // 
            buttonConfirm.BackColor = Color.FromArgb(202, 57, 0);
            buttonConfirm.FlatStyle = FlatStyle.Flat;
            buttonConfirm.Font = new Font("Roboto", 16F);
            buttonConfirm.ForeColor = Color.White;
            buttonConfirm.Location = new Point(22, 114);
            buttonConfirm.Name = "buttonConfirm";
            buttonConfirm.Size = new Size(170, 50);
            buttonConfirm.TabIndex = 2;
            buttonConfirm.Text = "Подтвердить";
            buttonConfirm.UseVisualStyleBackColor = false;
            buttonConfirm.Click += buttonConfirm_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.FlatStyle = FlatStyle.Flat;
            buttonCancel.Font = new Font("Roboto", 16F);
            buttonCancel.ForeColor = Color.FromArgb(202, 57, 0);
            buttonCancel.Location = new Point(212, 114);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(170, 50);
            buttonCancel.TabIndex = 3;
            buttonCancel.Text = "Отменить";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // verificationForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(404, 181);
            Controls.Add(buttonCancel);
            Controls.Add(buttonConfirm);
            Controls.Add(textBox1);
            Controls.Add(labelVerificationCode);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximumSize = new Size(420, 220);
            MinimumSize = new Size(420, 220);
            Name = "verificationForm";
            Text = "Подтверждение Email";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelVerificationCode;
        private TextBox textBox1;
        private Button buttonConfirm;
        private Button buttonCancel;
    }
}