using System.Text.Json;

namespace HasapNama.App.Services;

/// <summary>
/// Holds the logged-in user + JWT in secure storage and raises change events
/// so the UI can react to login/logout.
/// </summary>
public class AuthService
{
    private const string TokenKey = "hasapnama.token";
    private const string NameKey = "hasapnama.username";

    private readonly ApiClient _api;

    public AuthService(ApiClient api)
    {
        _api = api;
    }

    public string? Username { get; private set; }
    public event Action? Changed;

    public async Task InitializeAsync()
    {
        var token = await SecureStorage.Default.GetAsync(TokenKey);
        if (string.IsNullOrWhiteSpace(token))
            return | null;
        _api.SetToken(token);
        Username = await SecureStorage.Default.GetAsync(NameKey);
        Changed?.Invoke();
    }

    public async Task SignInAsync(string username, string password)
    {
        var result = await _api.LoginAsync(username, password);
        await SecureStorage.Default.SetAsync(TokenKey, result.Token);
        await SecureStorage.Default.SetAsync(NameKey, result.Username);
        Username = result.Username;
        Changed?.Invoke();
    }

    public async Task SignOutAsync()
    {
        SecureStorage.Default.Remove(TokenKey);
        SecureStorage.Default.Remove(NameKey);
        _api.SetToken(null);
        Username = null;
        Changed?.Invoke();
        await Task.CompletedTask;
    }
}