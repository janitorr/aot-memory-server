using Microsoft.EntityFrameworkCore;
using Mittens.Models;

namespace Mittens.Data;

public class AppDbContext : DbContext
{
    public DbSet<MittensFact> MittensFacts => Set<MittensFact>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("mittens");
        modelBuilder.Entity<MittensFact>(entity =>
        {
            entity.HasIndex(e => new { e.Category, e.Key, e.Scope }).IsUnique();
            entity.HasIndex(e => e.Scope);
        });
    }
}
