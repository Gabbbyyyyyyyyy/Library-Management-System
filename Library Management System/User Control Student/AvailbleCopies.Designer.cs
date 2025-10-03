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
            this.dgvAvailableCopies = new System.Windows.Forms.DataGridView();
            this.lblSearchMessage = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAvailableCopies)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvAvailableCopies
            // 
            this.dgvAvailableCopies.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAvailableCopies.Location = new System.Drawing.Point(3, 326);
            this.dgvAvailableCopies.Name = "dgvAvailableCopies";
            this.dgvAvailableCopies.RowHeadersWidth = 51;
            this.dgvAvailableCopies.RowTemplate.Height = 24;
            this.dgvAvailableCopies.Size = new System.Drawing.Size(1748, 782);
            this.dgvAvailableCopies.TabIndex = 0;
            this.dgvAvailableCopies.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvAvailableCopies_CellContentClick);
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
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(48, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(228, 32);
            this.label1.TabIndex = 4;
            this.label1.Text = "Available Copies";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(1449, 293);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(230, 27);
            this.textBox1.TabIndex = 5;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // AvailbleCopies
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblSearchMessage);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.dgvAvailableCopies);
            this.Name = "AvailbleCopies";
            this.Size = new System.Drawing.Size(1749, 959);
            this.Load += new System.EventHandler(this.AvailbleCopies_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAvailableCopies)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvAvailableCopies;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label lblSearchMessage;
        private Label label1;
     
    }
}
