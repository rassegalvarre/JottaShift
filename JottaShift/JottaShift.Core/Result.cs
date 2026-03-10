namespace JottaShift.Core;

public record Result
{
    public required bool Succeeded { get; init; }
    public string? ErrorMessage { get; init; }

    public static Result Success() => new()
    { 
        Succeeded = true
    };

    public static Result Failure(string errorMessage) => new()
    {
        Succeeded = false,
        ErrorMessage = errorMessage
    };
}

public record Result<T> : Result
{
    public T? Value { get; init; }

    public static Result<T> Success(T value) => new()
    { 
        Succeeded = true,
        Value = value
    };

    public static new Result<T> Failure(string errorMessage) => new()
    {
        Succeeded = false,
        ErrorMessage = errorMessage
    };
}