namespace Library_Management_System.User_Control
{
    partial class ReturnBooksControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dgvBorrowedBooks;
        private System.Windows.Forms.Button btnReturn;

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
            this.dgvBorrowedBooks = new System.Windows.Forms.DataGridView();
            this.btnReturn = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBorrowedBooks)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvBorrowedBooks
            // 
            this.dgvBorrowedBooks.AllowUserToAddRows = false;
            this.dgvBorrowedBooks.AllowUserToDeleteRows = false;
            this.dgvBorrowedBooks.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvBorrowedBooks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvBorrowedBooks.Location = new System.Drawing.Point(277, 196);
            this.dgvBorrowedBooks.Margin = new System.Windows.Forms.Padding(4);
            this.dgvBorrowedBooks.MultiSelect = false;
            this.dgvBorrowedBooks.Name = "dgvBorrowedBooks";
            this.dgvBorrowedBooks.ReadOnly = true;
            this.dgvBorrowedBooks.RowHeadersWidth = 51;
            this.dgvBorrowedBooks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvBorrowedBooks.Size = new System.Drawing.Size(1728, 887);
            this.dgvBorrowedBooks.TabIndex = 0;
            this.dgvBorrowedBooks.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvBorrowedBooks_CellContentClick);
            // 
            // btnReturn
            // 
            this.btnReturn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(181)))), ((int)(((byte)(139)))));
            this.btnReturn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnReturn.FlatAppearance.BorderSize = 0;
            this.btnReturn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReturn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReturn.Location = new System.Drawing.Point(294, 140);
            this.btnReturn.Margin = new System.Windows.Forms.Padding(4);
            this.btnReturn.Name = "btnReturn";
            this.btnReturn.Size = new System.Drawing.Size(196, 49);
            this.btnReturn.TabIndex = 1;
            this.btnReturn.Text = "Confirm";
            this.btnReturn.UseVisualStyleBackColor = false;
            this.btnReturn.Click += new System.EventHandler(this.btnReturn_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(288, 61);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(0, 32);
            this.label6.TabIndex = 8;
            this.label6.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(194)))), ((int)(((byte)(167)))), ((int)(((byte)(144)))));
            this.panel1.Controls.Add(this.label2);
            this.panel1.Location = new System.Drawing.Point(244, 28);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 51);
            this.panel1.TabIndex = 23;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(27, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 32);
            this.label2.TabIndex = 20;
            this.label2.Text = "Return";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(1763, 165);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(242, 22);
            this.txtSearch.TabIndex = 24;
            // 
            // ReturnBooksControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(212)))), ((int)(((byte)(201)))));
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btnReturn);
            this.Controls.Add(this.dgvBorrowedBooks);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ReturnBooksControl";
            this.Size = new System.Drawing.Size(1855, 811);
            this.Load += new System.EventHandler(this.ReturnBooksControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvBorrowedBooks)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSearch;
    }
}