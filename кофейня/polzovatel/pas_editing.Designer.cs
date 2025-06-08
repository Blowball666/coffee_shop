namespace кофейня.polzovatel
{
    partial class pas_editing
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(pas_editing));
            buttonCancel = new Button();
            buttonConfirm = new Button();
            textBoxCode = new TextBox();
            labelVerificationCode = new Label();
            SuspendLayout();
            // 
            // buttonCancel
            // 
            buttonCancel.FlatStyle = FlatStyle.Flat;
            buttonCancel.Font = new Font("Roboto", 16F);
            buttonCancel.ForeColor = Color.FromArgb(202, 57, 0);
            buttonCancel.Location = new Point(214, 111);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(170, 50);
            buttonCancel.TabIndex = 7;
            buttonCancel.Text = "Отмена";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // buttonConfirm
            // 
            buttonConfirm.BackColor = Color.FromArgb(202, 57, 0);
            buttonConfirm.FlatStyle = FlatStyle.Flat;
            buttonConfirm.Font = new Font("Roboto", 16F);
            buttonConfirm.ForeColor = Color.White;
            buttonConfirm.Location = new Point(24, 111);
            buttonConfirm.Name = "buttonConfirm";
            buttonConfirm.Size = new Size(170, 50);
            buttonConfirm.TabIndex = 6;
            buttonConfirm.Text = "Подтвердить";
            buttonConfirm.UseVisualStyleBackColor = false;
            buttonConfirm.Click += buttonConfirm_Click;
            // 
            // textBoxCode
            // 
            textBoxCode.BorderStyle = BorderStyle.FixedSingle;
            textBoxCode.Font = new Font("Roboto", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            textBoxCode.ForeColor = Color.FromArgb(132, 154, 157);
            textBoxCode.Location = new Point(24, 63);
            textBoxCode.Name = "textBoxCode";
            textBoxCode.Size = new Size(360, 33);
            textBoxCode.TabIndex = 5;
            // 
            // labelVerificationCode
            // 
            labelVerificationCode.AutoSize = true;
            labelVerificationCode.Font = new Font("Roboto", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            labelVerificationCode.ForeColor = Color.FromArgb(132, 154, 157);
            labelVerificationCode.Location = new Point(46, 9);
            labelVerificationCode.Name = "labelVerificationCode";
            labelVerificationCode.Size = new Size(312, 35);
            labelVerificationCode.TabIndex = 4;
            labelVerificationCode.Text = "Введите новый пароль:";
            // 
            // pas_editing
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(404, 181);
            Controls.Add(buttonCancel);
            Controls.Add(buttonConfirm);
            Controls.Add(textBoxCode);
            Controls.Add(labelVerificationCode);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximumSize = new Size(420, 220);
            MinimumSize = new Size(420, 220);
            Name = "pas_editing";
            Text = "Смена пароля";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button buttonCancel;
        private Button buttonConfirm;
        private TextBox textBoxCode;
        private Label labelVerificationCode;
    }
}