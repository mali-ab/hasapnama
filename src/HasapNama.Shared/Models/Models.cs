namespace HasapNama.Shared.Models;

/// <summary>
/// A customer record in the hasapnama CRM.
/// Every customer belongs to the user (OwnerId) that created it, so several
/// apps / users can share one server and one database.
/// </summary>
public class Customer
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OwnerId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Company { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Payload used to create or update a customer.</summary>
public class CustomerRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
}

/// <summary>Login request / response DTOs.</summary>
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public ErrorResponse? Error { get; set; }
}

public class RegisterRequest : LoginRequest
{
    public string DisplayName { get; set; } = string.Empty;
}

public class RegisterResponse
{
    public bool Succeeded { get; set; }
    public string? Message { get; set; }
}

/// <summary>Standard error shape returned by the API.</summary>
public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
}

/// <summary>User account record stored on the shared server.</summary>
public class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}