namespace JottaShift.Core;

public record Result<T>
{
    public bool Succeeded { get; init; }
    public T? Value { get; init; }
    public string? ErrorMessage { get; init; }

    private Result(bool success, T value)
    {
        Succeeded = success;
        Value = value;
    }

    private Result(bool success, string errorMessage)
    {
        Succeeded = success;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Success(T value) => new(true, value);

    public static Result<T> Failure(string errorMessage) => new(false, errorMessage);
}
