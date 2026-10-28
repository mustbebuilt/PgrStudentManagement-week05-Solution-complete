using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using PgrStudentManagement.Tests.TestDoubles;

using PgrStudentManagement.Web.Modules.AcademicSupervisors.Data.Daos;
using PgrStudentManagement.Web.Modules.AcademicSupervisors.Models;
using PgrStudentManagement.Web.Modules.AcademicSupervisors.Services;

using PgrStudentManagement.Web.Modules.Students.Data;
using PgrStudentManagement.Web.Modules.Students.Models;
using Xunit;

namespace PgrStudentManagement.Tests.Services;

public sealed class AcademicSupervisorServiceTests
{
    [Fact]
    public async Task GetAcademicSupervisorsAsync_ReturnsSupervisorsInNameOrder()
    {
        await using var testContext = await TestContext.CreateAsync();

        await testContext.Dao
            .AddAcademicSupervisorAsync(
                CreateSupervisor(
                    staffNumber: "SUP002",
                    firstName: "Zoe",
                    lastName: "Brown"));

        await testContext.Dao
            .AddAcademicSupervisorAsync(
                CreateSupervisor(
                    staffNumber: "SUP003",
                    firstName: "Alice",
                    lastName: "Brown"));

        await testContext.Dao
            .AddAcademicSupervisorAsync(
                CreateSupervisor(
                    staffNumber: "SUP001",
                    firstName: "James",
                    lastName: "Carter"));

        var supervisors = await testContext.Service
                .GetAcademicSupervisorsAsync();

        Assert.Collection(
            supervisors,
            supervisor =>
                Assert.Equal(
                    "SUP003",
                    supervisor.StaffNumber),
            supervisor =>
                Assert.Equal(
                    "SUP002",
                    supervisor.StaffNumber),
            supervisor =>
                Assert.Equal(
                    "SUP001",
                    supervisor.StaffNumber));
    }

    [Fact]
    public async Task GetAcademicSupervisorAsync_WhenSupervisorExists_ReturnsSupervisor()
    {
        await using var testContext = await TestContext.CreateAsync();

        await testContext.Dao
            .AddAcademicSupervisorAsync(
                CreateSupervisor(staffNumber: "SUP001"));

        var supervisor =
            await testContext.Service.GetAcademicSupervisorAsync("SUP001");

        Assert.NotNull(supervisor);
        Assert.Equal(
            "SUP001",
            supervisor.StaffNumber);
    }

    [Fact]
    public async Task GetAcademicSupervisorAsync_WhenSupervisorDoesNotExist_ReturnsNull()
    {
        await using var testContext = await TestContext.CreateAsync();

        var supervisor =
            await testContext.Service.GetAcademicSupervisorAsync("UNKNOWN");

        Assert.Null(supervisor);
    }

    [Fact]
    public async Task CreateAcademicSupervisorAsync_WithUniqueStaffNumber_ReturnsCreated()
    {
        await using var testContext = await TestContext.CreateAsync();

        var supervisor =
            CreateSupervisor(staffNumber: "SUP001");

        var outcome =
            await testContext.Service.CreateAcademicSupervisorAsync(supervisor);

        Assert.Equal(
            SupervisorOutcome.Created,
            outcome);

        var saved =
            await testContext.Dao
                .GetAcademicSupervisorAsync(
                    "SUP001");

        Assert.NotNull(saved);
        Assert.Equal(
            "SUP001",
            saved.StaffNumber);
    }

    [Fact]
    public async Task CreateAcademicSupervisorAsync_WithDuplicateStaffNumber_ReturnsDuplicateStaffNumber()
    {
        await using var testContext = await TestContext.CreateAsync();

        await testContext.Dao
            .AddAcademicSupervisorAsync(CreateSupervisor(staffNumber: "SUP001"));

        var duplicate =
            CreateSupervisor(
                staffNumber: "SUP001",
                firstName: "Different",
                lastName: "Person");

        var outcome =
            await testContext.Service.CreateAcademicSupervisorAsync(duplicate);

        Assert.Equal(
            SupervisorOutcome.DuplicateStaffNumber,
            outcome);

        Assert.Single(
            await testContext.Context
                .AcademicSupervisors
                .ToListAsync());
    }

