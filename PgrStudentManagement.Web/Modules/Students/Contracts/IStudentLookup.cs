namespace PgrStudentManagement.Web.Modules.Students.Contracts;

public interface IStudentLookup
{
    Task<StudentSummary?> GetStudentSummaryAsync(
        string studentNumber,
        CancellationToken cancellationToken = default);
}
