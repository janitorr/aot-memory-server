using Microsoft.EntityFrameworkCore;
using Mittens.Core.Fact;

namespace Mittens.Host.Memory.Data;

public class AppDbContext : DbContext
{
    public DbSet<Fact> MittensFacts => Set<Fact>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("mittens");
        modelBuilder.Entity<Fact>(entity =>
        {
            entity.HasIndex(e => new { e.Category, e.Key, e.Scope }).IsUnique();
            entity.HasIndex(e => e.Scope);
        });
    }
}
