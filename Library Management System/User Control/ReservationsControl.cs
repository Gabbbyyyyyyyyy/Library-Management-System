using System;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using Library_Management_System.Models;
using LibraryManagementSystem.Data;

namespace Library_Management_System.User_Control
{

    public partial class ReservationsControl : UserControl
    {
        private int targetWidth = 155; // final width when fully shown
        private Timer slideTimer;
        // ADD THIS LINE:
        private Panel noResultPanel;

        public ReservationsControl()
        {
            InitializeComponent();
            dgvReservations.CellContentClick += DgvReservations_CellContentClick;

            // Your existing initialization code...
            InitializeNoResultPanel();
            btnReturn.ApplyRoundedCorners(10, Color.Black, 2); // radius 12
            // Initially collapsed
            panel1.Width = 0;

            // Setup Timer
            slideTimer = new Timer();
            slideTimer.Interval = 10;
            slideTimer.Tick += SlideTimer_Tick;
        }

        private void LoadReservations()
        {
            try
            {
                using (var con = Db.GetConnection())
                {
                    con.Open();

                    string query = @"
                        SELECT 
                            r.ReservationId,
                            b.Title,
                            m.StudentNo,
                            m.FirstName || ' ' || m.LastName AS ReservedBy,
                            r.ReserveDate,
                            r.Status
                        FROM Reservations r
                        INNER JOIN Books b ON b.BookId = r.BookId
                        INNER JOIN Members m ON m.MemberId = r.MemberId
                        ORDER BY r.ReserveDate DESC";

                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, con))
                    using (var adapter = new System.Data.SQLite.SQLiteDataAdapter(cmd))
                    {
                        System.Data.DataTable dt = new System.Data.DataTable();
                        adapter.Fill(dt);

                        dgvReservations.DataSource = dt;


                        // Optional: adjust headers again
                        dgvReservations.Columns["StudentNo"].HeaderText = "Student Number";
                        dgvReservations.Columns["ReservedBy"].HeaderText = "Reserved By";
                        dgvReservations.Columns["ReserveDate"].HeaderText = "Reserve Date";

                        // Remove old button column if exists
                        if (dgvReservations.Columns.Contains("Action"))
                            dgvReservations.Columns.Remove("Action");

                        // Add "Remove" button column
                        DataGridViewButtonColumn removeCol = new DataGridViewButtonColumn
                        {
                            Name = "Action",
                            HeaderText = "Action",
                            Text = "Remove",
                            UseColumnTextForButtonValue = true,
                            Width = 70
                        };
                        dgvReservations.Columns.Add(removeCol);


                        // Hide internal IDs
                        if (dgvReservations.Columns.Contains("ReservationId"))
                            dgvReservations.Columns["ReservationId"].Visible = false;

                        // Apply helper styles
                        DataGridViewHelper.ApplyDefaultStyle(dgvReservations);

                        // Adjust row height
                        dgvReservations.RowTemplate.Height = 30;

                        // Show/Hide "No Results" panel
                        if (dt.Rows.Count == 0)
                        {
                            ShowNoReservations();
                        }
                        else
                        {
                            dgvReservations.Visible = true;
                            noResultPanel.Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading reservations: " + ex.Message);
            }
        }

        private void DgvReservations_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // header row

            if (dgvReservations.Columns[e.ColumnIndex].Name == "Action")
            {
                string reservationId = dgvReservations.Rows[e.RowIndex].Cells["ReservationId"].Value.ToString();
                string studentNo = dgvReservations.Rows[e.RowIndex].Cells["StudentNo"].Value.ToString();
                string bookTitle = dgvReservations.Rows[e.RowIndex].Cells["Title"].Value.ToString();
                string status = dgvReservations.Rows[e.RowIndex].Cells["Status"].Value.ToString();

                // ❌ Validation: cannot remove fulfilled reservation
                if (status == "Fulfilled")
                {
                    MessageBox.Show($"You cannot remove or reject this reservation because it has already been accepted.",
                                    "Action Denied",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                    return;
                }

                // Confirm deletion
                var result = MessageBox.Show(
                    $"Are you sure you want to reject/remove the reservation for '{bookTitle}'?",
                    "Confirm Remove",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Yes)
                {
                    using (var con = Db.GetConnection())
                    {
                        con.Open();

                        // Update status to Rejected
                        string updateQuery = "UPDATE Reservations SET Status='Rejected' WHERE ReservationId=@id";
                        using (var cmd = new SQLiteCommand(updateQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@id", reservationId);
                            cmd.ExecuteNonQuery();
                        }

                        // Insert notification
                        string notifQuery = @"
                INSERT INTO Notifications (StudentNo, Message, DateCreated, IsRead)
                VALUES (@studentNo, @msg, @date, 0)";
                        using (var cmd = new SQLiteCommand(notifQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@studentNo", studentNo);
                            cmd.Parameters.AddWithValue("@msg", $"Your reservation for '{bookTitle}' has been rejected by the admin.");
                            cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show($"Reservation for '{bookTitle}' has been rejected.", "Removed", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadReservations(); // reload
                }
            }
        }


        private void ReservationsControl_Load(object sender, EventArgs e)
        {
            LoadReservations();
        }

        private void dgvBorrowedBooks_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            if (dgvReservations.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a reservation first.");
                return;
            }

            string reservationId = dgvReservations.SelectedRows[0].Cells["ReservationId"].Value.ToString();
            string studentNo = dgvReservations.SelectedRows[0].Cells["StudentNo"].Value.ToString();
            string bookTitle = dgvReservations.SelectedRows[0].Cells["Title"].Value.ToString();

            using (var con = Db.GetConnection())
            {
                con.Open();

                // 1️⃣ Check if the book is currently borrowed
                string borrowQuery = @"
            SELECT ReturnDate, DueDate 
            FROM Borrowings 
            WHERE BookId = (SELECT BookId FROM Reservations WHERE ReservationId=@resId)
              AND Status='Borrowed'
            ORDER BY DueDate DESC
            LIMIT 1";

                DateTime availableDate = DateTime.Now; // default: available now
                string message;

                using (var cmd = new SQLiteCommand(borrowQuery, con))
                {
                    cmd.Parameters.AddWithValue("@resId", reservationId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Book is borrowed
                            DateTime dueDate = reader.GetDateTime(reader.GetOrdinal("DueDate"));

                            if (DateTime.Now < dueDate)
                            {
                                // Book will be returned in the future
                                availableDate = dueDate;
                                message = $" Your reservation for '{bookTitle}' has been approved. You can pick it up on {availableDate:MMMM dd, yyyy HH:mm tt}.";
                            }
                            else
                            {
                                // Borrowed book overdue → can pick up now
                                message = $" Your reservation for '{bookTitle}' is ready. You can pick it up now.";
                            }
                        }
                        else
                        {
                            // Book not borrowed, available immediately
                            message = $" Your reservation for '{bookTitle}' has been approved. You can pick it up now.";
                        }
                    }
                }

                // 2️⃣ Update Reservation Status → Fulfilled
                string updateQuery = "UPDATE Reservations SET Status='Fulfilled' WHERE ReservationId=@id";
                using (var cmd = new SQLiteCommand(updateQuery, con))
                {
                    cmd.Parameters.AddWithValue("@id", reservationId);
                    cmd.ExecuteNonQuery();
                }

                // 3️⃣ Insert notification
                string notifQuery = @"
            INSERT INTO Notifications (StudentNo, Message, DateCreated, IsRead)
            VALUES (@studentNo, @msg, @date, 0)";
                using (var cmd = new SQLiteCommand(notifQuery, con))
                {
                    cmd.Parameters.AddWithValue("@studentNo", studentNo);
                    cmd.Parameters.AddWithValue("@msg", message);
                    cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Reservation processed and student notified!");
            LoadReservations(); // reload table
        }



        private void SlideTimer_Tick(object sender, EventArgs e)
        {
            if (panel2.Width < targetWidth)
            {
                panel2.Width += 8; // adjust speed
                panel2.Invalidate(); // redraw rounded corners
            }
            else
            {
                slideTimer.Stop(); // stop when fully expanded
            }
        }

        // Call this from MainForm
        public void RollOutPanel()
        {
            // Reset panel to collapsed before rolling out
            if (panel2.Width != 0)
            {
                panel2.Width = 0;
                panel2.Invalidate();
            }

            slideTimer.Start();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            int radius = 20;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            GraphicsPath path = new GraphicsPath();

            // Only round top-right and bottom-right corners
            path.AddLine(0, 0, panel2.Width - radius, 0);                    // top edge
            path.AddArc(panel2.Width - radius, 0, radius, radius, 270, 90); // top-right
            path.AddLine(panel2.Width, radius, panel2.Width, panel2.Height - radius); // right edge
            path.AddArc(panel2.Width - radius, panel2.Height - radius, radius, radius, 0, 90); // bottom-right
            path.AddLine(panel2.Width - radius, panel2.Height, 0, panel2.Height);             // bottom edge
            path.AddLine(0, panel2.Height, 0, 0);                                           // left edge
            path.CloseFigure();

            panel2.Region = new Region(path);
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }
        // Inside your ReservationsControl class
        private void InitializeNoResultPanel()
        {
            // Create hidden no-result panel
            noResultPanel = new Panel
            {
                Size = dgvReservations.Size,
                Location = dgvReservations.Location,
                BackColor = Color.Transparent,
                Visible = false
            };
            this.Controls.Add(noResultPanel);

            // Add the picture/icon
            PictureBox pic = new PictureBox
            {
                Image = Properties.Resources.NoBooksIcon, // <-- Your icon here
                Size = new Size(120, 120),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            noResultPanel.Controls.Add(pic);

            // Main title
            Label lblMain = new Label
            {
                Text = "No reservations found",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.Gray,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            noResultPanel.Controls.Add(lblMain);

            // Subtitle
            Label lblSub = new Label
            {
                Text = "Try searching again or wait for a reservation.",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.DimGray,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            noResultPanel.Controls.Add(lblSub);

            // Save controls for layout adjustment
            noResultPanel.Tag = new Control[] { pic, lblMain, lblSub };
        }

        private void RefreshNoResultLayout(string mainText, string subText)
        {
            var controls = (Control[])noResultPanel.Tag;
            PictureBox pic = (PictureBox)controls[0];
            Label lblMain = (Label)controls[1];
            Label lblSub = (Label)controls[2];

            lblMain.Text = mainText;
            lblSub.Text = subText;

            // Center icon
            pic.Location = new Point(
                (noResultPanel.Width - pic.Width) / 2,
                20
            );

            // Center main text
            lblMain.Location = new Point(
                (noResultPanel.Width - lblMain.Width) / 2,
                pic.Bottom + 15
            );

            // Center subtitle
            lblSub.Location = new Point(
                (noResultPanel.Width - lblSub.Width) / 2,
                lblMain.Bottom + 5
            );
        }

        // Example usage when no data
        private void ShowNoReservations()
        {
            dgvReservations.Visible = false;
            noResultPanel.Visible = true;
            RefreshNoResultLayout("No reservations yet", "Reserved books will appear here.");
        }

    }
}
