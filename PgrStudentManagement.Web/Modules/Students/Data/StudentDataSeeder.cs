using PgrStudentManagement.Web.Modules.Students.Data.Daos;
using PgrStudentManagement.Web.Modules.Students.Models;

namespace PgrStudentManagement.Web.Modules.Students.Data;

public sealed class StudentDataSeeder
{
    private readonly StudentDao _studentDao;

    public StudentDataSeeder(StudentDao studentDao)
    {
        _studentDao = studentDao;
    }

    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        await AddStudentIfMissingAsync(
            new Student
            {
                StudentNumber = "S100001",
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john.doe@example.test",
                Course = "PhD Computing",
                ModeOfStudy = "Full-time",
                StartDate =
                    new DateTime(2025, 10, 1),
                Status = Status.Researching,
                ThesisTitle =
                    "Explainable Models for "
                    + "Clinical Decision Support",
                OriginalExpectedSubmissionDate =
                    new DateTime(2028, 9, 30),
                ExpectedSubmissionDate =
                    new DateTime(2028, 9, 30)
            },
            cancellationToken);

        await AddStudentIfMissingAsync(
            new Student
            {
                StudentNumber = "S100002",
                FirstName = "Jane",
                LastName = "Smith",
                EmailAddress = "jane.smith@example.test",
                Course = "Professional Doctorate",
                ModeOfStudy = "Part-time",
                StartDate =
                    new DateTime(2024, 10, 1),
                Status = Status.WritingUp,
                ThesisTitle = null,
                OriginalExpectedSubmissionDate =
                    new DateTime(2030, 9, 30),
                ExpectedSubmissionDate =
                    new DateTime(2030, 9, 30)
            },
            cancellationToken);

        await AddStudentIfMissingAsync(
            new Student
            {
                StudentNumber = "S100003",
                FirstName = "Casey",
                LastName = "Wilson",
                EmailAddress = "casey.wilson@example.test",
                Course = "PhD Design",
                ModeOfStudy = "Full-time",
                StartDate =
                    new DateTime(2026, 10, 1),
                Status = Status.Researching,
                ThesisTitle = null,
                OriginalExpectedSubmissionDate =
                    new DateTime(2029, 9, 30),
                ExpectedSubmissionDate =
                    new DateTime(2029, 9, 30)
            },
            cancellationToken);

        await AddStudentIfMissingAsync(
            new Student
            {
                StudentNumber = "S100004",
                FirstName = "Morgan",
                LastName = "Lee",
                EmailAddress = "morgan.lee@example.test",
                Course = "MPhil Computing",
                ModeOfStudy = "Part-time",
                StartDate =
                    new DateTime(2025, 10, 1),
                Status = Status.Suspended,
                ThesisTitle =
                    "Responsible Use of Generative AI "
                    + "in Higher Education",
                OriginalExpectedSubmissionDate =
                    new DateTime(2028, 9, 30),
                ExpectedSubmissionDate =
                    new DateTime(2029, 3, 31)
            },
            cancellationToken);
    }

    private async Task AddStudentIfMissingAsync(
        Student student,
        CancellationToken cancellationToken)
    {
        var studentExists =
            await _studentDao.StudentNumberExistsAsync(
                student.StudentNumber,
                cancellationToken);

        if (studentExists)
        {
            return;
        }

        await _studentDao.AddStudentAsync(
            student,
            cancellationToken);
    }
}