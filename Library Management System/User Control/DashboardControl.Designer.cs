using System.Drawing;
using System.Windows.Forms;

namespace Library_Management_System.User_Control
{

    partial class DashboardControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }


        private void InitializeComponent()
        {
            this.lblGreeting = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.pnlTotalBooks = new System.Windows.Forms.Panel();
            this.lblTotalBooksValue = new System.Windows.Forms.Label();
            this.lblTotalBooks = new System.Windows.Forms.Label();
            this.picTotalBooks = new System.Windows.Forms.PictureBox();
            this.pnlBorrowedBooks = new System.Windows.Forms.Panel();
            this.lblBorrowedBooksValue = new System.Windows.Forms.Label();
            this.lblBorrowedBooks = new System.Windows.Forms.Label();
            this.picBorrowedBooks = new System.Windows.Forms.PictureBox();
            this.pnlAvailableBooks = new System.Windows.Forms.Panel();
            this.lblAvailableBooksValue = new System.Windows.Forms.Label();
            this.lblAvailableBooks = new System.Windows.Forms.Label();
            this.picAvailableBooks = new System.Windows.Forms.PictureBox();
            this.pnlActiveMembers = new System.Windows.Forms.Panel();
            this.lblActiveMembersValue = new System.Windows.Forms.Label();
            this.lblActiveMembers = new System.Windows.Forms.Label();
            this.picActiveMembers = new System.Windows.Forms.PictureBox();
            this.pnlOverdueBooks = new System.Windows.Forms.Panel();
            this.lblOverdueBooksValue = new System.Windows.Forms.Label();
            this.lblOverdueBooks = new System.Windows.Forms.Label();
            this.picOverdueBooks = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panelMostBorrowed = new System.Windows.Forms.Panel();
            this.pnlTotalBooks.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picTotalBooks)).BeginInit();
            this.pnlBorrowedBooks.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBorrowedBooks)).BeginInit();
            this.pnlAvailableBooks.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picAvailableBooks)).BeginInit();
            this.pnlActiveMembers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picActiveMembers)).BeginInit();
            this.pnlOverdueBooks.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picOverdueBooks)).BeginInit();
            this.SuspendLayout();
            // 
            // lblGreeting
            // 
            this.lblGreeting.AutoSize = true;
            this.lblGreeting.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGreeting.Location = new System.Drawing.Point(747, 0);
            this.lblGreeting.Name = "lblGreeting";
            this.lblGreeting.Size = new System.Drawing.Size(326, 32);
            this.lblGreeting.TabIndex = 0;
            this.lblGreeting.Text = "Good morning, ADMIN!";
            this.lblGreeting.Click += new System.EventHandler(this.lblGreeting_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(221, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(309, 32);
            this.label1.TabIndex = 1;
            this.label1.Text = "Welcome to Dashboard";
            // 
            // pnlTotalBooks
            // 
            this.pnlTotalBooks.BackColor = System.Drawing.Color.White;
            this.pnlTotalBooks.Controls.Add(this.lblTotalBooksValue);
            this.pnlTotalBooks.Controls.Add(this.lblTotalBooks);
            this.pnlTotalBooks.Controls.Add(this.picTotalBooks);
            this.pnlTotalBooks.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pnlTotalBooks.Location = new System.Drawing.Point(227, 107);
            this.pnlTotalBooks.Name = "pnlTotalBooks";
            this.pnlTotalBooks.Size = new System.Drawing.Size(225, 91);
            this.pnlTotalBooks.TabIndex = 7;
            this.pnlTotalBooks.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlTotalBooks_Paint_1);
            // 
            // lblTotalBooksValue
            // 
            this.lblTotalBooksValue.AutoSize = true;
            this.lblTotalBooksValue.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblTotalBooksValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalBooksValue.Location = new System.Drawing.Point(146, 33);
            this.lblTotalBooksValue.Name = "lblTotalBooksValue";
            this.lblTotalBooksValue.Size = new System.Drawing.Size(18, 20);
            this.lblTotalBooksValue.TabIndex = 14;
            this.lblTotalBooksValue.Text = "0";
            this.lblTotalBooksValue.Click += new System.EventHandler(this.label2_Click_1);
            // 
            // lblTotalBooks
            // 
            this.lblTotalBooks.AutoSize = true;
            this.lblTotalBooks.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblTotalBooks.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalBooks.Location = new System.Drawing.Point(101, 10);
            this.lblTotalBooks.Name = "lblTotalBooks";
            this.lblTotalBooks.Size = new System.Drawing.Size(98, 20);
            this.lblTotalBooks.TabIndex = 9;
            this.lblTotalBooks.Text = "Total Books\r\n";
            this.lblTotalBooks.Click += new System.EventHandler(this.label2_Click);
            // 
            // picTotalBooks
            // 
            this.picTotalBooks.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.picTotalBooks.Image = global::Library_Management_System.Properties.Resources.tb1;
            this.picTotalBooks.Location = new System.Drawing.Point(29, 27);
            this.picTotalBooks.Name = "picTotalBooks";
            this.picTotalBooks.Size = new System.Drawing.Size(48, 47);
            this.picTotalBooks.TabIndex = 6;
            this.picTotalBooks.TabStop = false;
            this.picTotalBooks.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // pnlBorrowedBooks
            // 
            this.pnlBorrowedBooks.BackColor = System.Drawing.Color.White;
            this.pnlBorrowedBooks.Controls.Add(this.lblBorrowedBooksValue);
            this.pnlBorrowedBooks.Controls.Add(this.lblBorrowedBooks);
            this.pnlBorrowedBooks.Controls.Add(this.picBorrowedBooks);
            this.pnlBorrowedBooks.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pnlBorrowedBooks.Location = new System.Drawing.Point(490, 107);
            this.pnlBorrowedBooks.Name = "pnlBorrowedBooks";
            this.pnlBorrowedBooks.Size = new System.Drawing.Size(225, 91);
            this.pnlBorrowedBooks.TabIndex = 10;
            // 
            // lblBorrowedBooksValue
            // 
            this.lblBorrowedBooksValue.AutoSize = true;
            this.lblBorrowedBooksValue.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblBorrowedBooksValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBorrowedBooksValue.Location = new System.Drawing.Point(150, 33);
            this.lblBorrowedBooksValue.Name = "lblBorrowedBooksValue";
            this.lblBorrowedBooksValue.Size = new System.Drawing.Size(18, 20);
            this.lblBorrowedBooksValue.TabIndex = 15;
            this.lblBorrowedBooksValue.Text = "0";
            this.lblBorrowedBooksValue.Click += new System.EventHandler(this.label3_Click_1);
            // 
            // lblBorrowedBooks
            // 
            this.lblBorrowedBooks.AutoSize = true;
            this.lblBorrowedBooks.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBorrowedBooks.Location = new System.Drawing.Point(120, 10);
            this.lblBorrowedBooks.Name = "lblBorrowedBooks";
            this.lblBorrowedBooks.Size = new System.Drawing.Size(81, 20);
            this.lblBorrowedBooks.TabIndex = 9;
            this.lblBorrowedBooks.Text = "Borrowed\r\n";
            // 
            // picBorrowedBooks
            // 
            this.picBorrowedBooks.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.picBorrowedBooks.Image = global::Library_Management_System.Properties.Resources.b1;
            this.picBorrowedBooks.Location = new System.Drawing.Point(26, 27);
            this.picBorrowedBooks.Name = "picBorrowedBooks";
            this.picBorrowedBooks.Size = new System.Drawing.Size(48, 47);
            this.picBorrowedBooks.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picBorrowedBooks.TabIndex = 6;
            this.picBorrowedBooks.TabStop = false;
            this.picBorrowedBooks.Click += new System.EventHandler(this.pictureBox6_Click);
            // 
            // pnlAvailableBooks
            // 
            this.pnlAvailableBooks.BackColor = System.Drawing.Color.White;
            this.pnlAvailableBooks.Controls.Add(this.lblAvailableBooksValue);
            this.pnlAvailableBooks.Controls.Add(this.lblAvailableBooks);
            this.pnlAvailableBooks.Controls.Add(this.picAvailableBooks);
            this.pnlAvailableBooks.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pnlAvailableBooks.Location = new System.Drawing.Point(748, 107);
            this.pnlAvailableBooks.Name = "pnlAvailableBooks";
            this.pnlAvailableBooks.Size = new System.Drawing.Size(230, 91);
            this.pnlAvailableBooks.TabIndex = 12;
            // 
            // lblAvailableBooksValue
            // 
            this.lblAvailableBooksValue.AutoSize = true;
            this.lblAvailableBooksValue.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblAvailableBooksValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAvailableBooksValue.Location = new System.Drawing.Point(147, 33);
            this.lblAvailableBooksValue.Name = "lblAvailableBooksValue";
            this.lblAvailableBooksValue.Size = new System.Drawing.Size(18, 20);
            this.lblAvailableBooksValue.TabIndex = 16;
            this.lblAvailableBooksValue.Text = "0";
            this.lblAvailableBooksValue.Click += new System.EventHandler(this.label4_Click);
            // 
            // lblAvailableBooks
            // 
            this.lblAvailableBooks.AutoSize = true;
            this.lblAvailableBooks.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAvailableBooks.Location = new System.Drawing.Point(116, 10);
            this.lblAvailableBooks.Name = "lblAvailableBooks";
            this.lblAvailableBooks.Size = new System.Drawing.Size(76, 20);
            this.lblAvailableBooks.TabIndex = 9;
            this.lblAvailableBooks.Text = "Available\r\n";
            // 
            // picAvailableBooks
            // 
            this.picAvailableBooks.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.picAvailableBooks.Image = global::Library_Management_System.Properties.Resources.av1;
            this.picAvailableBooks.Location = new System.Drawing.Point(30, 27);
            this.picAvailableBooks.Name = "picAvailableBooks";
            this.picAvailableBooks.Size = new System.Drawing.Size(48, 47);
            this.picAvailableBooks.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picAvailableBooks.TabIndex = 6;
            this.picAvailableBooks.TabStop = false;
            this.picAvailableBooks.Click += new System.EventHandler(this.picAvailableBooks_Click);
            // 
            // pnlActiveMembers
            // 
            this.pnlActiveMembers.BackColor = System.Drawing.Color.White;
            this.pnlActiveMembers.Controls.Add(this.lblActiveMembersValue);
            this.pnlActiveMembers.Controls.Add(this.lblActiveMembers);
            this.pnlActiveMembers.Controls.Add(this.picActiveMembers);
            this.pnlActiveMembers.Location = new System.Drawing.Point(1017, 107);
            this.pnlActiveMembers.Name = "pnlActiveMembers";
            this.pnlActiveMembers.Size = new System.Drawing.Size(225, 91);
            this.pnlActiveMembers.TabIndex = 13;
            // 
            // lblActiveMembersValue
            // 
            this.lblActiveMembersValue.AutoSize = true;
            this.lblActiveMembersValue.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblActiveMembersValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblActiveMembersValue.Location = new System.Drawing.Point(159, 33);
            this.lblActiveMembersValue.Name = "lblActiveMembersValue";
            this.lblActiveMembersValue.Size = new System.Drawing.Size(18, 20);
            this.lblActiveMembersValue.TabIndex = 15;
            this.lblActiveMembersValue.Text = "0";
            this.lblActiveMembersValue.Click += new System.EventHandler(this.label5_Click);
            // 
            // lblActiveMembers
            // 
            this.lblActiveMembers.AutoSize = true;
            this.lblActiveMembers.BackColor = System.Drawing.Color.Transparent;
            this.lblActiveMembers.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblActiveMembers.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblActiveMembers.Location = new System.Drawing.Point(81, 10);
            this.lblActiveMembers.Name = "lblActiveMembers";
            this.lblActiveMembers.Size = new System.Drawing.Size(130, 20);
            this.lblActiveMembers.TabIndex = 9;
            this.lblActiveMembers.Text = "Active Members";
            // 
            // picActiveMembers
            // 
            this.picActiveMembers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.picActiveMembers.Image = global::Library_Management_System.Properties.Resources.ac1;
            this.picActiveMembers.Location = new System.Drawing.Point(27, 27);
            this.picActiveMembers.Name = "picActiveMembers";
            this.picActiveMembers.Size = new System.Drawing.Size(48, 47);
            this.picActiveMembers.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picActiveMembers.TabIndex = 6;
            this.picActiveMembers.TabStop = false;
            this.picActiveMembers.Click += new System.EventHandler(this.picActiveMembers_Click);
            // 
            // pnlOverdueBooks
            // 
            this.pnlOverdueBooks.BackColor = System.Drawing.Color.White;
            this.pnlOverdueBooks.Controls.Add(this.lblOverdueBooksValue);
            this.pnlOverdueBooks.Controls.Add(this.lblOverdueBooks);
            this.pnlOverdueBooks.Controls.Add(this.picOverdueBooks);
            this.pnlOverdueBooks.Location = new System.Drawing.Point(1282, 107);
            this.pnlOverdueBooks.Name = "pnlOverdueBooks";
            this.pnlOverdueBooks.Size = new System.Drawing.Size(225, 91);
            this.pnlOverdueBooks.TabIndex = 10;
            // 
            // lblOverdueBooksValue
            // 
            this.lblOverdueBooksValue.AutoSize = true;
            this.lblOverdueBooksValue.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblOverdueBooksValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOverdueBooksValue.Location = new System.Drawing.Point(149, 33);
            this.lblOverdueBooksValue.Name = "lblOverdueBooksValue";
            this.lblOverdueBooksValue.Size = new System.Drawing.Size(18, 20);
            this.lblOverdueBooksValue.TabIndex = 16;
            this.lblOverdueBooksValue.Text = "0";
            this.lblOverdueBooksValue.Click += new System.EventHandler(this.label6_Click);
            // 
            // lblOverdueBooks
            // 
            this.lblOverdueBooks.AutoSize = true;
            this.lblOverdueBooks.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOverdueBooks.Location = new System.Drawing.Point(117, 10);
            this.lblOverdueBooks.Name = "lblOverdueBooks";
            this.lblOverdueBooks.Size = new System.Drawing.Size(72, 20);
            this.lblOverdueBooks.TabIndex = 9;
            this.lblOverdueBooks.Text = "Overdue\r\n";
            // 
            // picOverdueBooks
            // 
            this.picOverdueBooks.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.picOverdueBooks.Image = global::Library_Management_System.Properties.Resources.ov1;
            this.picOverdueBooks.Location = new System.Drawing.Point(30, 27);
            this.picOverdueBooks.Name = "picOverdueBooks";
            this.picOverdueBooks.Size = new System.Drawing.Size(48, 47);
            this.picOverdueBooks.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picOverdueBooks.TabIndex = 6;
            this.picOverdueBooks.TabStop = false;
            this.picOverdueBooks.Click += new System.EventHandler(this.picOverdueBooks_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(224, 76);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(106, 15);
            this.label2.TabIndex = 14;
            this.label2.Text = "Admin/Dashboard";
            // 
            // panelMostBorrowed
            // 
            this.panelMostBorrowed.AutoScroll = true;
            this.panelMostBorrowed.BackColor = System.Drawing.Color.White;
            this.panelMostBorrowed.Location = new System.Drawing.Point(833, 561);
            this.panelMostBorrowed.Name = "panelMostBorrowed";
            this.panelMostBorrowed.Size = new System.Drawing.Size(425, 309);
            this.panelMostBorrowed.TabIndex = 15;
            this.panelMostBorrowed.Paint += new System.Windows.Forms.PaintEventHandler(this.panelMostBorrowed_Paint);
            // 
            // DashboardControl
            // 
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.Controls.Add(this.panelMostBorrowed);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblGreeting);
            this.Controls.Add(this.pnlOverdueBooks);
            this.Controls.Add(this.pnlActiveMembers);
            this.Controls.Add(this.pnlAvailableBooks);
            this.Controls.Add(this.pnlBorrowedBooks);
            this.Controls.Add(this.pnlTotalBooks);
            this.Controls.Add(this.label1);
            this.Name = "DashboardControl";
            this.Size = new System.Drawing.Size(1814, 995);
            this.Load += new System.EventHandler(this.DashboardControl_Load);
            this.pnlTotalBooks.ResumeLayout(false);
            this.pnlTotalBooks.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picTotalBooks)).EndInit();
            this.pnlBorrowedBooks.ResumeLayout(false);
            this.pnlBorrowedBooks.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBorrowedBooks)).EndInit();
            this.pnlAvailableBooks.ResumeLayout(false);
            this.pnlAvailableBooks.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picAvailableBooks)).EndInit();
            this.pnlActiveMembers.ResumeLayout(false);
            this.pnlActiveMembers.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picActiveMembers)).EndInit();
            this.pnlOverdueBooks.ResumeLayout(false);
            this.pnlOverdueBooks.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picOverdueBooks)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private Label lblGreeting;
        private Label label1;
        private PictureBox picTotalBooks;
        private Panel pnlTotalBooks;
        private Label lblTotalBooks;
        private Panel pnlBorrowedBooks;
        private Label lblBorrowedBooks;
        private PictureBox picBorrowedBooks;
        private Panel pnlAvailableBooks;
        private Label lblAvailableBooks;
        private PictureBox picAvailableBooks;
        private Panel pnlActiveMembers;
        private Label lblActiveMembers;
        private PictureBox picActiveMembers;
        private Panel pnlOverdueBooks;
        private Label lblOverdueBooks;
        private PictureBox picOverdueBooks;
        private Label lblTotalBooksValue;
        private Label lblBorrowedBooksValue;
        private Label lblAvailableBooksValue;
        private Label lblActiveMembersValue;
        private Label lblOverdueBooksValue;
        private Label label2;
        private Panel panelMostBorrowed;
    }
}