namespace PgrStudentManagement.Web.Modules.Students.Services;

public class OperationResult
{
    public bool Succeeded { get; init; }

    public string? ErrorMessage { get; init; }

    public static OperationResult Success()
    {
        return new OperationResult
        {
            Succeeded = true
        };
    }

    public static OperationResult Failure(string message)
    {
        return new OperationResult
        {
            Succeeded = false,
            ErrorMessage = message
        };
    }
}