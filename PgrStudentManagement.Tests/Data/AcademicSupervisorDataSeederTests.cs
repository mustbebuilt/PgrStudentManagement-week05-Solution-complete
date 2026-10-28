using Microsoft.EntityFrameworkCore;
using PgrStudentManagement.Tests.Fixtures;
using PgrStudentManagement.Web.Modules.AcademicSupervisors.Data;
using PgrStudentManagement.Web.Modules.AcademicSupervisors.Data.Daos;
using PgrStudentManagement.Web.Modules.Students.Data;
using PgrStudentManagement.Web.Modules.Students.Data.Daos;
using PgrStudentManagement.Web.Modules.Students.Services;
using Xunit;

namespace PgrStudentManagement.Tests.Data;

public sealed class AcademicSupervisorDataSeederTests
{
    [Fact]
    public async Task SeedAsync_AddsSupervisorsRolesAndAssignments()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();

        var studentDao = new StudentDao(database.Context);

        var enrolmentDao = new EnrolmentDao(database.Context);

        await new StudentDataSeeder(studentDao).SeedAsync();

        var studentService =
            new StudentService(studentDao, enrolmentDao);

        var supervisorDao = new AcademicSupervisorDao(database.Context);

        var seeder =
            new AcademicSupervisorDataSeeder(supervisorDao, studentService);

        await seeder.SeedAsync();

        Assert.NotEmpty(
            await database.Context
                .AcademicSupervisors
                .ToListAsync());

        Assert.NotEmpty(
            await database.Context
                .AcademicSupervisorEligibleRoles
                .ToListAsync());

        Assert.NotEmpty(
            await database.Context
                .SupervisoryAssignments
                .ToListAsync());
    }

    [Fact]
    public async Task SeedAsync_WhenCalledTwice_DoesNotDuplicateData()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();

        var studentDao = new StudentDao(database.Context);

        var enrolmentDao = new EnrolmentDao(database.Context);

        await new StudentDataSeeder(studentDao).SeedAsync();

        var seeder =
            new AcademicSupervisorDataSeeder(
                new AcademicSupervisorDao(database.Context),
                new StudentService(studentDao, enrolmentDao));

        await seeder.SeedAsync();

        int supervisorCount =
            await database.Context
                .AcademicSupervisors
                .CountAsync();

        int roleCount =
            await database.Context
                .AcademicSupervisorEligibleRoles
                .CountAsync();

        int assignmentCount =
            await database.Context
                .SupervisoryAssignments
                .CountAsync();

        database.Context.ChangeTracker.Clear();

        await seeder.SeedAsync();

        Assert.Equal(
            supervisorCount,
            await database.Context
                .AcademicSupervisors
                .CountAsync());

        Assert.Equal(
            roleCount,
            await database.Context
                .AcademicSupervisorEligibleRoles
                .CountAsync());

        Assert.Equal(
            assignmentCount,
            await database.Context
                .SupervisoryAssignments
                .CountAsync());
    }
}