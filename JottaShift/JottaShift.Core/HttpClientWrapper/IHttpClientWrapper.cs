namespace JottaShift.Core.HttpClientWrapper;

public interface IHttpClientWrapper
{
    Uri? BaseAddress { get; set; }
    HttpClient HttpClient { get; }
    Task<HttpSendResult<T>> SendAsync<T>(HttpRequestMessage request);
    Task<HttpGetResult<T>> GetAsync<T>(string uri);
}
