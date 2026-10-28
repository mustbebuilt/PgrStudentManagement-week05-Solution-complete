using PgrStudentManagement.Web.Modules.Students.Contracts;
using PgrStudentManagement.Web.Modules.Students.Data.Daos;
using PgrStudentManagement.Web.Modules.Students.Models;

namespace PgrStudentManagement.Web.Modules.Students.Services;

public class StudentService : IStudentLookup
{
    private readonly StudentDao _studentDao;
    private readonly EnrolmentDao _enrolmentDao;

    public StudentService(
        StudentDao studentDao,
        EnrolmentDao enrolmentDao)
    {
        _studentDao = studentDao;
        _enrolmentDao = enrolmentDao;
    }

    public Task<IReadOnlyList<Enrolment>>
        GetEnrolmentsForStudentAsync(
            string studentNumber,
            CancellationToken cancellationToken = default)
    {
        return _enrolmentDao.GetEnrolmentsForStudentAsync(
            studentNumber,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Student>> GetStudentsAsync(
        CancellationToken cancellationToken = default)
    {
        var students = await _studentDao.GetStudentsAsync(
            cancellationToken);

        return students
            .OrderBy(student => student.LastName)
            .ThenBy(student => student.FirstName)
            .ThenBy(student => student.StudentNumber)
            .ToList();
    }

    public Task<Student?> GetStudentAsync(
        string studentNumber,
        CancellationToken cancellationToken = default)
    {
        return _studentDao.GetStudentAsync(
            studentNumber,
            cancellationToken);
    }

    public async Task<OperationResult<Student>> CreateStudentAsync(
        Student student,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(student);

        var studentNumberExists =
            await _studentDao.StudentNumberExistsAsync(
                student.StudentNumber,
                cancellationToken);

        if (studentNumberExists)
        {
            return OperationResult<Student>.Failure(
                "A student with this student number already exists");
        }

        await _studentDao.AddStudentAsync(
            student,
            cancellationToken);

        return OperationResult<Student>.Success(student);
    }

    public async Task<OperationResult> UpdateStatusAsync(
        string studentNumber,
        Status status,
        CancellationToken cancellationToken = default)
    {
        var student = await _studentDao.GetStudentAsync(
            studentNumber,
            cancellationToken);

        if (student is null)
        {
            return OperationResult.Failure(
                "Student not found");
        }

        student.Status = status;

        var updated = await _studentDao.UpdateStudentAsync(
            student,
            cancellationToken);

        if (!updated)
        {
            return OperationResult.Failure(
                "Student not found");
        }

        return OperationResult.Success();
    }

    public async Task<OperationResult>
        UpdateExpectedSubmissionDateAsync(
            string studentNumber,
            DateTime? expectedSubmissionDate,
            CancellationToken cancellationToken = default)
    {
        var student = await _studentDao.GetStudentAsync(
            studentNumber,
            cancellationToken);

        if (student is null)
        {
            return OperationResult.Failure(
                "Student not found");
        }

        if (expectedSubmissionDate is null)
        {
            return OperationResult.Failure(
                "Invalid expected submission date");
        }

        student.ExpectedSubmissionDate =
            expectedSubmissionDate;

        var updated = await _studentDao.UpdateStudentAsync(
            student,
            cancellationToken);

        if (!updated)
        {
            return OperationResult.Failure(
                "Student not found");
        }

        return OperationResult.Success();
    }

    public async Task<OperationResult> RecordSubmissionAsync(
        string studentNumber,
        DateTime? actualSubmissionDate,
        CancellationToken cancellationToken = default)
    {
        var student = await _studentDao.GetStudentAsync(
            studentNumber,
            cancellationToken);

        if (student is null)
        {
            return OperationResult.Failure(
                "Student not found");
        }

        if (actualSubmissionDate is null)
        {
            return OperationResult.Failure(
                "Invalid submission date");
        }

        student.ActualSubmissionDate =
            actualSubmissionDate;

        student.Status = Status.Submitted;

        var updated = await _studentDao.UpdateStudentAsync(
            student,
            cancellationToken);

        if (!updated)
        {
            return OperationResult.Failure(
                "Student not found");
        }

        return OperationResult.Success();
    }

    // Implementation of the IStudentLookup contract.
    public async Task<StudentSummary?> GetStudentSummaryAsync(
        string studentNumber,
        CancellationToken cancellationToken = default)
    {
        var student = await _studentDao.GetStudentAsync(
            studentNumber,
            cancellationToken);

        if (student is null)
        {
            return null;
        }

        return new StudentSummary(
            student.StudentNumber,
            student.FullName);
    }
}