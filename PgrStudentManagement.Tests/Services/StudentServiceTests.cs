using Microsoft.EntityFrameworkCore;

using PgrStudentManagement.Tests.Fixtures;

using PgrStudentManagement.Web.Modules.Students.Data;
using PgrStudentManagement.Web.Modules.Students.Data.Daos;
using PgrStudentManagement.Web.Modules.Students.Models;
using PgrStudentManagement.Web.Modules.Students.Services;

using Xunit;

namespace PgrStudentManagement.Tests.Services;

public sealed class StudentServiceTests
{
    [Fact]
    public async Task
        GetStudentsAsync_ReturnsStudentsOrderedByNameAndNumber()
    {
        await using var testContext =
            await TestContext.CreateAsync();

        await testContext.StudentDao
            .AddStudentAsync(
                CreateStudent(
                    studentNumber: "S200003",
                    firstName: "Zoe",
                    lastName: "Brown"));

        await testContext.StudentDao
            .AddStudentAsync(
                CreateStudent(
                    studentNumber: "S200002",
                    firstName: "Alice",
                    lastName: "Brown"));

        await testContext.StudentDao
            .AddStudentAsync(
                CreateStudent(
                    studentNumber: "S200001",
                    firstName: "James",
                    lastName: "Carter"));

        var students =
            await testContext.Service
                .GetStudentsAsync();

        Assert.Collection(
            students,
            student =>
                Assert.Equal(
                    "S200002",
                    student.StudentNumber),

            student =>
                Assert.Equal(
                    "S200003",
                    student.StudentNumber),

            student =>
                Assert.Equal(
                    "S200001",
                    student.StudentNumber));
    }

    [Fact]
    public async Task
        GetStudentAsync_WhenStudentExists_ReturnsStudent()
    {
        await using var testContext =
            await TestContext.CreateAsync();

        await testContext.StudentDao
            .AddStudentAsync(
                CreateStudent(
                    studentNumber: "S200001"));

        var result =
            await testContext.Service
                .GetStudentAsync(
                    "S200001");

        Assert.NotNull(result);

        Assert.Equal(
            "S200001",
            result.StudentNumber);
    }

    [Fact]
    public async Task
        GetStudentAsync_WhenStudentDoesNotExist_ReturnsNull()
    {
        await using var testContext =
            await TestContext.CreateAsync();

        var result =
            await testContext.Service
                .GetStudentAsync(
                    "UNKNOWN");

        Assert.Null(result);
    }

    [Fact]
    public async Task
        CreateStudentAsync_WithUniqueStudentNumber_ReturnsSuccess()
    {
        await using var testContext =
            await TestContext.CreateAsync();

        var student =
            CreateStudent(
                studentNumber: "S200001");

        var result =
            await testContext.Service
                .CreateStudentAsync(student);

        Assert.True(result.Succeeded);

        var saved =
            await testContext.StudentDao
                .GetStudentAsync(
                    "S200001");

        Assert.NotNull(saved);
    }

    [Fact]
    public async Task
        CreateStudentAsync_WithDuplicateStudentNumber_ReturnsFailure()
    {
        await using var testContext =
            await TestContext.CreateAsync();

        await testContext.StudentDao
            .AddStudentAsync(
                CreateStudent(
                    studentNumber: "S200001"));

        var duplicate =
            CreateStudent(
                studentNumber: "S200001",
                firstName: "Different",
                lastName: "Student");

        var result =
            await testContext.Service
                .CreateStudentAsync(
                    duplicate);

        Assert.False(result.Succeeded);

        Assert.Equal(
            "A student with this student number "
            + "already exists",
            result.ErrorMessage);

        Assert.Equal(
            1,
            await testContext.Context.Students
                .CountAsync());
    }

