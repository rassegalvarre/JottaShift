namespace JottaShift.Core.HttpClientWrapper;

public interface IHttpClientWrapper
{
    Uri? BaseAddress { get; set; }
    Task<HttpSendResult<T>> SendAsync<T>(HttpRequestMessage request);
    Task<HttpGetResult<T>> GetAsync<T>(string uri);
}
