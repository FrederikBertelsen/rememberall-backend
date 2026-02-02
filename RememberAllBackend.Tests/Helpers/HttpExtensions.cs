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
    public static async Task<T> GetAsync<T>(this HttpClient client, string requestUri)
    {
        var response = await client.GetAsync(requestUri);
        return await response.GetContentAsync<T>();
    }

    public static async Task<T> PostAsJsonAsync<T>(this HttpClient client, string requestUri, object content)
    {
        var response = await client.PostAsJsonAsync(requestUri, content);
        return await response.GetContentAsync<T>();
    }

    public static async Task<T> PutAsJsonAsync<T>(this HttpClient client, string requestUri, object content)
    {
        var response = await client.PutAsJsonAsync(requestUri, content);
        return await response.GetContentAsync<T>();
    }

    public static async Task<T> PatchAsJsonAsync<T>(this HttpClient client, string requestUri, object content)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, requestUri)
        {
            Content = JsonContent.Create(content)
        };
        var response = await client.SendAsync(request);
        
        return await response.GetContentAsync<T>();
    }
}