    [Fact]
    public async Task PrepareAssignmentAsync_WhenStudentExists_ReturnsStudentSupervisorsAndRoles()
    {
        await using var testContext = await TestContext.CreateAsync();

        testContext.StudentLookup.AddStudent(
            "S100001",
            "Test Student");

        await testContext.Dao
            .AddAcademicSupervisorAsync(
                CreateSupervisor(
                    staffNumber: "SUP002",
                    firstName: "Zoe",
                    lastName: "Brown"));

        await testContext.Dao
            .AddAcademicSupervisorAsync(
                CreateSupervisor(
                    staffNumber: "SUP001",
                    firstName: "Alice",
                    lastName: "Brown"));

        var result =
            await testContext.Service
                .PrepareAssignmentAsync(
                    "S100001");

        Assert.NotNull(result.Student);

        Assert.Equal(
            "S100001",
            result.Student.StudentNumber);

        Assert.Equal(
            2,
            result.Supervisors.Count);

        Assert.Equal(
            "SUP001",
            result.Supervisors[0].StaffNumber);

        Assert.NotEmpty(
            result.AvailableRoles);
    }

    [Fact]
    public async Task PrepareAssignmentAsync_WhenStudentDoesNotExist_ReturnsNullStudent()
    {
        await using var testContext = await TestContext.CreateAsync();

        var result =
            await testContext.Service.PrepareAssignmentAsync("UNKNOWN");

        Assert.Null(result.Student);
    }

    [Fact]
    public async Task AssignSupervisorAsync_WhenStudentDoesNotExist_ReturnsStudentNotFound()
    {
        await using var testContext = await TestContext.CreateAsync();

        var outcome =
            await testContext.Service
                .AssignSupervisorAsync(
                    CreateAssignmentRequest(
                        studentNumber: "UNKNOWN",
                        staffNumber: "SUP001"));

        Assert.Equal(
            AssignmentOutcome.StudentNotFound,
            outcome);

        Assert.Empty(
            await testContext.Context
                .SupervisoryAssignments
                .ToListAsync());
    }

    [Fact]
    public async Task AssignSupervisorAsync_WhenSupervisorDoesNotExist_ReturnsSupervisorNotFound()
    {
        await using var testContext = await TestContext.CreateAsync();

        await testContext.AddStudentAsync("S100001");

        var outcome =
            await testContext.Service
                .AssignSupervisorAsync(
                    CreateAssignmentRequest(
                        studentNumber: "S100001",
                        staffNumber: "UNKNOWN"));

        Assert.Equal(
            AssignmentOutcome.SupervisorNotFound,
            outcome);
    }

    [Fact]
    public async Task AssignSupervisorAsync_WhenAssignmentAlreadyExists_ReturnsDuplicateAssignment()
    {
        await using var testContext = await TestContext.CreateAsync();

        await testContext.AddStudentAsync(
            "S100001");

        await testContext.Dao
            .AddAcademicSupervisorAsync(
                CreateSupervisor(
                    staffNumber: "SUP001",
                    eligibleRoles:
                    [
                        SupervisoryRole.Adviser
                    ]));

        await testContext.Dao
            .AddSupervisoryAssignmentAsync(
                CreateAssignment(
                    studentNumber: "S100001",
                    staffNumber: "SUP001",
                    role: SupervisoryRole.Adviser));

        var outcome =
            await testContext.Service
                .AssignSupervisorAsync(
                    CreateAssignmentRequest(
                        studentNumber: "S100001",
                        staffNumber: "SUP001",
                        role: SupervisoryRole.Adviser));

        Assert.Equal(
            AssignmentOutcome.DuplicateAssignment,
            outcome);
    }

    [Theory]
    [InlineData(SupervisoryRole.DirectorOfStudies)]
    [InlineData(SupervisoryRole.FirstSupervisor)]
    public async Task 
        AssignSupervisorAsync_WhenUniqueRoleIsAlreadyFilled_ReturnsRoleAlreadyFilled(
            SupervisoryRole role)
    {
        await using var testContext = await TestContext.CreateAsync();

        await testContext.AddStudentAsync("S100001");

        await testContext.Dao
            .AddAcademicSupervisorAsync(
                CreateSupervisor(
                    staffNumber: "SUP001",
                    eligibleRoles: [role]));

        await testContext.Dao
            .AddAcademicSupervisorAsync(
                CreateSupervisor(
                    staffNumber: "SUP002",
                    eligibleRoles: [role]));

        await testContext.Dao
            .AddSupervisoryAssignmentAsync(
                CreateAssignment(
                    studentNumber: "S100001",
                    staffNumber: "SUP001",
                    role: role));

        var outcome =
            await testContext.Service
                .AssignSupervisorAsync(
                    CreateAssignmentRequest(
                        studentNumber: "S100001",
                        staffNumber: "SUP002",
                        role: role));

        Assert.Equal(
            AssignmentOutcome.RoleAlreadyFilled,
            outcome);
    }

