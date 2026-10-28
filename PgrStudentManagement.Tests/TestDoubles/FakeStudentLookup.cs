using PgrStudentManagement.Web.Modules.Students.Contracts;

namespace PgrStudentManagement.Tests.TestDoubles;

public class FakeStudentLookup : IStudentLookup
{
    private readonly Dictionary<string, StudentSummary> _students =
        new(StringComparer.OrdinalIgnoreCase);

    public void AddStudent(string studentNumber, string displayName)
    {
        _students[studentNumber] = 
            new StudentSummary(studentNumber, displayName);
    }

    public Task<StudentSummary?> GetStudentSummaryAsync(
            string studentNumber,
            CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _students.TryGetValue(studentNumber, out var student);

        return Task.FromResult(student);
    }
}