    [Fact]
    public async Task
        UpdateStatusAsync_WhenStudentExists_UpdatesStatus()
    {
        await using var testContext =
            await TestContext.CreateAsync();

        await testContext.StudentDao
            .AddStudentAsync(
                CreateStudent(
                    studentNumber: "S200001"));

        testContext.Context.ChangeTracker.Clear();

        var result =
            await testContext.Service
                .UpdateStatusAsync(
                    "S200001",
                    Status.WritingUp);

        Assert.True(result.Succeeded);

        testContext.Context.ChangeTracker.Clear();

        var saved =
            await testContext.StudentDao
                .GetStudentAsync(
                    "S200001");

        Assert.NotNull(saved);

        Assert.Equal(
            Status.WritingUp,
            saved.Status);
    }

    [Fact]
    public async Task
        UpdateStatusAsync_WhenStudentDoesNotExist_ReturnsFailure()
    {
        await using var testContext =
            await TestContext.CreateAsync();

        var result =
            await testContext.Service
                .UpdateStatusAsync(
                    "UNKNOWN",
                    Status.WritingUp);

        Assert.False(result.Succeeded);

        Assert.Equal(
            "Student not found",
            result.ErrorMessage);
    }

    [Fact]
    public async Task
        UpdateExpectedSubmissionDateAsync_WithValidDate_UpdatesDate()
    {
        await using var testContext =
            await TestContext.CreateAsync();

        await testContext.StudentDao
            .AddStudentAsync(
                CreateStudent(
                    studentNumber: "S200001"));

        testContext.Context.ChangeTracker.Clear();

        var expectedDate =
            new DateTime(2030, 3, 31);

        var result =
            await testContext.Service
                .UpdateExpectedSubmissionDateAsync(
                    "S200001",
                    expectedDate);

        Assert.True(result.Succeeded);

        testContext.Context.ChangeTracker.Clear();

        var saved =
            await testContext.StudentDao
                .GetStudentAsync(
                    "S200001");

        Assert.NotNull(saved);

        Assert.Equal(
            expectedDate,
            saved.ExpectedSubmissionDate);
    }

    [Fact]
    public async Task
        UpdateExpectedSubmissionDateAsync_WhenDateIsNull_ReturnsFailure()
    {
        await using var testContext =
            await TestContext.CreateAsync();

        await testContext.StudentDao
            .AddStudentAsync(
                CreateStudent(
                    studentNumber: "S200001"));

        var result =
            await testContext.Service
                .UpdateExpectedSubmissionDateAsync(
                    "S200001",
                    null);

        Assert.False(result.Succeeded);

        Assert.Equal(
            "Invalid expected submission date",
            result.ErrorMessage);
    }

    [Fact]
    public async Task
        UpdateExpectedSubmissionDateAsync_WhenStudentDoesNotExist_ReturnsFailure()
    {
        await using var testContext =
            await TestContext.CreateAsync();

        var result =
            await testContext.Service
                .UpdateExpectedSubmissionDateAsync(
                    "UNKNOWN",
                    new DateTime(2030, 3, 31));

        Assert.False(result.Succeeded);

        Assert.Equal(
            "Student not found",
            result.ErrorMessage);
    }

    [Fact]
    public async Task
        RecordSubmissionAsync_WithValidDate_UpdatesDateAndStatus()
    {
        await using var testContext =
            await TestContext.CreateAsync();

        await testContext.StudentDao
            .AddStudentAsync(
                CreateStudent(
                    studentNumber: "S200001"));

        testContext.Context.ChangeTracker.Clear();

        var submissionDate =
            new DateTime(2029, 8, 15);

        var result =
            await testContext.Service
                .RecordSubmissionAsync(
                    "S200001",
                    submissionDate);

        Assert.True(result.Succeeded);

        testContext.Context.ChangeTracker.Clear();

        var saved =
            await testContext.StudentDao
                .GetStudentAsync(
                    "S200001");

        Assert.NotNull(saved);

        Assert.Equal(
            submissionDate,
            saved.ActualSubmissionDate);

        Assert.Equal(
            Status.Submitted,
            saved.Status);
    }

