namespace Library_Management_System.User_Control_Student
{
    partial class Borrowing
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dgvBorrowedBooks;
        private System.Windows.Forms.Button btnReturnBook;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.dgvBorrowedBooks = new System.Windows.Forms.DataGridView();
            this.btnReturnBook = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
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
            this.dgvBorrowedBooks.Location = new System.Drawing.Point(4, 246);
            this.dgvBorrowedBooks.MultiSelect = false;
            this.dgvBorrowedBooks.Name = "dgvBorrowedBooks";
            this.dgvBorrowedBooks.ReadOnly = true;
            this.dgvBorrowedBooks.RowHeadersWidth = 51;
            this.dgvBorrowedBooks.RowTemplate.Height = 24;
            this.dgvBorrowedBooks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvBorrowedBooks.Size = new System.Drawing.Size(1728, 890);
            this.dgvBorrowedBooks.TabIndex = 0;
            this.dgvBorrowedBooks.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvBorrowedBooks_CellContentClick);
            // 
            // btnReturnBook
            // 
            this.btnReturnBook.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnReturnBook.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnReturnBook.FlatAppearance.BorderSize = 0;
            this.btnReturnBook.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReturnBook.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReturnBook.Location = new System.Drawing.Point(14, 177);
            this.btnReturnBook.Name = "btnReturnBook";
            this.btnReturnBook.Size = new System.Drawing.Size(180, 63);
            this.btnReturnBook.TabIndex = 1;
            this.btnReturnBook.Text = "Return Selected Book";
            this.btnReturnBook.UseVisualStyleBackColor = false;
            this.btnReturnBook.Click += new System.EventHandler(this.BtnReturnBook_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(194)))), ((int)(((byte)(167)))), ((int)(((byte)(144)))));
            this.panel1.Controls.Add(this.label2);
            this.panel1.Location = new System.Drawing.Point(3, 14);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 51);
            this.panel1.TabIndex = 24;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(27, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(119, 32);
            this.label2.TabIndex = 20;
            this.label2.Text = "Records";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // Borrowing
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(212)))), ((int)(((byte)(201)))));
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnReturnBook);
            this.Controls.Add(this.dgvBorrowedBooks);
            this.Name = "Borrowing";
            this.Size = new System.Drawing.Size(1755, 887);
            this.Load += new System.EventHandler(this.Borrowing_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvBorrowedBooks)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
    }
}
