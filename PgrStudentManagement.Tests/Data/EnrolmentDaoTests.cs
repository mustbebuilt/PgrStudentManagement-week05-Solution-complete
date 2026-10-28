using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using PgrStudentManagement.Web.Modules.Students.Data;
using PgrStudentManagement.Web.Modules.Students.Data.Daos;
using PgrStudentManagement.Web.Modules.Students.Models;

using Xunit;

namespace PgrStudentManagement.Tests.Data;

public sealed class EnrolmentDaoTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly StudentManagementDbContext _context;
    private readonly EnrolmentDao _enrolmentDao;
    private readonly StudentDataSeeder _studentSeeder;
    private readonly EnrolmentDataSeeder _enrolmentSeeder;

    public EnrolmentDaoTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");

        _connection.Open();

        var options =
            new DbContextOptionsBuilder<StudentManagementDbContext>()
                .UseSqlite(_connection)
                .Options;

        _context = new StudentManagementDbContext(options);

        _context.Database.EnsureCreated();

        _enrolmentDao = new EnrolmentDao(_context);

        _studentSeeder = new StudentDataSeeder(new StudentDao(_context));

        _enrolmentSeeder = new EnrolmentDataSeeder(_enrolmentDao);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task GetEnrolmentsForStudentAsync_ReturnsChronologicalEnrolmentsForStudent()
    {
        await SeedTestDataAsync();

        var enrolments =
            await _enrolmentDao.GetEnrolmentsForStudentAsync("S100002");

        Assert.Equal(2, enrolments.Count);

        Assert.Equal(
            "MPHIL-COMP",
            enrolments[0].ProgrammeCode);

        Assert.Equal(
            EnrolmentStatus.Completed,
            enrolments[0].Status);

        Assert.Equal(
            "PHD-COMP",
            enrolments[1].ProgrammeCode);

        Assert.Equal(
            EnrolmentStatus.Active,
            enrolments[1].Status);

        Assert.True(
            enrolments[0].StartDate
                < enrolments[1].StartDate);
    }

    [Fact]
    public async Task GetEnrolmentsForStudentAsync_ReturnsEmptyWhenStudentHasNoEnrolments()
    {
        await SeedTestDataAsync();

        var enrolments =
            await _enrolmentDao.GetEnrolmentsForStudentAsync("S100004");

        Assert.Empty(enrolments);
    }

    [Fact]
    public async Task GetEnrolmentsForStudentAsync_ReturnsEmptyWhenStudentDoesNotExist()
    {
        await SeedTestDataAsync();

        var enrolments =
            await _enrolmentDao.GetEnrolmentsForStudentAsync("UNKNOWN");

        Assert.Empty(enrolments);
    }

    [Fact]
    public async Task GetEnrolmentsForStudentAsync_HandlesNullExpectedEndDate()
    {
        await SeedTestDataAsync();

        var enrolments =
            await _enrolmentDao.GetEnrolmentsForStudentAsync("S100003");

        var enrolment =
            Assert.Single(enrolments);

        Assert.Null(
            enrolment.ExpectedEndDate);

        Assert.Equal(
            EnrolmentStatus.Suspended,
            enrolment.Status);
    }

    [Fact]
    public async Task EnrolmentExistsAsync_WhenEnrolmentExists_ReturnsTrue()
    {
        await SeedTestDataAsync();

        var exists =
            await _enrolmentDao.EnrolmentExistsAsync(1);

        Assert.True(exists);
    }

    [Fact]
    public async Task EnrolmentExistsAsync_WhenEnrolmentDoesNotExist_ReturnsFalse()
    {
        await SeedTestDataAsync();

        var exists =
            await _enrolmentDao.EnrolmentExistsAsync(999);

        Assert.False(exists);
    }

    [Fact]
    public async Task AddEnrolmentAsync_SavesEnrolment()
    {
        await _studentSeeder.SeedAsync();

        var enrolment =
            new Enrolment
            {
                EnrolmentId = 10,
                StudentNumber = "S100004",
                ProgrammeCode = "PHD-COMP",
                StartDate =
                    new DateTime(2026, 10, 1),
                ExpectedEndDate =
                    new DateTime(2029, 9, 30),
                ModeOfStudy = "Full-time",
                Status = EnrolmentStatus.Active
            };

        await _enrolmentDao.AddEnrolmentAsync(enrolment);

        _context.ChangeTracker.Clear();

        var saved =
            await _context.Enrolments
                .AsNoTracking()
                .SingleAsync(existing =>
                    existing.EnrolmentId == 10);

        Assert.Equal(
            "S100004",
            saved.StudentNumber);

        Assert.Equal(
            "PHD-COMP",
            saved.ProgrammeCode);

        Assert.Equal(
            EnrolmentStatus.Active,
            saved.Status);
    }

    [Fact]
    public async Task AddEnrolmentAsync_WhenStudentDoesNotExist_ThrowsDbUpdateException()
    {
        var enrolment =
            new Enrolment
            {
                EnrolmentId = 11,
                StudentNumber = "UNKNOWN",
                ProgrammeCode = "PHD-COMP",
                StartDate =
                    new DateTime(2026, 10, 1),
                ExpectedEndDate =
                    new DateTime(2029, 9, 30),
                ModeOfStudy = "Full-time",
                Status = EnrolmentStatus.Active
            };

        await Assert.ThrowsAsync<DbUpdateException>(
            () => _enrolmentDao.AddEnrolmentAsync(enrolment));
    }

    private async Task SeedTestDataAsync()
    {
        await _studentSeeder.SeedAsync();
        await _enrolmentSeeder.SeedAsync();
    }
}