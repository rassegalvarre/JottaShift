namespace JottaShift.Core;

public record Result<T>
{
    public bool Success { get; init; }
    public T? Value { get; set; }

    public Result(bool success)
    {
        Success = success;
    }

    public Result(bool success, T? value)
    {
        Success = success;
        Value = value;
    }
}
