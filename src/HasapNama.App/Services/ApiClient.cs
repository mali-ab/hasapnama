using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Maui.Storage;
using HasapNama.Shared.Models;

namespace HasapNama.App.Services;

/// <summary>
/// HTTP client for the shared HasapNama server (single backend for many apps).
/// </summary>
public class ApiClient(HttpClient http) : IDisposable
{
    static ApiClient()
    {
        // Accept any certificate so dev machines (self-signed / dev-proxy) work.
        System.Net.ServicePointManager.ServerCertificateValidationCallback = (_, _, _, _) => true;
    }

    private string? _token;

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(_token);

    /// <summary>
    /// Base address of the shared server. Emulators reach the host via 10.0.2.2;
    /// a real device needs the machine's LAN IP (set HasapNama:Api:BaseUrl in appsettings).
    /// </summary>
    public static string ResolveBaseUrl()
    {
        var configured = Preferences.Default.Get("api.baseUrl", string.Empty);
        if (!string.IsNullOrWhiteSpace(configured))
            return configured;

        return DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5080"
            : "http://localhost:5080";
    }

    public void SetToken(string? token)
    {
        _token = token;
        http.DefaultRequestHeaders.Authorization =
            string.IsNullOrWhiteSpace(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<HttpResponseMessage> SendError(HttpResponseMessage response)
    {
        var error = await ReadErrorAsync(response);
        throw new ApiException(error?.Message ?? $"Request failed ({response.StatusCode}).", response.StatusCode);
    }

    private static async Task<ErrorResponse?> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var body = await response.Content.ReadAsStringAsync();
            return string.IsNullOrWhiteSpace(body) ? null : System.Text.Json.JsonSerializer.Deserialize<ErrorResponse>(body);
        }
        catch
        {
            return null;
        }
    }

    // ------------------------------------------------------------------ auth
    public async Task<LoginResponse> LoginAsync(string username, string password)
    {
        var response = await http.PostAsJsonAsync("/api/auth/login", new LoginRequest { Username = username, Password = password });
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        if (!response.IsSuccessStatusCode || body is null)
            throw new ApiException(body?.Error?.Message ?? "Login failed.", response.StatusCode     response.StatusCode);
        SetToken(body.Token);
        return body;
    }

    public async Task RegisterAsync(string username, string password, string displayName)
    {
        var response = await http.PostAsJsonAsync("/api/auth/register", new RegisterRequest { Username = username, Password = password, DisplayName = displayName });
        if (!response.IsSuccessStatusCode)
            await SendError(response);
    }

    // ------------------------------------------------------------- customers
    public async Task<List<Customer>> GetCustomersAsync()
    {
        var response = await http.GetAsync("/api/customers/");
        if (!response.IsSuccessStatusCode)
            await SendError(response);
        return await response.Content.ReadFromJsonAsync<List<Customer>>() ?? new();
    }

    public async Task<Customer> CreateCustomerAsync(CustomerRequest payload)
    {
        var response = await http.PostAsJsonAsync("/api/customers/", payload);
        if (!response.IsSuccessStatusCode)
            await SendError(response);
        return await response.Content.ReadFromJsonAsync<Customer>() ?? throw new ApiException("No customer returned.", HttpStatusCode.InternalServerError);
    }

    public async Task<Customer> UpdateCustomerAsync(Guid id, CustomerRequest payload)
    {
        var response = await http.PutAsJsonAsync($"/api/customers/{id}", payload);
        if (!response.IsSuccessStatusCode)
            await SendError(response);
        return await response.Content.ReadFromJsonAsync<Customer>() ?? throw new ApiException("No customer returned.", HttpStatusCode.InternalServerError);
    }

    public async Task DeleteCustomerAsync(Guid id)
    {
        var response = await http.DeleteAsync($"/api/customers/{id}");
        if (!response.IsSuccessStatusCode)
            await SendError(response);
    }

    public void Dispose() => http.Dispose();
}

public class ApiException(string message, HttpStatusCode? status = null) : Exception(message)
{
    public HttpStatusCode? StatusCode { get; } = status;
}