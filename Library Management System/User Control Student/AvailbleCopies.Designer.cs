using System.Windows.Forms;
using System.Drawing;

namespace Library_Management_System.User_Control_Student
{
    partial class AvailbleCopies
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.lblSearchMessage = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lblSearchMessage
            // 
            this.lblSearchMessage.AutoSize = true;
            this.lblSearchMessage.BackColor = System.Drawing.Color.Transparent;
            this.lblSearchMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSearchMessage.Location = new System.Drawing.Point(876, 512);
            this.lblSearchMessage.Name = "lblSearchMessage";
            this.lblSearchMessage.Size = new System.Drawing.Size(0, 39);
            this.lblSearchMessage.TabIndex = 2;
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 28.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(37, 102);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(892, 61);
            this.textBox1.TabIndex = 5;
            // 
            // AvailbleCopies
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gainsboro;
            this.Controls.Add(this.lblSearchMessage);
            this.Controls.Add(this.textBox1);
            this.Name = "AvailbleCopies";
            this.Size = new System.Drawing.Size(1749, 959);
            this.Load += new System.EventHandler(this.AvailbleCopies_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label lblSearchMessage;
    }
}
