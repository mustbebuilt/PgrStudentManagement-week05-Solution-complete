namespace PgrStudentManagement.Web.Modules.Students.Services;

public class OperationResult<T>
{
    public bool Succeeded { get; init; }

    public T? Value { get; init; }

    public string? ErrorMessage { get; init; }

    public static OperationResult<T> Success(T value)
    {
        return new OperationResult<T>
        {
            Succeeded = true,
            Value = value
        };
    }

    public static OperationResult<T> Failure(string message)
    {
        return new OperationResult<T>
        {
            Succeeded = false,
            ErrorMessage = message
        };
    }
}