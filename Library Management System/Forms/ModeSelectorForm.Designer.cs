namespace Library_Management_System
{
    partial class ModeSelectorForm
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
            this.btnStudent = new System.Windows.Forms.Button();
            this.btnRegister = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnStudent
            // 
            this.btnStudent.BackColor = System.Drawing.Color.Transparent;
            this.btnStudent.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStudent.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStudent.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStudent.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnStudent.Location = new System.Drawing.Point(66, 106);
            this.btnStudent.Name = "btnStudent";
            this.btnStudent.Size = new System.Drawing.Size(406, 54);
            this.btnStudent.TabIndex = 2;
            this.btnStudent.TabStop = false;
            this.btnStudent.Text = "    Student log in";
            this.btnStudent.UseVisualStyleBackColor = false;
            this.btnStudent.Click += new System.EventHandler(this.btnStudent_Click_1);
            // 
            // btnRegister
            // 
            this.btnRegister.BackColor = System.Drawing.Color.Transparent;
            this.btnRegister.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRegister.FlatAppearance.BorderSize = 0;
            this.btnRegister.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRegister.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRegister.Location = new System.Drawing.Point(260, 208);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(212, 32);
            this.btnRegister.TabIndex = 6;
            this.btnRegister.TabStop = false;
            this.btnRegister.Text = "Register as a student";
            this.btnRegister.UseVisualStyleBackColor = false;
            this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click_1);
            // 
            // ModeSelectorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(549, 318);
            this.Controls.Add(this.btnRegister);
            this.Controls.Add(this.btnStudent);
            this.Name = "ModeSelectorForm";
            this.Text = "ModeSelectorForm";
            this.Load += new System.EventHandler(this.ModeSelectorForm_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnStudent;
        private System.Windows.Forms.Button btnRegister;
    }
}