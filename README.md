# Postgraduate Research (PGR) Student Management System

A modular ASP.NET Core web application for managing Postgraduate Research (PGR) students, supervisory teams, and research activities.

---

## Architecture Overview

The application is structured as a **modular monolith** with three primary functional modules:

1. **Students Module** ([PgrStudentManagement.Web/Modules/Students](PgrStudentManagement.Web/Modules/Students))
   - Handles student records, enrolments, mode of study, and progression milestones.
   - Persisted via Entity Framework Core with **SQLite** (`student-management.db`).
   - Exposes public module contract `IStudentLookup` for cross-module integration.

2. **Academic Supervisors Module** ([PgrStudentManagement.Web/Modules/AcademicSupervisors](PgrStudentManagement.Web/Modules/AcademicSupervisors))
   - Manages supervisor records, role eligibility, and supervisory team assignments (Main, Co-Supervisor, Advisor).
   - Enforces supervisory team constraints (e.g., eligible roles, maximum supervisor thresholds, employment type rules).
   - Persisted via EF Core with **SQLite**.

3. **Research Activities Module** ([PgrStudentManagement.Web/Modules/ResearchActivities](PgrStudentManagement.Web/Modules/ResearchActivities))
   - Tracks research activities, conferences, papers, and milestones.
   - Persisted as document collections in **MongoDB** (`StudentManagement.ResearchActivities`).

---

## Database Setup & Configuration

The application uses a polyglot persistence approach: **SQLite** for relational domain models and **MongoDB** for unstructured research activity documents.

### 1. SQLite Relational Database (`student-management.db`)

- **Purpose**: Stores Students, Enrolments, Academic Supervisors, Eligible Roles, and Supervisory Assignments.
- **Connection String**: Configured in `PgrStudentManagement.Web/appsettings.json`:
  ```json
  "ConnectionStrings": {
    "StudentManagementDatabase": "Data Source=student-management.db"
  }
  ```
- **Setup & Installation**:
  - **Zero installation required**: SQLite is an embedded file-based database and does not require a background service or daemon.
  - **Automatic Migrations & Seeding**: On application startup, `Program.cs` automatically invokes `await context.Database.MigrateAsync()`. This creates `student-management.db` if it does not exist and applies all EF Core migrations, followed by executing the data seeders.
- **Manual Migrations (Optional)**:
  If you modify domain entities and want to run migrations manually via the .NET EF Core CLI tools:
  ```bash
  dotnet ef database update --project PgrStudentManagement.Web
  ```
- **Resetting SQLite Data**:
  To reset the database to a fresh state, stop the web application and delete the `student-management.db` file (located in `PgrStudentManagement.Web/` or the working directory where the process was started). It will be recreated and re-seeded on the next launch.
- **Viewing the Data**:
  You can inspect the database using the VS Code *SQLite Viewer* extension, *DB Browser for SQLite*, or the `sqlite3` CLI.

---

### 2. MongoDB Document Database (`StudentManagement`)

- **Purpose**: Stores student research activity documents.
- **Connection Settings**: Configured in `PgrStudentManagement.Web/appsettings.json`:
  ```json
  "ResearchActivityDatabase": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "StudentManagement",
    "CollectionName": "ResearchActivities"
  }
  ```
- **Setup & Installation**:
  MongoDB is an external service and must be running on `localhost:27017` before launching the application. If MongoDB is not reachable, startup will fail with a `System.TimeoutException` / `MongoConnectionException`.

  Choose one of the following setup options:

  #### Option A: Docker (Recommended)
  Run a local MongoDB container in the background:
  ```bash
  docker run -d --name mongodb -p 27017:27017 mongo:latest
  ```
  To restart the container on subsequent sessions:
  ```bash
  docker start mongodb
  ```

  #### Option B: macOS (Homebrew)
  Install and run MongoDB Community Edition as a background service:
  ```bash
  brew tap mongodb/brew
  brew install mongodb-community
  brew services start mongodb-community
  ```
  *(To check service status: `brew services list`)*

  #### Option C: MongoDB Atlas (Cloud) or Custom Host
  If using an external host or MongoDB Atlas:
  1. Obtain your connection URI (e.g. `mongodb+srv://<username>:<password>@cluster.mongodb.net/`).
  2. Update `ResearchActivityDatabase:ConnectionString` in `PgrStudentManagement.Web/appsettings.json` or override it via an environment variable:
     ```bash
     export ResearchActivityDatabase__ConnectionString="mongodb://<host>:27017"
     ```

