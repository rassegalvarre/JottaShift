using System.Net;

namespace JottaShift.Core.HttpClientWrapper;

public record HttpPostResult<T> : BaseHttpResult
{
    public T? Content { get; init; }

    public HttpPostResult(HttpStatusCode statusCode) : base(statusCode) { }

    public HttpPostResult(HttpStatusCode statusCode, string errorMessage) : base(statusCode, errorMessage) { }

    public HttpPostResult(HttpStatusCode statusCode, T data) : base(statusCode)
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