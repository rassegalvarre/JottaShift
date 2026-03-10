using System.Net;

namespace JottaShift.Core.HttpClientWrapper;

public record HttpGetResult<T> : BaseHttpResult
{
    public T? Content { get; init; }

    public HttpGetResult(HttpStatusCode statusCode) : base(statusCode) { }

    public HttpGetResult(HttpStatusCode statusCode, string errorMessage) : base(statusCode, errorMessage) { }

    public HttpGetResult(HttpStatusCode statusCode, T data) : base(statusCode)
    {
        StatusCode = statusCode;
        Content = data;
    }

    public Result<T> ToResult()
    {
        if (Success && Content != null)
        {
            return Result<T>.Success(Content);
        }
        else
        {
            return Result<T>.Failure(ErrorMessage ?? $"HTTP request failed with status code {StatusCode}");
        }
    }
}