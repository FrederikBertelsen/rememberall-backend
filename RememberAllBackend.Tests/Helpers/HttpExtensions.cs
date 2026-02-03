using System.Net;
using FluentAssertions;

namespace RememberAllBackend.Tests.Helpers;

public static class HttpResponseExtensions
{
    public static async Task<T> GetContentAsync<T>(this HttpResponseMessage response)
    {
        response.IsSuccessStatusCode.Should().BeTrue(
            because: $"response should be successful but was {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");

        var content = await response.Content.ReadFromJsonAsync<T>();
        content.Should().NotBeNull(because: "response content should deserialize successfully");

        return content!;
    }

    public static async Task<T> GetContentAsync<T>(this HttpResponseMessage response, HttpStatusCode expectedStatusCode)
    {
        response.StatusCode.Should().Be(expectedStatusCode,
            because: $"response status should be {expectedStatusCode} but was {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");

        var content = await response.Content.ReadFromJsonAsync<T>();
        content.Should().NotBeNull(because: "response content should deserialize successfully");

        return content!;
    }

    public static async Task<T?> TryGetContentAsync<T>(this HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
            return default;

        return await response.Content.ReadFromJsonAsync<T>();
    }

    public static async Task<string> GetErrorContentAsync(this HttpResponseMessage response)
    {
        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<T> GetContentOrThrowAsync<T>(this HttpResponseMessage response, string errorContext = "")
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Request failed{(string.IsNullOrEmpty(errorContext) ? "" : $" ({errorContext})")}: " +
                $"{response.StatusCode} - {errorContent}");
        }

        var content = await response.Content.ReadFromJsonAsync<T>();
        if (content == null)
        {
            throw new InvalidOperationException($"Failed to deserialize response content to {typeof(T).Name}");
        }

        return content;
    }

    public static HttpResponseMessage AssertStatusCode(this HttpResponseMessage response, HttpStatusCode expected)
    {
        response.StatusCode.Should().Be(expected, because: $"response status should be {expected}");
        return response;
    }

    public static HttpResponseMessage AssertSuccess(this HttpResponseMessage response)
    {
        response.IsSuccessStatusCode.Should().BeTrue(
            because: $"response should be successful but was {response.StatusCode}: {response.Content.ReadAsStringAsync().Result}");
        return response;
    }

    public static HttpResponseMessage AssertUnauthorized(this HttpResponseMessage response) =>
        response.AssertStatusCode(HttpStatusCode.Unauthorized);
    public static HttpResponseMessage AssertForbidden(this HttpResponseMessage response) =>
        response.AssertStatusCode(HttpStatusCode.Forbidden);
    public static HttpResponseMessage AssertNotFound(this HttpResponseMessage response) =>
        response.AssertStatusCode(HttpStatusCode.NotFound);
    public static HttpResponseMessage AssertBadRequest(this HttpResponseMessage response) =>
        response.AssertStatusCode(HttpStatusCode.BadRequest);
}

public static class HttpClientExtensions
{
    public static async Task<T> GetAsync<T>(this HttpClient userClient, string requestUri)
    {
        var response = await userClient.GetAsync(requestUri);
        return await response.GetContentAsync<T>();
    }

    public static async Task<T> PostAsJsonAsync<T>(this HttpClient userClient, string requestUri, object content)
    {
        var response = await userClient.PostAsJsonAsync(requestUri, content);
        return await response.GetContentAsync<T>();
    }

    public static async Task<T> PutAsJsonAsync<T>(this HttpClient userClient, string requestUri, object content)
    {
        var response = await userClient.PutAsJsonAsync(requestUri, content);
        return await response.GetContentAsync<T>();
    }

    public static async Task<T> PatchAsJsonAsync<T>(this HttpClient userClient, string requestUri, object content)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, requestUri)
        {
            Content = JsonContent.Create(content)
        };
        var response = await userClient.SendAsync(request);

        return await response.GetContentAsync<T>();
    }

    // Raw response methods that don't assert success
    public static async Task<HttpResponseMessage> GetResponseAsync(this HttpClient userClient, string requestUri)
    {
        return await userClient.GetAsync(requestUri);
    }

    public static async Task<HttpResponseMessage> PostAsJsonResponseAsync(this HttpClient userClient, string requestUri, object content)
    {
        return await userClient.PostAsJsonAsync(requestUri, content);
    }

    public static async Task<HttpResponseMessage> PutAsJsonResponseAsync(this HttpClient userClient, string requestUri, object content)
    {
        return await userClient.PutAsJsonAsync(requestUri, content);
    }

    public static async Task<HttpResponseMessage> PatchAsJsonResponseAsync(this HttpClient userClient, string requestUri, object content)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, requestUri)
        {
            Content = JsonContent.Create(content)
        };
        return await userClient.SendAsync(request);
    }

    public static async Task<HttpResponseMessage> DeleteResponseAsync(this HttpClient userClient, string requestUri)
    {
        return await userClient.DeleteAsync(requestUri);
    }
}
