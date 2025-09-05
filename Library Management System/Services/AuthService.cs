using System;
using System.Data.SQLite;
using LibraryManagementSystem.Data;

namespace Library_Management_System.Services
{
    internal class AuthService
    {
        public bool Authenticate(string studentNo)
        {
            using (var con = Db.GetConnection())
            {
                con.Open();

                string query = "SELECT COUNT(1) FROM Members WHERE StudentNo=@studentNo";
                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@studentNo", studentNo);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count == 1;
                }
            }
        }
    }
}
