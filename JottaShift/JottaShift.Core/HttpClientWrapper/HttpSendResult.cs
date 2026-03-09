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
}