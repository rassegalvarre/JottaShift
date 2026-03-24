using System.Net;

namespace JottaShift.Core.HttpClientWrapper;

public abstract record BaseHttpResult
{
    public HttpStatusCode StatusCode { get; init; }
    public string? ErrorMessage { get; init; }

    public bool Success => StatusCode == HttpStatusCode.OK;

    public BaseHttpResult(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
    }

    public BaseHttpResult(HttpStatusCode statusCode, string errorMessage)
    {
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
    }
}