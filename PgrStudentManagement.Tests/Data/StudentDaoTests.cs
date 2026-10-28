using Microsoft.EntityFrameworkCore;

using PgrStudentManagement.Tests.Fixtures;

using PgrStudentManagement.Web.Modules.Students.Data.Daos;
using PgrStudentManagement.Web.Modules.Students.Models;

using Xunit;

namespace PgrStudentManagement.Tests.Data;

public sealed class StudentDaoTests
{
    [Fact]
    public async Task
        GetStudentsAsync_WhenNoStudentsExist_ReturnsEmptyCollection()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var dao =
            new StudentDao(database.Context);

        var students =
            await dao.GetStudentsAsync();

        Assert.Empty(students);
    }

    [Fact]
    public async Task
        AddStudentAsync_SavesStudent()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var dao =
            new StudentDao(database.Context);

        var student =
            CreateStudent(
                studentNumber: "S200001",
                firstName: "Alex",
                lastName: "Morgan");

        await dao.AddStudentAsync(student);

        database.Context.ChangeTracker.Clear();

        var saved =
            await database.Context.Students
                .AsNoTracking()
                .SingleAsync(existing =>
                    existing.StudentNumber
                        == "S200001");

        Assert.Equal(
            "S200001",
            saved.StudentNumber);

        Assert.Equal(
            "Alex",
            saved.FirstName);

        Assert.Equal(
            "Morgan",
            saved.LastName);

        Assert.Equal(
            "alex.morgan@example.test",
            saved.EmailAddress);
    }

    [Fact]
    public async Task
        GetStudentsAsync_ReturnsAllStudents()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var dao =
            new StudentDao(database.Context);

        await dao.AddStudentAsync(
            CreateStudent(
                studentNumber: "S200001"));

        await dao.AddStudentAsync(
            CreateStudent(
                studentNumber: "S200002"));

        database.Context.ChangeTracker.Clear();

        var students =
            await dao.GetStudentsAsync();

        Assert.Equal(2, students.Count);

        Assert.Contains(
            students,
            student =>
                student.StudentNumber
                    == "S200001");

        Assert.Contains(
            students,
            student =>
                student.StudentNumber
                    == "S200002");
    }

    [Fact]
    public async Task
        GetStudentAsync_WhenStudentExists_ReturnsStudent()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var dao =
            new StudentDao(database.Context);

        await dao.AddStudentAsync(
            CreateStudent(
                studentNumber: "S200001",
                firstName: "Taylor",
                lastName: "Brown"));

        database.Context.ChangeTracker.Clear();

        var result =
            await dao.GetStudentAsync(
                "S200001");

        Assert.NotNull(result);

        Assert.Equal(
            "S200001",
            result.StudentNumber);

        Assert.Equal(
            "Taylor",
            result.FirstName);

        Assert.Equal(
            "Brown",
            result.LastName);
    }

    [Fact]
    public async Task
        GetStudentAsync_WhenStudentDoesNotExist_ReturnsNull()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var dao =
            new StudentDao(database.Context);

        var result =
            await dao.GetStudentAsync(
                "UNKNOWN");

        Assert.Null(result);
    }

    [Fact]
    public async Task
        StudentNumberExistsAsync_WhenStudentExists_ReturnsTrue()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var dao =
            new StudentDao(database.Context);

        await dao.AddStudentAsync(
            CreateStudent(
                studentNumber: "S200001"));

        var exists =
            await dao.StudentNumberExistsAsync(
                "S200001");

        Assert.True(exists);
    }

    [Fact]
    public async Task
        StudentNumberExistsAsync_WhenStudentDoesNotExist_ReturnsFalse()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var dao =
            new StudentDao(database.Context);

        var exists =
            await dao.StudentNumberExistsAsync(
                "UNKNOWN");

        Assert.False(exists);
    }

    [Fact]
    public async Task
        AddStudentAsync_WhenStudentNumberAlreadyExists_ThrowsDbUpdateException()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var dao =
            new StudentDao(database.Context);

        await dao.AddStudentAsync(
            CreateStudent(
                studentNumber: "S200001"));

        database.Context.ChangeTracker.Clear();

        var duplicate =
            CreateStudent(
                studentNumber: "S200001",
                firstName: "Different",
                lastName: "Student");

        await Assert.ThrowsAsync<DbUpdateException>(
            () => dao.AddStudentAsync(
                duplicate));
    }

    [Fact]
    public async Task
        UpdateStudentAsync_WhenStudentExists_UpdatesStudent()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var dao =
            new StudentDao(database.Context);

        await dao.AddStudentAsync(
            CreateStudent(
                studentNumber: "S200001"));

        database.Context.ChangeTracker.Clear();

        var updatedStudent =
            CreateStudent(
                studentNumber: "S200001",
                firstName: "Updated",
                lastName: "Name");

        updatedStudent.Status =
            Status.WritingUp;

        updatedStudent.ExpectedSubmissionDate =
            new DateTime(2030, 3, 31);

        var updated =
            await dao.UpdateStudentAsync(
                updatedStudent);

        Assert.True(updated);

        database.Context.ChangeTracker.Clear();

        var saved =
            await database.Context.Students
                .AsNoTracking()
                .SingleAsync(student =>
                    student.StudentNumber
                        == "S200001");

        Assert.Equal(
            "Updated",
            saved.FirstName);

        Assert.Equal(
            "Name",
            saved.LastName);

        Assert.Equal(
            Status.WritingUp,
            saved.Status);

        Assert.Equal(
            new DateTime(2030, 3, 31),
            saved.ExpectedSubmissionDate);
    }

    [Fact]
    public async Task
        UpdateStudentAsync_WhenStudentDoesNotExist_ReturnsFalse()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var dao =
            new StudentDao(database.Context);

        var student =
            CreateStudent(
                studentNumber: "UNKNOWN");

        var updated =
            await dao.UpdateStudentAsync(
                student);

        Assert.False(updated);

        Assert.Empty(
            await database.Context.Students
                .ToListAsync());
    }

    [Fact]
    public async Task
        GetStudentAsync_ReturnsNoTrackingEntity()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var dao =
            new StudentDao(database.Context);

        await dao.AddStudentAsync(
            CreateStudent(
                studentNumber: "S200001"));

        database.Context.ChangeTracker.Clear();

        var student =
            await dao.GetStudentAsync(
                "S200001");

        Assert.NotNull(student);

        var isTracked =
            database.Context.ChangeTracker
                .Entries<Student>()
                .Any(entry =>
                    entry.Entity.StudentNumber
                        == "S200001");

        Assert.False(isTracked);
    }

    private static Student CreateStudent(
        string studentNumber,
        string firstName = "Test",
        string lastName = "Student")
    {
        return new Student
        {
            StudentNumber = studentNumber,
            FirstName = firstName,
            LastName = lastName,

            EmailAddress =
                $"{firstName.ToLowerInvariant()}."
                + $"{lastName.ToLowerInvariant()}"
                + "@example.test",

            Course = "PhD Computing",
            ModeOfStudy = "Full-time",

            StartDate =
                new DateTime(2026, 10, 1),

            Status =
                Status.Researching,

            ThesisTitle =
                "A Test Thesis",

            OriginalExpectedSubmissionDate =
                new DateTime(2029, 9, 30),

            ExpectedSubmissionDate =
                new DateTime(2029, 9, 30),

            ActualSubmissionDate = null
        };
    }
}