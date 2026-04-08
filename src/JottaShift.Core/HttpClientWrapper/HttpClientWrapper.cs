using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace JottaShift.Core.HttpClientWrapper;

public class HttpClientWrapper(HttpClient _http, ILogger<HttpClientWrapper> _logger) : IHttpClientWrapper
{
    public Uri? BaseAddress { get; set; }
    public HttpClient HttpClient=> _http;

    public async Task<HttpSendResult<T>> SendAsync<T>(HttpRequestMessage request)
    {
        HttpResponseMessage response;
        T? data;

        try
        {
            response = await _http.SendAsync(request);

            if (response.StatusCode is not System.Net.HttpStatusCode.OK)
            {
                _logger.LogError("Request resulted in status {HttpStatus}", response.StatusCode);

                return new HttpSendResult<T>(response.StatusCode);
            }

            if (typeof(T) == typeof(string))
            {
                var responseContentString = await response.Content.ReadAsStringAsync();
                data = (T)(object)responseContentString;
            }
            else
            {
                var responseContentStream = await response.Content.ReadAsStreamAsync();
                var responseContentString = await response.Content.ReadAsStringAsync();

                if (responseContentStream.Length == 0)
                {
                    _logger.LogInformation("Repsonse did not contain any data");
                    return new HttpSendResult<T>(response.StatusCode);
                }
                
                data = JsonSerializer.Deserialize<T?>(responseContentString);
            }

            if (data == null)
            {
                _logger.LogError("Could not deserialize response to the expected schema");
                return new HttpSendResult<T>(response.StatusCode, "Could not deserialize response to the expected schema");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while sending the request");
            return new HttpSendResult<T>(System.Net.HttpStatusCode.InternalServerError, ex.Message);
        }

        return new HttpSendResult<T>(response.StatusCode, data);
    }

    public async Task<HttpGetResult<T>> GetAsync<T>(string requestUri)
    {
        HttpResponseMessage response;
        T? data;

        if (BaseAddress != null)
        {
            requestUri = new Uri(BaseAddress, requestUri).ToString();
        }

        try
        {
            response = await _http.GetAsync(requestUri);

            if (response.StatusCode is not System.Net.HttpStatusCode.OK)
            {
                _logger.LogError("Request resulted in status {HttpStatus}", response.StatusCode);

                return new HttpGetResult<T>(response.StatusCode);
            }
            var responseContentStream = await response.Content.ReadAsStreamAsync();
            data = await JsonSerializer.DeserializeAsync<T?>(responseContentStream);

            if (data == null)
            {
                _logger.LogError("Could not deserialize response to the expected schema");
                return new HttpGetResult<T>(response.StatusCode, "Could not deserialize response to the expected schema");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while sending the request");
            return new HttpGetResult<T>(System.Net.HttpStatusCode.InternalServerError, ex.Message);
        }

        return new HttpGetResult<T>(response.StatusCode, data);
    }

    public async Task<HttpPostResult<TResponse>> PostAsync<TRequest, TResponse>(string requestUri, TRequest request)
    {
        HttpResponseMessage response;
        TResponse? data;

        if (BaseAddress != null)
        {
            requestUri = new Uri(BaseAddress, requestUri).ToString();
        }

        try
        {
            string jsonContent = JsonSerializer.Serialize(request);
            response = await _http.PostAsJsonAsync(requestUri, jsonContent);

            if (response.StatusCode is not System.Net.HttpStatusCode.OK)
            {
                _logger.LogError("Request resulted in status {HttpStatus}", response.StatusCode);

                return new HttpPostResult<TResponse>(response.StatusCode);
            }
            var responseContentStream = await response.Content.ReadAsStreamAsync();
            data = await JsonSerializer.DeserializeAsync<TResponse?>(responseContentStream);

            if (data == null)
            {
                _logger.LogError("Could not deserialize response to the expected schema");
                return new HttpPostResult<TResponse>(response.StatusCode, "Could not deserialize response to the expected schema");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while sending the request");
            return new HttpPostResult<TResponse>(System.Net.HttpStatusCode.InternalServerError, ex.Message);
        }

        return new HttpPostResult<TResponse>(response.StatusCode, data);
    }
}