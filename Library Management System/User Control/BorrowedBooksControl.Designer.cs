namespace Library_Management_System.User_Control
{
    partial class BorrowBooksControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtMemberID;
        private System.Windows.Forms.Button btnLoadMember;
        private System.Windows.Forms.Label lblMemberName;
        private System.Windows.Forms.DataGridView dgvAvailableBooks;
        private System.Windows.Forms.Button btnIssue;
        private System.Windows.Forms.Label lblDueDate;
        private System.Windows.Forms.Label lblMessage;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
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
            this.txtMemberID = new System.Windows.Forms.TextBox();
            this.btnLoadMember = new System.Windows.Forms.Button();
            this.lblMemberName = new System.Windows.Forms.Label();
            this.dgvAvailableBooks = new System.Windows.Forms.DataGridView();
            this.btnIssue = new System.Windows.Forms.Button();
            this.lblDueDate = new System.Windows.Forms.Label();
            this.lblMessage = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAvailableBooks)).BeginInit();
            this.SuspendLayout();
            // 
            // txtMemberID
            // 
            this.txtMemberID.Location = new System.Drawing.Point(328, 242);
            this.txtMemberID.Margin = new System.Windows.Forms.Padding(4);
            this.txtMemberID.Name = "txtMemberID";
            this.txtMemberID.Size = new System.Drawing.Size(199, 22);
            this.txtMemberID.TabIndex = 0;
            this.txtMemberID.TextChanged += new System.EventHandler(this.txtMemberID_TextChanged);
            // 
            // btnLoadMember
            // 
            this.btnLoadMember.Location = new System.Drawing.Point(541, 239);
            this.btnLoadMember.Margin = new System.Windows.Forms.Padding(4);
            this.btnLoadMember.Name = "btnLoadMember";
            this.btnLoadMember.Size = new System.Drawing.Size(133, 31);
            this.btnLoadMember.TabIndex = 1;
            this.btnLoadMember.Text = "Load Member";
            this.btnLoadMember.UseVisualStyleBackColor = true;
            this.btnLoadMember.Click += new System.EventHandler(this.BtnLoadMember_Click);
            // 
            // lblMemberName
            // 
            this.lblMemberName.AutoSize = true;
            this.lblMemberName.Location = new System.Drawing.Point(328, 285);
            this.lblMemberName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMemberName.Name = "lblMemberName";
            this.lblMemberName.Size = new System.Drawing.Size(101, 16);
            this.lblMemberName.TabIndex = 2;
            this.lblMemberName.Text = "Member: (none)";
            // 
            // dgvAvailableBooks
            // 
            this.dgvAvailableBooks.AllowUserToAddRows = false;
            this.dgvAvailableBooks.AllowUserToDeleteRows = false;
            this.dgvAvailableBooks.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvAvailableBooks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAvailableBooks.Location = new System.Drawing.Point(277, 350);
            this.dgvAvailableBooks.Margin = new System.Windows.Forms.Padding(4);
            this.dgvAvailableBooks.MultiSelect = false;
            this.dgvAvailableBooks.Name = "dgvAvailableBooks";
            this.dgvAvailableBooks.ReadOnly = true;
            this.dgvAvailableBooks.RowHeadersWidth = 51;
            this.dgvAvailableBooks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAvailableBooks.Size = new System.Drawing.Size(1728, 782);
            this.dgvAvailableBooks.TabIndex = 3;
            this.dgvAvailableBooks.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvAvailableBooks_CellContentClick);
            // 
            // btnIssue
            // 
            this.btnIssue.Location = new System.Drawing.Point(754, 233);
            this.btnIssue.Margin = new System.Windows.Forms.Padding(4);
            this.btnIssue.Name = "btnIssue";
            this.btnIssue.Size = new System.Drawing.Size(133, 37);
            this.btnIssue.TabIndex = 4;
            this.btnIssue.Text = "Issue Book";
            this.btnIssue.UseVisualStyleBackColor = true;
            this.btnIssue.Click += new System.EventHandler(this.BtnIssue_Click);
            // 
            // lblDueDate
            // 
            this.lblDueDate.AutoSize = true;
            this.lblDueDate.Location = new System.Drawing.Point(914, 243);
            this.lblDueDate.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDueDate.Name = "lblDueDate";
            this.lblDueDate.Size = new System.Drawing.Size(67, 16);
            this.lblDueDate.TabIndex = 5;
            this.lblDueDate.Text = "Due Date:";
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.ForeColor = System.Drawing.Color.Red;
            this.lblMessage.Location = new System.Drawing.Point(754, 282);
            this.lblMessage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(71, 16);
            this.lblMessage.TabIndex = 6;
            this.lblMessage.Text = "Messages";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(281, 77);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(103, 32);
            this.label6.TabIndex = 7;
            this.label6.Text = "Borrow";
            this.label6.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(1763, 321);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(242, 22);
            this.txtSearch.TabIndex = 0;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // BorrowBooksControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.lblDueDate);
            this.Controls.Add(this.btnIssue);
            this.Controls.Add(this.dgvAvailableBooks);
            this.Controls.Add(this.lblMemberName);
            this.Controls.Add(this.btnLoadMember);
            this.Controls.Add(this.txtMemberID);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "BorrowBooksControl";
            this.Size = new System.Drawing.Size(1855, 947);
            this.Load += new System.EventHandler(this.BorrowBooksControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAvailableBooks)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtSearch;
    }
}
