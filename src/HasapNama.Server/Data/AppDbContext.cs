using HasapNama.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace HasapNama.Server.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>(u =>
        {
            u.HasIndex(x => x.Username).IsUnique();
            u.Property(x => x.Username).HasMaxLength(64).IsRequired();
        });

        builder.Entity<Customer>(c =>
        {
            c.HasIndex(x => new { x.OwnerId, x.Name });
            c.Property(x => x.Name).HasMaxLength(200).IsRequired();
            c.Property(x => x.Email).HasMaxLength(200);
            c.Property(x => x.Phone).HasMaxLength(64);
            c.Property(x => x.Company).HasMaxLength(200);
        });
    }
}