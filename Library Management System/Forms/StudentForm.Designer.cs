using System.Windows.Forms;

namespace Library_Management_System.Forms
{
    partial class StudentForm
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

        private void InitializeComponent()
        {
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblYearLevel = new System.Windows.Forms.Label();
            this.lblCourse = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.lblStudentNo = new System.Windows.Forms.Label();
            this.panelTop = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnRecords = new System.Windows.Forms.Button();
            this.btnBorrowing = new System.Windows.Forms.Button();
            this.btnAvailableCopies = new System.Windows.Forms.Button();
            this.btnLogout = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(933, 115);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 23);
            this.label4.TabIndex = 7;
            this.label4.Text = "Student No:";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(733, 115);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 23);
            this.label3.TabIndex = 6;
            this.label3.Text = "Year Level:";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(533, 115);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 23);
            this.label2.TabIndex = 5;
            this.label2.Text = "Course:";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(303, 115);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 23);
            this.label1.TabIndex = 4;
            this.label1.Text = "Name:";
            // 
            // lblYearLevel
            // 
            this.lblYearLevel.Location = new System.Drawing.Point(813, 115);
            this.lblYearLevel.Name = "lblYearLevel";
            this.lblYearLevel.Size = new System.Drawing.Size(100, 23);
            this.lblYearLevel.TabIndex = 3;
            // 
            // lblCourse
            // 
            this.lblCourse.Location = new System.Drawing.Point(593, 115);
            this.lblCourse.Name = "lblCourse";
            this.lblCourse.Size = new System.Drawing.Size(100, 23);
            this.lblCourse.TabIndex = 2;
            // 
            // lblName
            // 
            this.lblName.Location = new System.Drawing.Point(363, 115);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(100, 23);
            this.lblName.TabIndex = 1;
            // 
            // lblStudentNo
            // 
            this.lblStudentNo.Location = new System.Drawing.Point(1023, 115);
            this.lblStudentNo.Name = "lblStudentNo";
            this.lblStudentNo.Size = new System.Drawing.Size(100, 23);
            this.lblStudentNo.TabIndex = 0;
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.White;
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1313, 80);
            this.panelTop.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.btnRecords);
            this.panel1.Controls.Add(this.btnBorrowing);
            this.panel1.Controls.Add(this.btnAvailableCopies);
            this.panel1.Controls.Add(this.btnLogout);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 80);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(275, 798);
            this.panel1.TabIndex = 11;
            // 
            // btnRecords
            // 
            this.btnRecords.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRecords.Location = new System.Drawing.Point(31, 300);
            this.btnRecords.Name = "btnRecords";
            this.btnRecords.Size = new System.Drawing.Size(182, 50);
            this.btnRecords.TabIndex = 11;
            this.btnRecords.Text = "Records";
            this.btnRecords.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnBorrowing
            // 
            this.btnBorrowing.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBorrowing.Location = new System.Drawing.Point(31, 226);
            this.btnBorrowing.Name = "btnBorrowing";
            this.btnBorrowing.Size = new System.Drawing.Size(182, 50);
            this.btnBorrowing.TabIndex = 10;
            this.btnBorrowing.Text = "Borrowing";
            this.btnBorrowing.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnAvailableCopies
            // 
            this.btnAvailableCopies.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAvailableCopies.Location = new System.Drawing.Point(31, 148);
            this.btnAvailableCopies.Name = "btnAvailableCopies";
            this.btnAvailableCopies.Size = new System.Drawing.Size(182, 50);
            this.btnAvailableCopies.TabIndex = 9;
            this.btnAvailableCopies.Text = "Available Copies";
            this.btnAvailableCopies.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAvailableCopies.Click += new System.EventHandler(this.btnAvailableCopies_Click_1);
            // 
            // btnLogout
            // 
            this.btnLogout.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLogout.Location = new System.Drawing.Point(31, 614);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(182, 50);
            this.btnLogout.TabIndex = 8;
            this.btnLogout.Text = "Logout";
            this.btnLogout.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // StudentForm
            // 
            this.ClientSize = new System.Drawing.Size(1313, 878);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lblStudentNo);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.lblCourse);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblYearLevel);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Name = "StudentForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Student Area";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private Label label4;
        private Label label3;
        private Label label2;
        private Label label1;
        private Label lblYearLevel;
        private Label lblCourse;
        private Label lblName;
        private Label lblStudentNo;
        private Panel panelTop;
        private Panel panel1;
        private Button btnRecords;
        private Button btnBorrowing;
        private Button btnAvailableCopies;
        private Button btnLogout;
    }
}

