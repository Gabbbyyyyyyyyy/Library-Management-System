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

        // Add this field
        private Panel panelContent;

        private void InitializeComponent()
        {
            this.button5 = new System.Windows.Forms.Button();
            this.lblCourse = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.lblYearLevel = new System.Windows.Forms.Label();
            this.btnAvailableCopies = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.lblStudentNo = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnBorrowing = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panelContent = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button5
            // 
            this.button5.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button5.FlatAppearance.BorderSize = 0;
            this.button5.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightGray;
            this.button5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button5.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button5.Location = new System.Drawing.Point(12, 268);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(148, 48);
            this.button5.TabIndex = 6;
            this.button5.Text = "Logout";
            this.button5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // lblCourse
            // 
            this.lblCourse.Location = new System.Drawing.Point(593, 115);
            this.lblCourse.Name = "lblCourse";
            this.lblCourse.Size = new System.Drawing.Size(100, 23);
            this.lblCourse.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(303, 115);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 23);
            this.label1.TabIndex = 4;
            this.label1.Text = "Name:";
            // 
            // lblName
            // 
            this.lblName.Location = new System.Drawing.Point(363, 115);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(100, 23);
            this.lblName.TabIndex = 1;
            // 
            // lblYearLevel
            // 
            this.lblYearLevel.Location = new System.Drawing.Point(813, 115);
            this.lblYearLevel.Name = "lblYearLevel";
            this.lblYearLevel.Size = new System.Drawing.Size(100, 23);
            this.lblYearLevel.TabIndex = 3;
            // 
            // btnAvailableCopies
            // 
            this.btnAvailableCopies.BackColor = System.Drawing.Color.White;
            this.btnAvailableCopies.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAvailableCopies.FlatAppearance.BorderSize = 0;
            this.btnAvailableCopies.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightGray;
            this.btnAvailableCopies.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAvailableCopies.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAvailableCopies.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAvailableCopies.Location = new System.Drawing.Point(12, 160);
            this.btnAvailableCopies.Name = "btnAvailableCopies";
            this.btnAvailableCopies.Size = new System.Drawing.Size(177, 48);
            this.btnAvailableCopies.TabIndex = 5;
            this.btnAvailableCopies.Text = "Available Copies";
            this.btnAvailableCopies.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAvailableCopies.UseVisualStyleBackColor = false;
            this.btnAvailableCopies.Click += new System.EventHandler(this.btnAvailableCopies_Click);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(933, 115);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 23);
            this.label4.TabIndex = 7;
            this.label4.Text = "Student No:";
            // 
            // lblStudentNo
            // 
            this.lblStudentNo.Location = new System.Drawing.Point(1023, 115);
            this.lblStudentNo.Name = "lblStudentNo";
            this.lblStudentNo.Size = new System.Drawing.Size(100, 23);
            this.lblStudentNo.TabIndex = 0;
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
            // btnBorrowing
            // 
            this.btnBorrowing.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBorrowing.FlatAppearance.BorderSize = 0;
            this.btnBorrowing.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightGray;
            this.btnBorrowing.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBorrowing.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBorrowing.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnBorrowing.Location = new System.Drawing.Point(12, 214);
            this.btnBorrowing.Name = "btnBorrowing";
            this.btnBorrowing.Size = new System.Drawing.Size(177, 48);
            this.btnBorrowing.TabIndex = 0;
            this.btnBorrowing.Text = "Records";
            this.btnBorrowing.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnBorrowing.UseVisualStyleBackColor = true;
            this.btnBorrowing.Click += new System.EventHandler(this.btnBorrowing_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox1.Image = global::Library_Management_System.Properties.Resources.logo2;
            this.pictureBox1.Location = new System.Drawing.Point(-10, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(170, 101);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.btnBorrowing);
            this.panel1.Controls.Add(this.btnAvailableCopies);
            this.panel1.Controls.Add(this.button5);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(215, 878);
            this.panel1.TabIndex = 12;
            // 
            // panelContent
            // 
            this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContent.Location = new System.Drawing.Point(215, 0);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(1235, 878);
            this.panelContent.TabIndex = 13;
            // 
            // StudentForm
            // 
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1450, 878);
            this.Controls.Add(this.panelContent);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lblStudentNo);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.lblCourse);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblYearLevel);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Name = "StudentForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Student Area";
            this.Load += new System.EventHandler(this.StudentForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private Button button5;
        private Label lblCourse;
        private Label label1;
        private Label lblName;
        private Label lblYearLevel;
        private Button btnAvailableCopies;
        private Label label4;
        private Label lblStudentNo;
        private Label label3;
        private Label label2;
        private Button btnBorrowing;
        private PictureBox pictureBox1;
        private Panel panel1;
    }
}

