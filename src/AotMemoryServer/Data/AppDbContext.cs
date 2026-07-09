using Microsoft.EntityFrameworkCore;
using AotMemoryServer.Models;

namespace AotMemoryServer.Data;

public class AppDbContext : DbContext
{
    public DbSet<MemoryFact> MemoryFacts => Set<MemoryFact>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("mem");
        modelBuilder.Entity<MemoryFact>(entity =>
        {
            entity.HasIndex(e => new { e.Category, e.Key, e.Scope }).IsUnique();
            entity.HasIndex(e => e.Scope);
        });
    }
}
