using Microsoft.EntityFrameworkCore;
using PgrStudentManagement.Tests.Fixtures;
using PgrStudentManagement.Web.Modules.AcademicSupervisors.Data;
using PgrStudentManagement.Web.Modules.AcademicSupervisors.Data.Daos;
using PgrStudentManagement.Web.Modules.Students.Data;
using PgrStudentManagement.Web.Modules.Students.Data.Daos;
using PgrStudentManagement.Web.Modules.Students.Services;
using Xunit;

namespace PgrStudentManagement.Tests.Data;

public sealed class ApplicationSeedOrderTests
{
    [Fact]
    public async Task StudentsMustExistBeforeSupervisorAssignmentsAreSeeded()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();

        var studentDao = new StudentDao(database.Context);

        var enrolmentDao = new EnrolmentDao(database.Context);

        var supervisorSeeder =
            new AcademicSupervisorDataSeeder(
                new AcademicSupervisorDao(database.Context),
                new StudentService(studentDao, enrolmentDao));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => supervisorSeeder.SeedAsync());

        await new StudentDataSeeder(studentDao).SeedAsync();

        await supervisorSeeder.SeedAsync();

        Assert.NotEmpty(
            await database.Context
                .SupervisoryAssignments
                .ToListAsync());
    }
}