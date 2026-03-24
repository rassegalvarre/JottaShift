using System.Net;

namespace JottaShift.Core.HttpClientWrapper;

public record HttpSendResult<T> : BaseHttpResult
{
    public T? Content { get; init; }

    public HttpSendResult(HttpStatusCode statusCode) : base(statusCode) { }

    public HttpSendResult(HttpStatusCode statusCode, string errorMessage) : base(statusCode, errorMessage) { }

    public HttpSendResult(HttpStatusCode statusCode, T data) : base(statusCode)
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