    [Fact]
    public async Task AssignSupervisorAsync_WhenSupervisorIsNotEligible_ReturnsSupervisorNotEligible()
    {
        await using var testContext = await TestContext.CreateAsync();

        await testContext.AddStudentAsync("S100001");

        await testContext.Dao
            .AddAcademicSupervisorAsync(
                CreateSupervisor(
                    staffNumber: "SUP001",
                    eligibleRoles:
                    [
                        SupervisoryRole.Adviser
                    ]));

        var outcome =
            await testContext.Service
                .AssignSupervisorAsync(
                    CreateAssignmentRequest(
                        studentNumber: "S100001",
                        staffNumber: "SUP001",
                        role:
                            SupervisoryRole
                                .FirstSupervisor));

        Assert.Equal(
            AssignmentOutcome.SupervisorNotEligible,
            outcome);
    }

    [Theory]
    [InlineData(SupervisoryRole.DirectorOfStudies)]
    [InlineData(SupervisoryRole.FirstSupervisor)]
    public async Task 
        AssignSupervisorAsync_WhenExternalSupervisorIsUsedForRestrictedRole_ReturnsEmploymentTypeNotPermitted(
            SupervisoryRole role)
    {
        await using var testContext = await TestContext.CreateAsync();

        await testContext.AddStudentAsync("S100001");

        await testContext.Dao
            .AddAcademicSupervisorAsync(
                CreateSupervisor(
                    staffNumber: "EXT001",
                    employmentType:
                        SupervisorEmploymentType
                            .External,
                    eligibleRoles: [role]));

        var outcome =
            await testContext.Service
                .AssignSupervisorAsync(
                    CreateAssignmentRequest(
                        studentNumber: "S100001",
                        staffNumber: "EXT001",
                        role: role));

        Assert.Equal(
            AssignmentOutcome.EmploymentTypeNotPermitted,
            outcome);
    }

    [Fact]
    public async Task AssignSupervisorAsync_WhenAddingFourthSupervisorWithoutException_ReturnsExceptionRequired()
    {
        await using var testContext = await TestContext.CreateAsync();

        await testContext.AddStudentAsync("S100001");

        await AddSupervisorAndAssignmentAsync(
            testContext,
            "SUP001",
            SupervisoryRole.DirectorOfStudies);

        await AddSupervisorAndAssignmentAsync(
            testContext,
            "SUP002",
            SupervisoryRole.FirstSupervisor);

        await AddSupervisorAndAssignmentAsync(
            testContext,
            "SUP003",
            SupervisoryRole.SecondSupervisor);

        await testContext.Dao
            .AddAcademicSupervisorAsync(
                CreateSupervisor(
                    staffNumber: "SUP004",
                    eligibleRoles:
                    [
                        SupervisoryRole
                            .SecondSupervisor
                    ]));

        var outcome =
            await testContext.Service
                .AssignSupervisorAsync(
                    CreateAssignmentRequest(
                        studentNumber: "S100001",
                        staffNumber: "SUP004",
                        role:
                            SupervisoryRole
                                .SecondSupervisor,
                        exceptionAuthorised: false));

        Assert.Equal(
            AssignmentOutcome.ExceptionRequired,
            outcome);
    }

    [Fact]
    public async Task AssignSupervisorAsync_WhenAddingFourthSupervisorWithException_CreatesAssignment()
    {
        await using var testContext = await TestContext.CreateAsync();

        await testContext.AddStudentAsync("S100001");

        await AddSupervisorAndAssignmentAsync(
            testContext,
            "SUP001",
            SupervisoryRole.DirectorOfStudies);

        await AddSupervisorAndAssignmentAsync(
            testContext,
            "SUP002",
            SupervisoryRole.FirstSupervisor);

        await AddSupervisorAndAssignmentAsync(
            testContext,
            "SUP003",
            SupervisoryRole.SecondSupervisor);

        await testContext.Dao
            .AddAcademicSupervisorAsync(
                CreateSupervisor(
                    staffNumber: "SUP004",
                    eligibleRoles:
                    [
                        SupervisoryRole
                            .SecondSupervisor
                    ]));

        var startDate = new DateTime(2026, 11, 1);

        var outcome =
            await testContext.Service
                .AssignSupervisorAsync(
                    CreateAssignmentRequest(
                        studentNumber: "S100001",
                        staffNumber: "SUP004",
                        role:
                            SupervisoryRole
                                .SecondSupervisor,
                        startDate: startDate,
                        exceptionAuthorised: true));

        Assert.Equal(
            AssignmentOutcome.Created,
            outcome);

        Assert.True(
            await testContext.Dao
                .SupervisoryAssignmentExistsAsync(
                    "S100001",
                    "SUP004",
                    SupervisoryRole.SecondSupervisor,
                    startDate));
    }

