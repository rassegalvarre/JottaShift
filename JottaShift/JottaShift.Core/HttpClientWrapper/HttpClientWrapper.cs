using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace JottaShift.Core.HttpClientWrapper;

public class HttpClientWrapper(HttpClient _http, ILogger<HttpClientWrapper> _logger) : IHttpClientWrapper
{
    public Uri? BaseAddress { get; set; }

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

            var responseContentStream = await response.Content.ReadAsStreamAsync();

            if (responseContentStream.Length == 0)
            {
                _logger.LogInformation("Repsonse did not contain any data");
                return new HttpSendResult<T>(response.StatusCode);
            }

            data = await JsonSerializer.DeserializeAsync<T?>(responseContentStream);

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

    public async Task<HttpGetResult<T>> GetAsync<T>(Uri requestUri)
    {
        HttpResponseMessage response;
        T? data;

        if (BaseAddress != null)
        {
            requestUri = new Uri(BaseAddress, requestUri);
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
}