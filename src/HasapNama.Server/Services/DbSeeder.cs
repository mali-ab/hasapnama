using HasapNama.Server.Data;
using HasapNama.Server.Services;
using HasapNama.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace HasapNama.Server.Services;

public static class DbSeeder
{
    private const string DefaultAdmin = "admin";
    private const string DefaultAdminPassword = "admin123";

    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync())
            return;

        db.Users.Add(new AppUser
        {
            Username = DefaultAdmin,
            DisplayName = "Administrator",
            PasswordHash = PasswordHasher.Hash(DefaultAdminPassword)
        });

        await db.SaveChangesAsync();
    }
}