    [Fact]
    public async Task AssignSupervisorAsync_WithValidRequest_CreatesAssignment()
    {
        await using var testContext = await TestContext.CreateAsync();

        await testContext.AddStudentAsync("S100001");

        await testContext.Dao
            .AddAcademicSupervisorAsync(
                CreateSupervisor(
                    staffNumber: "SUP001",
                    eligibleRoles:
                    [
                        SupervisoryRole
                            .DirectorOfStudies
                    ]));

        var startDate = new DateTime(2026, 10, 1);

        var outcome =
            await testContext.Service
                .AssignSupervisorAsync(
                    CreateAssignmentRequest(
                        studentNumber: "S100001",
                        staffNumber: "SUP001",
                        role:
                            SupervisoryRole
                                .DirectorOfStudies,
                        startDate: startDate));

        Assert.Equal(
            AssignmentOutcome.Created,
            outcome);

        Assert.True(
            await testContext.Dao
                .SupervisoryAssignmentExistsAsync(
                    "S100001",
                    "SUP001",
                    SupervisoryRole
                        .DirectorOfStudies,
                    startDate));
    }

    [Fact]
    public async Task GetSupervisoryTeamAsync_WhenStudentDoesNotExist_ReturnsStudentFoundFalse()
    {
        await using var testContext = await TestContext.CreateAsync();

        var result =
            await testContext.Service
                .GetSupervisoryTeamAsync(
                    "UNKNOWN");

        Assert.False(result.StudentFound);
        Assert.Null(result.Student);
        Assert.Empty(result.CurrentAssignments);
        Assert.Empty(result.HistoricalAssignments);
    }

    [Fact]
    public async Task GetSupervisoryTeamAsync_WhenStudentHasNoAssignments_ReturnsStudentFoundTrueWithEmptyLists()
    {
        await using var testContext = await TestContext.CreateAsync();

        await testContext.AddStudentAsync("S100001");

        var result =
            await testContext.Service
                .GetSupervisoryTeamAsync(
                    "S100001");

        Assert.True(result.StudentFound);
        Assert.NotNull(result.Student);
        Assert.Empty(result.CurrentAssignments);
        Assert.Empty(result.HistoricalAssignments);
    }

    [Fact]
    public async Task GetSupervisoryTeamAsync_SeparatesCurrentAndHistoricalAssignments()
    {
        await using var testContext = await TestContext.CreateAsync();

        await testContext.AddStudentAsync("S100001");

        await testContext.Dao
            .AddAcademicSupervisorAsync(
                CreateSupervisor(
                    staffNumber: "SUP001",
                    firstName: "Current",
                    lastName: "Supervisor"));

        await testContext.Dao
            .AddAcademicSupervisorAsync(
                CreateSupervisor(
                    staffNumber: "SUP002",
                    firstName: "Former",
                    lastName: "Supervisor"));

        await testContext.Dao
            .AddSupervisoryAssignmentAsync(
                CreateAssignment(
                    studentNumber: "S100001",
                    staffNumber: "SUP001",
                    role:
                        SupervisoryRole
                            .DirectorOfStudies,
                    startDate:
                        new DateTime(2026, 10, 1)));

        await testContext.Dao
            .AddSupervisoryAssignmentAsync(
                CreateAssignment(
                    studentNumber: "S100001",
                    staffNumber: "SUP002",
                    role:
                        SupervisoryRole
                            .FirstSupervisor,
                    startDate: new DateTime(2025, 10, 1),
                    endDate: new DateTime(2026, 9, 30)));

        var result =
            await testContext.Service.GetSupervisoryTeamAsync("S100001");

        Assert.True(result.StudentFound);

        Assert.Single(result.CurrentAssignments);

        Assert.Single(result.HistoricalAssignments);

        Assert.Equal(
            "SUP001",
            result.CurrentAssignments[0]
                .SupervisorStaffNumber);

        Assert.Equal(
            "SUP002",
            result.HistoricalAssignments[0]
                .SupervisorStaffNumber);
    }

