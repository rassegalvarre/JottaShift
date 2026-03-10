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
}