- **Automatic Initialization**:
  You do not need to manually create the database or collection. When the web application starts, `ResearchActivityDataSeeder.SeedAsync` checks if the `ResearchActivities` collection is empty and automatically seeds sample documents.
- **Viewing the Data**:
  You can inspect the documents using *MongoDB Compass* or the `mongosh` CLI:
  ```bash
  mongosh
  use StudentManagement
  db.ResearchActivities.find().pretty()
  ```

---

## How the Application Runs

### 1. Build the Solution
From the repository root:
```bash
dotnet build
```

### 2. Run the Web Application
Ensure your MongoDB instance is running, then run the web project:

```bash
# Option A: Run from the repository root specifying the project
dotnet run --project PgrStudentManagement.Web

# Option B: Navigate into the project folder
cd PgrStudentManagement.Web
dotnet run

# Option C: Run with hot reload for development
dotnet watch --project PgrStudentManagement.Web
```

### 3. Application Startup Sequence
When [PgrStudentManagement.Web/Program.cs](PgrStudentManagement.Web/Program.cs) executes:
1. **Service Registration**: Configures EF Core (SQLite), MongoDB client, module DAOs, repositories, and business services.
2. **Database Migration**: Automatically runs `context.Database.MigrateAsync()` to apply any pending EF Core SQLite migrations.
3. **Data Seeding**: Automatically seeds initial data in order:
   - `StudentDataSeeder` (Students)
   - `EnrolmentDataSeeder` (Enrolments referencing students)
   - `AcademicSupervisorDataSeeder` (Supervisors, eligible roles, assignments)
   - `ResearchActivityDataSeeder` (Research activities in MongoDB)
4. **Endpoint Routing**: Starts the web server listening by default on:
   - HTTP: `http://localhost:5277`
   - HTTPS: `https://localhost:7178`

### 4. Running the Tests
To execute the automated unit and integration tests:
```bash
dotnet test
```

---

## Important Note: The `await` Fix in `AcademicSupervisorsController`

### Problem
When accessing the Academic Supervisors list (`/AcademicSupervisors`), the application previously threw the following runtime exception:

```text
System.InvalidOperationException: The model item passed into the ViewDataDictionary is of type 
'System.Threading.Tasks.Task`1[System.Collections.Generic.IReadOnlyList`1[PgrStudentManagement.Web.Modules.AcademicSupervisors.Models.AcademicSupervisor]]', 
but this ViewDataDictionary instance requires a model item of type 
'System.Collections.Generic.IReadOnlyList`1[PgrStudentManagement.Web.Modules.AcademicSupervisors.Models.AcademicSupervisor]'.
```

### Cause
In [AcademicSupervisorsController.cs](PgrStudentManagement.Web/Modules/AcademicSupervisors/Controllers/AcademicSupervisorsController.cs), the `Index` action called the asynchronous service method `GetAcademicSupervisorsAsync` without awaiting it:

```csharp
// Previous buggy code:
public async Task<IActionResult> Index(
    CancellationToken cancellationToken = default)
{
    var supervisors =
        _academicSupervisorService
            .GetAcademicSupervisorsAsync(cancellationToken); // <-- Missing await!

    return View(supervisors); // Passed Task<IReadOnlyList<AcademicSupervisor>> to View
}
```

Because `await` was omitted, `supervisors` was an uncompleted `Task<...>` object rather than the supervisor list. When passed into `View(supervisors)`, Razor's model activation failed because [Views/AcademicSupervisors/Index.cshtml](PgrStudentManagement.Web/Views/AcademicSupervisors/Index.cshtml) declared `@model IReadOnlyList<AcademicSupervisor>`.

### Resolution
The call in [AcademicSupervisorsController.cs](PgrStudentManagement.Web/Modules/AcademicSupervisors/Controllers/AcademicSupervisorsController.cs) was corrected to `await` the asynchronous task:

```csharp
// Fixed code:
public async Task<IActionResult> Index(
    CancellationToken cancellationToken = default)
{
    var supervisors =
        await _academicSupervisorService
            .GetAcademicSupervisorsAsync(cancellationToken);

    return View(supervisors);
}
```

This ensures the task resolves to `IReadOnlyList<AcademicSupervisor>` before invoking the view, preventing the `InvalidOperationException`.
