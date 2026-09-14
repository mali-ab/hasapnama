using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HasapNama.Server.Data;
using HasapNama.Server.Services;
using HasapNama.Shared.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var jwt = builder.Configuration.GetSection("Jwt");
var jwtKey = jwt["Key"] ?? builder.Configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Missing Jwt:Key in configuration.");
var issuer = jwt["Issuer"] ?? "hasapnama";
var audience = jwt["Audience"] ?? "hasapnama-clients";

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// ---------------------------------------------------------------------------
// Auth
// ---------------------------------------------------------------------------
app.MapPost("/api/auth/register", async (AppDbContext db, RegisterRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest(new ErrorResponse { Message = "Username and password are required." });
    if (req.Password.Length < 6)
        return Results.BadRequest(new ErrorResponse { Message = "Password must be at least 6 characters." });

    var username = req.Username.Trim();
    if (await db.Users.AnyAsync(u => u.Username == username))
        return Results.Conflict(new ErrorResponse { Message = "That username is already taken." });

    var user = new AppUser
    {
        Username = username,
        DisplayName = string.IsNullOrWhiteSpace(req.DisplayName) ? username : req.DisplayName.Trim(),
        PasswordHash = PasswordHasher.Hash(req.Password),
        CreatedAt = DateTime.UtcNow
    };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/api/auth/users/{user.Id}", new { username });
});

app.MapPost("/api/auth/login", async (AppDbContext db, LoginRequest req) =>
{
    var user = await db.Users.SingleOrDefaultAsync(u => u.Username == req.Username);
    if (user is null || !PasswordHasher.Verify(req.Password, user.PasswordHash))
        return Results.Json(new ErrorResponse { Message = "Invalid username or password." }, statusCode: 401);

    var expiresAt = DateTime.UtcNow.AddDays(7);
    var token = JwtService.CreateToken(user, jwtKey, issuer, audience, expiresAt);
    return Results.Ok(new LoginResponse { Token = token, Username = user.Username, DisplayName = user.DisplayName, ExpiresAt = expiresAt });
});

app.MapGet("/api/auth/me", (ClaimsPrincipal user) =>
    Results.Ok(new { user.Identity?.Name })).RequireAuthorization();

// ---------------------------------------------------------------------------
// Customers (owned by the authenticated user)
// ---------------------------------------------------------------------------
var customers = app.MapGroup("/api/customers").RequireAuthorization();

customers.MapGet("/", async (ClaimsPrincipal principal, AppDbContext db) =>
{
    var ownerId = principal.GetUserId();
    var list = await db.Customers
        .Where(c => c.OwnerId == ownerId)
        .OrderByDescending(c => c.UpdatedAt)
        .ToListAsync();
    return Results.Ok(list);
});

customers.MapGet("/{id:guid}", async (Guid id, ClaimsPrincipal principal, AppDbContext db) =>
{
    var ownerId = principal.GetUserId();
    var customer = await db.Customers.SingleOrDefaultAsync(c => c.Id == id && c.OwnerId == ownerId);
    return customer is null ? Results.NotFound() : Results.Ok(customer);
});

customers.MapPost("/", async (CustomerRequest req, ClaimsPrincipal principal, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(req.Name))
        return Results.BadRequest(new ErrorResponse { Message = "Name is required." });

    var customer = new Customer
    {
        OwnerId = principal.GetUserId(),
        Name = req.Name.Trim(),
        Company = req.Company,
        Email = req.Email,
        Phone = req.Phone,
        Address = req.Address,
        Notes = req.Notes,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    db.Customers.Add(customer);
    await db.SaveChangesAsync();
    return Results.Created($"/api/customers/{customer.Id}", customer);
});

customers.MapPut("/{id:guid}", async (Guid id, CustomerRequest req, ClaimsPrincipal principal, AppDbContext db) =>
{
    var ownerId = principal.GetUserId();
    var customer = await db.Customers.SingleOrDefaultAsync(c => c.Id == id && c.OwnerId == ownerId);
    if (customer is null)
        return Results.NotFound();
    if (string.IsNullOrWhiteSpace(req.Name))
        return Results.BadRequest(new ErrorResponse { Message = "Name is required." });

    customer.Name = req.Name.Trim();
    customer.Company = req.Company;
    customer.Email = req.Email;
    customer.Phone = req.Phone;
    customer.Address = req.Address;
    customer.Notes = req.Notes;
    customer.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok(customer);
});

customers.MapDelete("/{id:guid}", async (Guid id, ClaimsPrincipal principal, AppDbContext db) =>
{
    var ownerId = principal.GetUserId();
    var customer = await db.Customers.SingleOrDefaultAsync(c => c.Id == id && c.OwnerId == ownerId);
    if (customer is null)
        return Results.NotFound();
    db.Customers.Remove(customer);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Map("/", () => Results.Redirect("/swagger"));

app.Run();

public static partial class Program; // marker for EF Core design-time tooling

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                    ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }
}