    private static AcademicSupervisor CreateSupervisor(
            string staffNumber,
            string firstName = "Robin",
            string lastName = "Carter",
            SupervisorEmploymentType employmentType = SupervisorEmploymentType.InstitutionEmployed,
            SupervisoryRole[]? eligibleRoles = null)
    {
        eligibleRoles ??=
        [
            SupervisoryRole.DirectorOfStudies,
            SupervisoryRole.FirstSupervisor,
            SupervisoryRole.SecondSupervisor,
            SupervisoryRole.Adviser
        ];

        return new AcademicSupervisor
        {
            StaffNumber = staffNumber,
            FirstName = firstName,
            LastName = lastName,
            EmailAddress =
                $"{staffNumber.ToLowerInvariant()}" +
                "@example.test",

            EmploymentType = employmentType,
            TrainingCompleted = true,

            EligibleRoleEntries =
                eligibleRoles
                    .Distinct()
                    .Select(role =>
                        new AcademicSupervisorEligibleRole
                        {
                            SupervisorStaffNumber =
                                staffNumber,

                            Role = role
                        })
                    .ToList()
        };
    }

    private static AssignmentRequest CreateAssignmentRequest(
            string studentNumber,
            string staffNumber,
            SupervisoryRole role =
                SupervisoryRole.Adviser,
            DateTime? startDate = null,
            bool exceptionAuthorised = false)
    {
        return new AssignmentRequest
        {
            StudentNumber = studentNumber,
            StaffNumber = staffNumber,
            Role = role,
            StartDate =
                startDate
                ?? new DateTime(2026, 10, 1),

            ExceptionAuthorised =
                exceptionAuthorised
        };
    }

    private static SupervisoryAssignment CreateAssignment(
            string studentNumber,
            string staffNumber,
            SupervisoryRole role,
            DateTime? startDate = null,
            DateTime? endDate = null)
    {
        return new SupervisoryAssignment
        {
            StudentNumber = studentNumber,
            SupervisorStaffNumber =
                staffNumber,

            Role = role,
            StartDate =
                startDate
                ?? new DateTime(2026, 10, 1),

            EndDate = endDate,
            ExceptionAuthorised = false
        };
    }

    private static async Task AddSupervisorAndAssignmentAsync(
            TestContext testContext,
            string staffNumber,
            SupervisoryRole role)
    {
        await testContext.Dao
            .AddAcademicSupervisorAsync(
                CreateSupervisor(
                    staffNumber: staffNumber,
                    eligibleRoles: [role]));

        await testContext.Dao
            .AddSupervisoryAssignmentAsync(
                CreateAssignment(
                    studentNumber: "S100001",
                    staffNumber: staffNumber,
                    role: role,
                    startDate:
                        staffNumber switch
                        {
                            "SUP001" =>
                                new DateTime(
                                    2026,
                                    1,
                                    1),

                            "SUP002" =>
                                new DateTime(
                                    2026,
                                    2,
                                    1),

                            _ =>
                                new DateTime(
                                    2026,
                                    3,
                                    1)
                        }));
    }

    private sealed class TestContext
        : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        public StudentManagementDbContext Context { get; }

        public AcademicSupervisorDao Dao { get; }

        public FakeStudentLookup StudentLookup { get; }

        public AcademicSupervisorService Service { get; }

        private TestContext(
            SqliteConnection connection,
            StudentManagementDbContext context,
            AcademicSupervisorDao dao,
            FakeStudentLookup studentLookup,
            AcademicSupervisorService service)
        {
            _connection = connection;
            Context = context;
            Dao = dao;
            StudentLookup = studentLookup;
            Service = service;
        }

        public static async Task<TestContext> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");

            await connection.OpenAsync();

            var options =
                new DbContextOptionsBuilder<StudentManagementDbContext>()
                    .UseSqlite(connection)
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging()
                    .Options;

            var context = new StudentManagementDbContext(options);

            await context.Database.EnsureCreatedAsync();

            var dao = new AcademicSupervisorDao(context);

            var studentLookup = new FakeStudentLookup();

            var service =
                new AcademicSupervisorService(dao, studentLookup);

            return new TestContext(
                connection,
                context,
                dao,
                studentLookup,
                service);
        }

        public async Task AddStudentAsync(string studentNumber)
        {
            Context.Students.Add(
                new Student
                {
                    StudentNumber = studentNumber,
                    FirstName = "Test",
                    LastName = "Student",
                    EmailAddress = $"{studentNumber.ToLowerInvariant()}" + "@example.test",
                    Course = "PhD Computing",
                    ModeOfStudy = "Full-time",
                    StartDate = new DateTime(2026, 10, 1),
                    Status = Status.Researching,
                    ThesisTitle = null,
                    OriginalExpectedSubmissionDate = new DateTime(2029, 9, 30),
                    ExpectedSubmissionDate = new DateTime(2029, 9, 30)
                });

            await Context.SaveChangesAsync();

            StudentLookup.AddStudent(studentNumber, "Test Student");
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}