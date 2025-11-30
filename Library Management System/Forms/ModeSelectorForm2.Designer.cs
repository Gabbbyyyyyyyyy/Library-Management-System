namespace Library_Management_System.Forms
{
    partial class ModeSelectorForm2
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
            this.btnAdmin = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnAdmin
            // 
            this.btnAdmin.BackColor = System.Drawing.Color.Transparent;
            this.btnAdmin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAdmin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdmin.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAdmin.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAdmin.Location = new System.Drawing.Point(71, 185);
            this.btnAdmin.Name = "btnAdmin";
            this.btnAdmin.Size = new System.Drawing.Size(406, 54);
            this.btnAdmin.TabIndex = 1;
            this.btnAdmin.TabStop = false;
            this.btnAdmin.Text = "Admin log in";
            this.btnAdmin.UseVisualStyleBackColor = false;
            this.btnAdmin.Click += new System.EventHandler(this.btnAdmin_Click);
            // 
            // ModeSelectorForm2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(549, 425);
            this.Controls.Add(this.btnAdmin);
            this.Name = "ModeSelectorForm2";
            this.Text = "ModeSelectorForm2";
            this.Load += new System.EventHandler(this.ModeSelectorForm2_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnAdmin;
    }
}
