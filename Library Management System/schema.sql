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
