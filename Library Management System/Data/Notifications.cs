using System;
using System.Data.SQLite;
using LibraryManagementSystem.Data;

namespace Library_Management_System.Data
{
    internal static class Notifications
    {
        public static void EnsureCreated(SQLiteConnection con)
        {
            string query = @"
            CREATE TABLE IF NOT EXISTS Notifications (
                NotificationId INTEGER PRIMARY KEY AUTOINCREMENT,
                StudentNo TEXT NOT NULL,
                Message TEXT NOT NULL,
                DateCreated TEXT NOT NULL,
                IsRead INTEGER DEFAULT 0,
                IsFollowUp INTEGER DEFAULT 0
            );
        ";

            using (var cmd = new SQLiteCommand(query, con))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public static void AddNotification(string studentNo, string message)
        {
            using (var con = Db.GetConnection())
            {
                con.Open();

                string query = @"
                INSERT INTO Notifications (StudentNo, Message, DateCreated, IsRead, IsFollowUp)
                VALUES (@stud, @msg, @date, 0, 0);
            ";

                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@stud", studentNo);
                    cmd.Parameters.AddWithValue("@msg", message);
                    cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Send initial reservation notification
        public static void NotifyReservation(string studentNo, string bookTitle, DateTime expectedPickup)
        {
            string message = $"Your reservation for '{bookTitle}' has been approved. You can pick it up on {expectedPickup:MMMM dd, yyyy HH:mm tt}.";

            using (var con = Db.GetConnection())
            {
                con.Open();

                string query = @"
                    INSERT INTO Notifications (StudentNo, Message, DateCreated, IsRead, IsFollowUp)
                    VALUES (@studentNo, @msg, @date, 0, 0)";
                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@studentNo", studentNo);
                    cmd.Parameters.AddWithValue("@msg", message);
                    cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Send follow-up notifications if book not returned
        public static void SendFollowUpNotifications()
        {
            using (var con = Db.GetConnection())
            {
                con.Open();

                string query = @"
                    SELECT r.ReservationId, r.StudentNo, b.Title, br.DueDate
                    FROM Reservations r
                    INNER JOIN Books b ON r.BookId = b.BookId
                    INNER JOIN Borrowings br ON b.BookId = br.BookId
                    WHERE r.Status='Fulfilled'
                      AND br.Status='Borrowed'
                      AND r.ReservationId NOT IN (
                          SELECT ReservationId FROM Notifications WHERE IsFollowUp = 1
                      )
                ";

                using (var cmd = new SQLiteCommand(query, con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string studentNo = reader.GetString(reader.GetOrdinal("StudentNo"));
                        string bookTitle = reader.GetString(reader.GetOrdinal("Title"));
                        DateTime dueDate = reader.GetDateTime(reader.GetOrdinal("DueDate"));

                        if (DateTime.Now >= dueDate) // if due date passed and book still not returned
                        {
                            string message = $"Sorry, the student has not returned '{bookTitle}' yet. Expected availability: {dueDate:MMMM dd, yyyy HH:mm tt}.";

                            using (var cmd2 = new SQLiteCommand(@"
                                INSERT INTO Notifications (StudentNo, Message, DateCreated, IsRead, IsFollowUp)
                                VALUES (@studentNo, @msg, @date, 0, 1)", con))
                            {
                                cmd2.Parameters.AddWithValue("@studentNo", studentNo);
                                cmd2.Parameters.AddWithValue("@msg", message);
                                cmd2.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                cmd2.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }
    }
}
