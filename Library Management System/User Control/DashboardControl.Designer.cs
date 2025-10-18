using System.Drawing;
using System.Windows.Forms;

namespace Library_Management_System.User_Control
{

    partial class DashboardControl
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel pnlTotalBooks;
        private System.Windows.Forms.Panel pnlBorrowedBooks;
        private System.Windows.Forms.Panel pnlAvailableBooks;
        private System.Windows.Forms.Panel pnlActiveMembers;
        private System.Windows.Forms.Panel pnlOverdueBooks;

        private System.Windows.Forms.Label lblTotalBooks;
        private System.Windows.Forms.Label lblBorrowedBooks;
        private System.Windows.Forms.Label lblAvailableBooks;
        private System.Windows.Forms.Label lblActiveMembers;
        private System.Windows.Forms.Label lblOverdueBooks;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }


        private void InitializeComponent()
        {
            this.pnlTotalBooks = new System.Windows.Forms.Panel();
            this.lblTotalBooks = new System.Windows.Forms.Label();
            this.pnlBorrowedBooks = new System.Windows.Forms.Panel();
            this.lblBorrowedBooks = new System.Windows.Forms.Label();
            this.pnlAvailableBooks = new System.Windows.Forms.Panel();
            this.lblAvailableBooks = new System.Windows.Forms.Label();
            this.pnlActiveMembers = new System.Windows.Forms.Panel();
            this.lblActiveMembers = new System.Windows.Forms.Label();
            this.pnlOverdueBooks = new System.Windows.Forms.Panel();
            this.lblOverdueBooks = new System.Windows.Forms.Label();
            this.lblGreeting = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pnlTotalBooks.SuspendLayout();
            this.pnlBorrowedBooks.SuspendLayout();
            this.pnlAvailableBooks.SuspendLayout();
            this.pnlActiveMembers.SuspendLayout();
            this.pnlOverdueBooks.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTotalBooks
            // 
            this.pnlTotalBooks.BackColor = System.Drawing.Color.LightBlue;
            this.pnlTotalBooks.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlTotalBooks.Controls.Add(this.lblTotalBooks);
            this.pnlTotalBooks.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pnlTotalBooks.Location = new System.Drawing.Point(276, 195);
            this.pnlTotalBooks.Name = "pnlTotalBooks";
            this.pnlTotalBooks.Size = new System.Drawing.Size(250, 100);
            this.pnlTotalBooks.TabIndex = 1;
            // 
            // lblTotalBooks
            // 
            this.lblTotalBooks.AutoSize = true;
            this.lblTotalBooks.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.lblTotalBooks.Location = new System.Drawing.Point(10, 35);
            this.lblTotalBooks.Name = "lblTotalBooks";
            this.lblTotalBooks.Size = new System.Drawing.Size(215, 29);
            this.lblTotalBooks.TabIndex = 0;
            this.lblTotalBooks.Text = "📚 Total Books: 0";
            this.lblTotalBooks.Click += new System.EventHandler(this.lblTotalBooks_Click);
            // 
            // pnlBorrowedBooks
            // 
            this.pnlBorrowedBooks.BackColor = System.Drawing.Color.LightCoral;
            this.pnlBorrowedBooks.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBorrowedBooks.Controls.Add(this.lblBorrowedBooks);
            this.pnlBorrowedBooks.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pnlBorrowedBooks.Location = new System.Drawing.Point(575, 195);
            this.pnlBorrowedBooks.Name = "pnlBorrowedBooks";
            this.pnlBorrowedBooks.Size = new System.Drawing.Size(250, 100);
            this.pnlBorrowedBooks.TabIndex = 2;
            this.pnlBorrowedBooks.Click += new System.EventHandler(this.pnlBorrowedBooks_Click);
            // 
            // lblBorrowedBooks
            // 
            this.lblBorrowedBooks.AutoSize = true;
            this.lblBorrowedBooks.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.lblBorrowedBooks.Location = new System.Drawing.Point(10, 35);
            this.lblBorrowedBooks.Name = "lblBorrowedBooks";
            this.lblBorrowedBooks.Size = new System.Drawing.Size(189, 29);
            this.lblBorrowedBooks.TabIndex = 0;
            this.lblBorrowedBooks.Text = "📖 Borrowed: 0";
            this.lblBorrowedBooks.Click += new System.EventHandler(this.lblBorrowedBooks_Click);
            // 
            // pnlAvailableBooks
            // 
            this.pnlAvailableBooks.BackColor = System.Drawing.Color.LightGreen;
            this.pnlAvailableBooks.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlAvailableBooks.Controls.Add(this.lblAvailableBooks);
            this.pnlAvailableBooks.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pnlAvailableBooks.Location = new System.Drawing.Point(872, 195);
            this.pnlAvailableBooks.Name = "pnlAvailableBooks";
            this.pnlAvailableBooks.Size = new System.Drawing.Size(250, 100);
            this.pnlAvailableBooks.TabIndex = 3;
            // 
            // lblAvailableBooks
            // 
            this.lblAvailableBooks.AutoSize = true;
            this.lblAvailableBooks.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.lblAvailableBooks.Location = new System.Drawing.Point(10, 35);
            this.lblAvailableBooks.Name = "lblAvailableBooks";
            this.lblAvailableBooks.Size = new System.Drawing.Size(182, 29);
            this.lblAvailableBooks.TabIndex = 0;
            this.lblAvailableBooks.Text = "✅ Available: 0";
            this.lblAvailableBooks.Click += new System.EventHandler(this.lblAvailableBooks_Click);
            // 
            // pnlActiveMembers
            // 
            this.pnlActiveMembers.BackColor = System.Drawing.Color.Khaki;
            this.pnlActiveMembers.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlActiveMembers.Controls.Add(this.lblActiveMembers);
            this.pnlActiveMembers.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pnlActiveMembers.Location = new System.Drawing.Point(1170, 195);
            this.pnlActiveMembers.Name = "pnlActiveMembers";
            this.pnlActiveMembers.Size = new System.Drawing.Size(250, 100);
            this.pnlActiveMembers.TabIndex = 4;
            this.pnlActiveMembers.Click += new System.EventHandler(this.pnlActiveMembers_Click);
            // 
            // lblActiveMembers
            // 
            this.lblActiveMembers.AutoSize = true;
            this.lblActiveMembers.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.lblActiveMembers.Location = new System.Drawing.Point(10, 35);
            this.lblActiveMembers.Name = "lblActiveMembers";
            this.lblActiveMembers.Size = new System.Drawing.Size(261, 29);
            this.lblActiveMembers.TabIndex = 0;
            this.lblActiveMembers.Text = "👥 Active Members: 0";
            this.lblActiveMembers.Click += new System.EventHandler(this.lblActiveMembers_Click);
            // 
            // pnlOverdueBooks
            // 
            this.pnlOverdueBooks.BackColor = System.Drawing.Color.OrangeRed;
            this.pnlOverdueBooks.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlOverdueBooks.Controls.Add(this.lblOverdueBooks);
            this.pnlOverdueBooks.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pnlOverdueBooks.Location = new System.Drawing.Point(276, 379);
            this.pnlOverdueBooks.Name = "pnlOverdueBooks";
            this.pnlOverdueBooks.Size = new System.Drawing.Size(250, 100);
            this.pnlOverdueBooks.TabIndex = 5;
            this.pnlOverdueBooks.Click += new System.EventHandler(this.pnlOverdueBooks_Click);
            // 
            // lblOverdueBooks
            // 
            this.lblOverdueBooks.AutoSize = true;
            this.lblOverdueBooks.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.lblOverdueBooks.Location = new System.Drawing.Point(10, 35);
            this.lblOverdueBooks.Name = "lblOverdueBooks";
            this.lblOverdueBooks.Size = new System.Drawing.Size(175, 29);
            this.lblOverdueBooks.TabIndex = 0;
            this.lblOverdueBooks.Text = "⚠️ Overdue: 0";
            // 
            // lblGreeting
            // 
            this.lblGreeting.AutoSize = true;
            this.lblGreeting.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold);
            this.lblGreeting.Location = new System.Drawing.Point(702, 15);
            this.lblGreeting.Name = "lblGreeting";
            this.lblGreeting.Size = new System.Drawing.Size(346, 36);
            this.lblGreeting.TabIndex = 0;
            this.lblGreeting.Text = "Good morning, ADMIN!";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panel2.Controls.Add(this.lblGreeting);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1814, 62);
            this.panel2.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(206, 65);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 32);
            this.label1.TabIndex = 1;
            this.label1.Text = "Home";
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 62);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 640);
            this.panel1.TabIndex = 6;
            // 
            // DashboardControl
            // 
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.pnlTotalBooks);
            this.Controls.Add(this.pnlBorrowedBooks);
            this.Controls.Add(this.pnlAvailableBooks);
            this.Controls.Add(this.pnlActiveMembers);
            this.Controls.Add(this.pnlOverdueBooks);
            this.Name = "DashboardControl";
            this.Size = new System.Drawing.Size(1814, 702);
            this.Load += new System.EventHandler(this.DashboardControl_Load);
            this.pnlTotalBooks.ResumeLayout(false);
            this.pnlTotalBooks.PerformLayout();
            this.pnlBorrowedBooks.ResumeLayout(false);
            this.pnlBorrowedBooks.PerformLayout();
            this.pnlAvailableBooks.ResumeLayout(false);
            this.pnlAvailableBooks.PerformLayout();
            this.pnlActiveMembers.ResumeLayout(false);
            this.pnlActiveMembers.PerformLayout();
            this.pnlOverdueBooks.ResumeLayout(false);
            this.pnlOverdueBooks.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private Label lblGreeting;
        private Panel panel2;
        private Label label1;
        private Panel panel1;
    }
}
