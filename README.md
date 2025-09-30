# Learning Management System (LMS)

A full-stack web application built with ASP.NET Core that provides a comprehensive learning management platform for students, professors, and administrators.

## Features

### Student Features
- View enrolled classes and grades
- Submit assignments
- Track assignment scores
- Calculate GPA automatically

### Professor Features
- Manage classes and view enrolled students
- Create assignment categories with weighted grading
- Create and grade assignments
- Automatic grade calculation based on category weights

### Administrator Features
- Create departments and courses
- Schedule class offerings
- Manage professors and course assignments
- Prevent scheduling conflicts

## Tech Stack

- **Backend**: ASP.NET Core 9.0 (MVC)
- **Database**: MySQL/MariaDB with Entity Framework Core 6.0
- **Authentication**: ASP.NET Core Identity with role-based authorization
- **Frontend**: Razor Pages with Bootstrap
- **ORM**: Entity Framework Core (Code-First approach)

## Database Architecture

The application uses a dual-context architecture:
- **Identity Context**: Manages user authentication and authorization
- **LMS Context**: Handles core academic data (courses, classes, assignments, grades)

Key entities: Department, Course, Class, Assignment, AssignmentCategory, Submission, Enroll, Student, Professor, Administrator

## Setup Instructions

### Prerequisites
- .NET 9.0 SDK
- MySQL or MariaDB server
- Git

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/code-sharingan/LMS.git
   cd LMS
   ```

2. **Set up the database**
   ```bash
   # Start MySQL/MariaDB and create databases
   mysql -u root -p
   CREATE DATABASE lms_identity;
   CREATE DATABASE lms_main;
   exit
   ```

3. **Configure connection strings**
   ```bash
   cd LMS
   dotnet user-secrets set "LMS:IdentityConnectionString" "server=localhost;database=lms_identity;user=root;password=YOUR_PASSWORD"
   dotnet user-secrets set "LMS:LMSConnectionString" "server=localhost;database=lms_main;user=root;password=YOUR_PASSWORD"
   ```

4. **Run database migrations**
   ```bash
   dotnet ef database update --context LMSIdentityDbContext
   dotnet ef database update --context LMSContext
   ```

5. **Run the application**
   ```bash
   dotnet run --project LMS/LMS.csproj
   ```

6. **Access the application**
   - Navigate to `https://localhost:5001` (or the port shown in terminal)
   - Register a new account and assign roles as needed

## Project Structure

```
LMS/
├── Controllers/          # MVC Controllers (Student, Professor, Administrator)
├── Models/
│   └── LMSModels/       # Entity Framework models
├── Views/               # Razor views for each role
├── Areas/Identity/      # ASP.NET Identity scaffolded pages
├── Migrations/          # EF Core migrations
└── wwwroot/            # Static files (CSS, JS, libraries)

LMSControllerTests/      # xUnit test project
```

## Key Features Implementation

### Automatic Grade Calculation
- Grades are recalculated automatically when:
  - New assignments are created
  - Assignments are graded
- Uses weighted average based on assignment categories
- Converts percentage scores to letter grades (A-E scale)

### Transaction Management
- All data modifications use explicit transactions
- Ensures data integrity across related entities
- Proper rollback on constraint violations

### Security
- Role-based authorization (Student, Professor, Administrator)
- Password hashing via ASP.NET Identity
- Connection strings stored in user secrets (not in source control)

## Development Commands

```bash
# Build the project
dotnet build

# Run tests
dotnet test

# Add a new migration
dotnet ef migrations add MigrationName --project LMS/LMS.csproj

# Update database
dotnet ef database update --project LMS/LMS.csproj
```

## Future Enhancements

- [ ] Add email notifications for assignment deadlines
- [ ] Implement file upload for assignment submissions
- [ ] Add discussion forums for classes
- [ ] Create mobile-responsive UI improvements
- [ ] Add unit test coverage
- [ ] Implement Docker containerization

## License

This project is open source and available under the [MIT License](LICENSE).

## Author

**Your Name**
[Shubham Singh](https://github.com/code-sharingan) | [LinkedIn](https://linkedin.com/in/shubhamanilsingh)
