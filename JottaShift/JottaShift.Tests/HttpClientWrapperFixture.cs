using JottaShift.Core.HttpClientWrapper;
using Microsoft.Extensions.Logging;
using Moq;

namespace JottaShift.Tests;

public class HttpClientWrapperFixture
{
    private readonly HttpClient _httpClient;

    public HttpClientWrapperFixture()
    {
        _httpClient = new HttpClient(); 
    }

    public HttpClientWrapper CreateHttpClientWrapper()
    {
        return new HttpClientWrapper(
            _httpClient,
            new Mock<ILogger<HttpClientWrapper>>().Object);
    }
}
