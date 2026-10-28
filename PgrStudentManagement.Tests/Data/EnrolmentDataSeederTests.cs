using Microsoft.EntityFrameworkCore;

using PgrStudentManagement.Tests.Fixtures;

using PgrStudentManagement.Web.Modules.Students.Data;
using PgrStudentManagement.Web.Modules.Students.Data.Daos;
using PgrStudentManagement.Web.Modules.Students.Models;

using Xunit;

namespace PgrStudentManagement.Tests.Data;

public sealed class EnrolmentDataSeederTests
{
    [Fact]
    public async Task SeedAsync_AddsExpectedEnrolments()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();

        var studentDao = new StudentDao(database.Context);

        var studentSeeder = new StudentDataSeeder(studentDao);

        await studentSeeder.SeedAsync();

        var enrolmentDao = new EnrolmentDao(database.Context);

        var enrolmentSeeder = new EnrolmentDataSeeder(enrolmentDao);

        await enrolmentSeeder.SeedAsync();

        var enrolments =
            await database.Context.Enrolments
                .AsNoTracking()
                .OrderBy(enrolment =>
                    enrolment.EnrolmentId)
                .ToListAsync();

        Assert.Equal(4, enrolments.Count);

        Assert.Equal(
            [1, 2, 3, 4],
            enrolments
                .Select(enrolment => enrolment.EnrolmentId)
                .ToList());

        Assert.Equal(
            2,
            enrolments.Count(enrolment =>
                enrolment.StudentNumber == "S100002"));

        var firstEnrolment = enrolments[0];

        Assert.Equal(
            "S100001",
            firstEnrolment.StudentNumber);

        Assert.Equal(
            "PHD-COMP",
            firstEnrolment.ProgrammeCode);

        Assert.Equal(
            EnrolmentStatus.Active,
            firstEnrolment.Status);
    }

    [Fact]
    public async Task SeedAsync_WhenCalledTwice_DoesNotDuplicateEnrolments()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();

        var studentSeeder =
            new StudentDataSeeder(new StudentDao(database.Context));

        await studentSeeder.SeedAsync();

        var enrolmentSeeder =
            new EnrolmentDataSeeder(new EnrolmentDao(database.Context));

        await enrolmentSeeder.SeedAsync();
        await enrolmentSeeder.SeedAsync();

        int enrolmentCount = await database.Context.Enrolments.CountAsync();

        Assert.Equal(4, enrolmentCount);
    }

    [Fact]
    public async Task SeedAsync_AddsExpectedSpecialCaseData()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();

        await new StudentDataSeeder(new StudentDao(database.Context))
            .SeedAsync();

        await new EnrolmentDataSeeder(new EnrolmentDao(database.Context))
            .SeedAsync();

        var enrolments =
            await database.Context.Enrolments
                .AsNoTracking()
                .OrderBy(enrolment =>
                    enrolment.EnrolmentId)
                .ToListAsync();

        var studentTwoEnrolments =
            enrolments
                .Where(enrolment =>
                    enrolment.StudentNumber
                        == "S100002")
                .ToList();

        Assert.Equal(
            2,
            studentTwoEnrolments.Count);

        var suspendedEnrolment =
            Assert.Single(
                enrolments,
                enrolment =>
                    enrolment.StudentNumber
                        == "S100003");

        Assert.Null(
            suspendedEnrolment.ExpectedEndDate);

        Assert.Equal(
            EnrolmentStatus.Suspended,
            suspendedEnrolment.Status);

        Assert.DoesNotContain(
            enrolments,
            enrolment =>
                enrolment.StudentNumber
                    == "S100004");
    }
}