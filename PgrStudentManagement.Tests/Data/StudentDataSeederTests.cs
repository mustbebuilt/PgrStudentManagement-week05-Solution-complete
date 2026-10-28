using Microsoft.EntityFrameworkCore;

using PgrStudentManagement.Tests.Fixtures;
using PgrStudentManagement.Web.Modules.Students.Data;
using PgrStudentManagement.Web.Modules.Students.Data.Daos;

using Xunit;

namespace PgrStudentManagement.Tests.Data;

public sealed class StudentDataSeederTests
{
    [Fact]
    public async Task SeedAsync_AddsExpectedStudents()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();

        var dao = new StudentDao(database.Context);

        var seeder = new StudentDataSeeder(dao);

        await seeder.SeedAsync();

        var students =
            await database.Context.Students
                .AsNoTracking()
                .OrderBy(student =>
                    student.StudentNumber)
                .ToListAsync();

        Assert.Equal(4, students.Count);

        var studentNumbers = students
            .Select(student => student.StudentNumber)
            .ToList();

        Assert.Equal(
            [
                "S100001",
                "S100002",
                "S100003",
                "S100004"
            ],
            studentNumbers);

        var firstStudent = students[0];

        Assert.Equal(
            "John",
            firstStudent.FirstName);

        Assert.Equal(
            "Doe",
            firstStudent.LastName);

        Assert.Equal(
            "john.doe@example.test",
            firstStudent.EmailAddress);
    }

    [Fact]
    public async Task SeedAsync_WhenCalledTwice_DoesNotDuplicateStudents()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();

        var dao = new StudentDao(database.Context);

        var seeder = new StudentDataSeeder(dao);

        await seeder.SeedAsync();
        await seeder.SeedAsync();

        int studentCount = await database.Context.Students.CountAsync();

        Assert.Equal(4, studentCount);
    }
}