    [Fact]
    public async Task
        RecordSubmissionAsync_WhenDateIsNull_ReturnsFailure()
    {
        await using var testContext =
            await TestContext.CreateAsync();

        await testContext.StudentDao
            .AddStudentAsync(
                CreateStudent(
                    studentNumber: "S200001"));

        var result =
            await testContext.Service
                .RecordSubmissionAsync(
                    "S200001",
                    null);

        Assert.False(result.Succeeded);

        Assert.Equal(
            "Invalid submission date",
            result.ErrorMessage);
    }

    [Fact]
    public async Task
        RecordSubmissionAsync_WhenStudentDoesNotExist_ReturnsFailure()
    {
        await using var testContext =
            await TestContext.CreateAsync();

        var result =
            await testContext.Service
                .RecordSubmissionAsync(
                    "UNKNOWN",
                    new DateTime(2029, 8, 15));

        Assert.False(result.Succeeded);

        Assert.Equal(
            "Student not found",
            result.ErrorMessage);
    }

    [Fact]
    public async Task
        GetStudentSummaryAsync_WhenStudentExists_ReturnsSummary()
    {
        await using var testContext =
            await TestContext.CreateAsync();

        await testContext.StudentDao
            .AddStudentAsync(
                CreateStudent(
                    studentNumber: "S200001",
                    firstName: "Alex",
                    lastName: "Morgan"));

        var summary =
            await testContext.Service
                .GetStudentSummaryAsync(
                    "S200001");

        Assert.NotNull(summary);

        Assert.Equal(
            "S200001",
            summary.StudentNumber);

        Assert.Equal(
            "Alex Morgan",
            summary.DisplayName);
    }

    [Fact]
    public async Task
        GetStudentSummaryAsync_WhenStudentDoesNotExist_ReturnsNull()
    {
        await using var testContext =
            await TestContext.CreateAsync();

        var summary =
            await testContext.Service
                .GetStudentSummaryAsync(
                    "UNKNOWN");

        Assert.Null(summary);
    }

    [Fact]
    public async Task
        GetEnrolmentsForStudentAsync_ReturnsEnrolments()
    {
        await using var testContext =
            await TestContext.CreateAsync();

        await testContext.StudentDao
            .AddStudentAsync(
                CreateStudent(
                    studentNumber: "S200001"));

        await testContext.EnrolmentDao
            .AddEnrolmentAsync(
                new Enrolment
                {
                    EnrolmentId = 1,
                    StudentNumber = "S200001",
                    ProgrammeCode = "PHD-COMP",

                    StartDate =
                        new DateTime(2026, 10, 1),

                    ExpectedEndDate =
                        new DateTime(2029, 9, 30),

                    ModeOfStudy = "Full-time",
                    Status = EnrolmentStatus.Active
                });

        var enrolments =
            await testContext.Service
                .GetEnrolmentsForStudentAsync(
                    "S200001");

        var enrolment =
            Assert.Single(enrolments);

        Assert.Equal(
            "PHD-COMP",
            enrolment.ProgrammeCode);
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

    private sealed class TestContext
        : IAsyncDisposable
    {
        public StudentManagementDbContext Context
        {
            get;
        }

        public StudentDao StudentDao
        {
            get;
        }

        public EnrolmentDao EnrolmentDao
        {
            get;
        }

        public StudentService Service
        {
            get;
        }

        private readonly SqliteTestDatabase _database;

        private TestContext(
            SqliteTestDatabase database,
            StudentDao studentDao,
            EnrolmentDao enrolmentDao,
            StudentService service)
        {
            _database = database;
            Context = database.Context;
            StudentDao = studentDao;
            EnrolmentDao = enrolmentDao;
            Service = service;
        }

        public static async Task<TestContext>
            CreateAsync()
        {
            var database =
                await SqliteTestDatabase
                    .CreateAsync();

            var studentDao =
                new StudentDao(
                    database.Context);

            var enrolmentDao =
                new EnrolmentDao(
                    database.Context);

            var service =
                new StudentService(
                    studentDao,
                    enrolmentDao);

            return new TestContext(
                database,
                studentDao,
                enrolmentDao,
                service);
        }

        public async ValueTask DisposeAsync()
        {
            await _database.DisposeAsync();
        }
    }
}