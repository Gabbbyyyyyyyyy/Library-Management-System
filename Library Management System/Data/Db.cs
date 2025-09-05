using System;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;

namespace LibraryManagementSystem.Data
{
    public static class Db
    {
        public static string DbPath => Path.Combine(Application.StartupPath, "library.db");
        public static string ConnectionString => $"Data Source={DbPath};Version=3;";

        // Create DB and tables if not exist
        public static void EnsureCreated()
        {
            if (!File.Exists(DbPath))
            {
                SQLiteConnection.CreateFile(DbPath);
            }

            using (var con = new SQLiteConnection(ConnectionString))
            {
                con.Open();
                string sql = @"
                CREATE TABLE IF NOT EXISTS Members (
                    MemberId   INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentNo  TEXT UNIQUE,
                    FullName   TEXT NOT NULL,
                    Course     TEXT,
                    YearLevel  TEXT
                );

                CREATE TABLE IF NOT EXISTS Books (
                    BookId     INTEGER PRIMARY KEY AUTOINCREMENT,
                    ISBN       TEXT,
                    Title      TEXT NOT NULL,
                    Author     TEXT,
                    Category   TEXT,
                    IsBorrowed INTEGER NOT NULL DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS BorrowedBooks (
                    BorrowId   INTEGER PRIMARY KEY AUTOINCREMENT,
                    MemberId   INTEGER NOT NULL,
                    BookId     INTEGER NOT NULL,
                    BorrowDate TEXT NOT NULL,
                    DueDate    TEXT NOT NULL,
                    ReturnDate TEXT,
                    Penalty    REAL DEFAULT 0,
                    FOREIGN KEY(MemberId) REFERENCES Members(MemberId),
                    FOREIGN KEY(BookId) REFERENCES Books(BookId)
                );
                ";
                using (var cmd = new SQLiteCommand(sql, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(ConnectionString);
        }
    }
}
