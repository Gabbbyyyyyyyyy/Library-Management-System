# Library Management System
A C# Windows Forms application for managing a library, using **SQLite** as the database.

## Getting Started
### Prerequisites
- Windows OS
- Visual Studio
- .NET Framework
- SQLite (CLI tool) installed on your system
- 
## Setup Instructions
1. Clone the repository:
   ```bash
   git clone https://github.com/Gabbbyyyyyyyyy/Library-Management-System.git
   cd LibraryManagementSystem
   
2. Open the solution in **Visual Studio**.

3. Restore NuGet packages if required.

4. Build and run the project.

## Database Setup
The database file `library.db` is ignored in this repository.
Instead, you can rebuild it anytime from the schema.

### Rebuild the DB anywhere
On a new machine, after pulling the repo, just run:

```bash
sqlite3 library.db < schema.sql
```

That will generate the `library.db` with all the tables defined in your schema.

## Notes
* `library.db` is not tracked in Git to avoid conflicts.
* Use `schema.sql` to regenerate the database when needed.
