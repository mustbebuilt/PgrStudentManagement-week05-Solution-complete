using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using PgrStudentManagement.Tests.Fixtures;

using PgrStudentManagement.Web.Modules.AcademicSupervisors.Data.Daos;
using PgrStudentManagement.Web.Modules.AcademicSupervisors.Models;

using PgrStudentManagement.Web.Modules.Students.Data;
using PgrStudentManagement.Web.Modules.Students.Models;

using static Xunit.Assert;
using Xunit;

namespace PgrStudentManagement.Tests.Data;

public sealed class AcademicSupervisorDaoTests
{
    [Fact]
    public async Task AddAcademicSupervisorAsync_SavesSupervisorAndRoles()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();

        var dao = new AcademicSupervisorDao(database.Context);

        var supervisor = AcademicSupervisorTestData
            .CreateInstitutionSupervisor(
                "SUP001",
                SupervisoryRole.DirectorOfStudies,
                SupervisoryRole.Adviser);

        await dao.AddAcademicSupervisorAsync(supervisor);

        database.Context.ChangeTracker.Clear();

        var saved = await database.Context
            .AcademicSupervisors
            .Include(existing => existing.EligibleRoleEntries)
            .SingleAsync(existing => existing.StaffNumber == "SUP001");

        Assert.Equal("SUP001", saved.StaffNumber);
        Assert.Equal(2, saved.EligibleRoleEntries.Count);

        Assert.Contains(
            saved.EligibleRoleEntries,
            role => role.Role == SupervisoryRole.DirectorOfStudies);

        Assert.Contains(
            saved.EligibleRoleEntries,
            role => role.Role == SupervisoryRole.Adviser);
    }

    [Fact]
    public async Task GetAcademicSupervisorAsync_LoadsEligibleRoles()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();

        var dao = new AcademicSupervisorDao(database.Context);

        await dao.AddAcademicSupervisorAsync(AcademicSupervisorTestData
            .CreateInstitutionSupervisor(
                "SUP002",
                SupervisoryRole.FirstSupervisor,
                SupervisoryRole.SecondSupervisor)
        );

        database.Context.ChangeTracker.Clear();

        var result = await dao.GetAcademicSupervisorAsync("SUP002");

        Assert.NotNull(result);
        Assert.Equal(
            2,
            result.EligibleRoleEntries.Count);
    }

    [Fact]
    public async Task UpdateEligibleRolesAsync_ReplacesExistingRoles()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();

        var dao = new AcademicSupervisorDao(database.Context);

        await dao.AddAcademicSupervisorAsync(
            AcademicSupervisorTestData
                .CreateInstitutionSupervisor(
                    "SUP003",
                    SupervisoryRole.DirectorOfStudies,
                    SupervisoryRole.Adviser));

        database.Context.ChangeTracker.Clear();

        var updated =
            await dao.UpdateEligibleRolesAsync(
                "SUP003",
                [
                    SupervisoryRole.SecondSupervisor,
                    SupervisoryRole.Adviser
                ]);

        Assert.True(updated);

        database.Context.ChangeTracker.Clear();

        var saved =
            await dao.GetAcademicSupervisorAsync("SUP003");

        Assert.NotNull(saved);

        var roles = saved.EligibleRoleEntries
            .Select(entry => entry.Role)
            .ToHashSet();

        Assert.Equal(2, roles.Count);
        Assert.Contains(SupervisoryRole.SecondSupervisor,roles);
        Assert.Contains(SupervisoryRole.Adviser, roles);
        Assert.DoesNotContain(SupervisoryRole.DirectorOfStudies, roles);
    }

    [Fact]
public async Task
    AddSupervisoryAssignmentAsync_SavesAssignment()
{
    await using var database =
        await SqliteTestDatabase.CreateAsync();

    await SeedStudentAsync(
        database.Context,
        "S100001");

    var dao =
        new AcademicSupervisorDao(
            database.Context);

    await dao.AddAcademicSupervisorAsync(
        AcademicSupervisorTestData
            .CreateInstitutionSupervisor(
                "SUP001"));

    await dao.AddSupervisoryAssignmentAsync(
        new SupervisoryAssignment
        {
            StudentNumber = "S100001",
            SupervisorStaffNumber = "SUP001",
            Role =
                SupervisoryRole.DirectorOfStudies,
            StartDate =
                new DateTime(2026, 10, 1),
            EndDate = null,
            ExceptionAuthorised = false
        });

    database.Context.ChangeTracker.Clear();

    var saved =
        await database.Context
            .SupervisoryAssignments
            .SingleAsync();

    Assert.Equal(
        "S100001",
        saved.StudentNumber);

    Assert.Equal(
        "SUP001",
        saved.SupervisorStaffNumber);
}

    [Fact]
    public async Task SupervisoryAssignmentExistsAsync_ReturnsTrueForCompositeKey()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();

        await SeedStudentAsync(database.Context, "S100001");

        var dao = new AcademicSupervisorDao(database.Context);

        await dao.AddAcademicSupervisorAsync(
            AcademicSupervisorTestData.CreateInstitutionSupervisor("SUP001"));

        var startDate = new DateTime(2026, 10, 1);

        await dao.AddSupervisoryAssignmentAsync(
            new SupervisoryAssignment
            {
                StudentNumber = "S100001",
                SupervisorStaffNumber = "SUP001",
                Role = SupervisoryRole.Adviser,
                StartDate = startDate,
                EndDate = null,
                ExceptionAuthorised = false
            });

        var exists =
            await dao.SupervisoryAssignmentExistsAsync(
                "S100001",
                "SUP001",
                SupervisoryRole.Adviser,
                startDate);

        Assert.True(exists);
    }

    private static async Task SeedStudentAsync(StudentManagementDbContext context, string studentNumber)
    {
        context.Students.Add(
            new Student
            {
                StudentNumber = studentNumber,
                FirstName = "Test",
                LastName = "Student",

                EmailAddress = 
                    $"{studentNumber.ToLowerInvariant()}" + "@example.test",

                Course = "PhD Computing",
                ModeOfStudy = "Full-time",
                StartDate = new DateTime(2026, 10, 1),

                Status = Status.Researching,

                OriginalExpectedSubmissionDate = new DateTime(2029, 9, 30),
                ExpectedSubmissionDate = new DateTime(2029, 9, 30)
            });

        await context.SaveChangesAsync();
    }
}