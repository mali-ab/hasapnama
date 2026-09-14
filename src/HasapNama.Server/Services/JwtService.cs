using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HasapNama.Shared.Models;
using Microsoft.IdentityModel.Tokens;

namespace HasapNama.Server.Services;

/// <summary>Creates signed (HS256) JWT bearer tokens for authenticated users.</summary>
public static class JwtService
{
    public static string CreateToken(AppUser user, string key, string issuer, string audience, DateTime expiresAt)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new("displayName", user.DisplayName)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}