namespace кофейня.admin
{
    partial class syrups
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(syrups));
            buttonCancel = new Button();
            buttonConfirm = new Button();
            textBox1 = new TextBox();
            labelVerificationCode = new Label();
            textBox2 = new TextBox();
            label1 = new Label();
            SuspendLayout();
            // 
            // buttonCancel
            // 
            buttonCancel.FlatStyle = FlatStyle.Flat;
            buttonCancel.Font = new Font("Roboto", 16F);
            buttonCancel.ForeColor = Color.FromArgb(202, 57, 0);
            buttonCancel.Location = new Point(211, 192);
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
            buttonConfirm.Location = new Point(21, 192);
            buttonConfirm.Name = "buttonConfirm";
            buttonConfirm.Size = new Size(170, 50);
            buttonConfirm.TabIndex = 6;
            buttonConfirm.Text = "Подтвердить";
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
            // labelVerificationCode
            // 
            labelVerificationCode.AutoSize = true;
            labelVerificationCode.Font = new Font("Roboto", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            labelVerificationCode.ForeColor = Color.FromArgb(132, 154, 157);
            labelVerificationCode.Location = new Point(21, 30);
            labelVerificationCode.Name = "labelVerificationCode";
            labelVerificationCode.Size = new Size(144, 35);
            labelVerificationCode.TabIndex = 4;
            labelVerificationCode.Text = "Название:";
            // 
            // textBox2
            // 
            textBox2.BorderStyle = BorderStyle.FixedSingle;
            textBox2.Font = new Font("Roboto", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            textBox2.ForeColor = Color.FromArgb(132, 154, 157);
            textBox2.Location = new Point(21, 142);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(360, 33);
            textBox2.TabIndex = 9;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Roboto", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label1.ForeColor = Color.FromArgb(132, 154, 157);
            label1.Location = new Point(21, 104);
            label1.Name = "label1";
            label1.Size = new Size(87, 35);
            label1.TabIndex = 8;
            label1.Text = "Цена:";
            // 
            // syrups
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(404, 287);
            Controls.Add(textBox2);
            Controls.Add(label1);
            Controls.Add(buttonCancel);
            Controls.Add(buttonConfirm);
            Controls.Add(textBox1);
            Controls.Add(labelVerificationCode);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "syrups";
            Text = "Изменение/добавление сиропов";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button buttonCancel;
        private Button buttonConfirm;
        private TextBox textBox1;
        private Label labelVerificationCode;
        private TextBox textBox2;
        private Label label1;
    }
}