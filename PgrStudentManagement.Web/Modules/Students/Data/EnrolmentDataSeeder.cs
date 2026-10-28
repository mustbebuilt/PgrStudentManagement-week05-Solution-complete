using PgrStudentManagement.Web.Modules.Students.Data.Daos;
using PgrStudentManagement.Web.Modules.Students.Models;

namespace PgrStudentManagement.Web.Modules.Students.Data;

public sealed class EnrolmentDataSeeder
{
    private readonly EnrolmentDao _enrolmentDao;

    public EnrolmentDataSeeder(
        EnrolmentDao enrolmentDao)
    {
        _enrolmentDao = enrolmentDao;
    }

    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        await AddEnrolmentIfMissingAsync(
            new Enrolment
            {
                EnrolmentId = 1,
                StudentNumber = "S100001",
                ProgrammeCode = "PHD-COMP",
                StartDate =
                    new DateTime(2024, 10, 1),
                ExpectedEndDate =
                    new DateTime(2027, 9, 30),
                ModeOfStudy = "Full-time",
                Status = EnrolmentStatus.Active
            },
            cancellationToken);

        await AddEnrolmentIfMissingAsync(
            new Enrolment
            {
                EnrolmentId = 2,
                StudentNumber = "S100002",
                ProgrammeCode = "MPHIL-COMP",
                StartDate =
                    new DateTime(2022, 10, 1),
                ExpectedEndDate =
                    new DateTime(2024, 9, 30),
                ModeOfStudy = "Full-time",
                Status = EnrolmentStatus.Completed
            },
            cancellationToken);

        await AddEnrolmentIfMissingAsync(
            new Enrolment
            {
                EnrolmentId = 3,
                StudentNumber = "S100002",
                ProgrammeCode = "PHD-COMP",
                StartDate =
                    new DateTime(2024, 10, 1),
                ExpectedEndDate =
                    new DateTime(2030, 9, 30),
                ModeOfStudy = "Part-time",
                Status = EnrolmentStatus.Active
            },
            cancellationToken);

        await AddEnrolmentIfMissingAsync(
            new Enrolment
            {
                EnrolmentId = 4,
                StudentNumber = "S100003",
                ProgrammeCode = "PHD-DESIGN",
                StartDate =
                    new DateTime(2025, 10, 1),
                ExpectedEndDate = null,
                ModeOfStudy = "Full-time",
                Status = EnrolmentStatus.Suspended
            },
            cancellationToken);
    }

    private async Task AddEnrolmentIfMissingAsync(
        Enrolment enrolment,
        CancellationToken cancellationToken)
    {
        var enrolmentExists =
            await _enrolmentDao.EnrolmentExistsAsync(
                enrolment.EnrolmentId,
                cancellationToken);

        if (enrolmentExists)
        {
            return;
        }

        await _enrolmentDao.AddEnrolmentAsync(
            enrolment,
            cancellationToken);
    }
}