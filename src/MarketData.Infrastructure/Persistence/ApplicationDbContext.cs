namespace MarketData.Infrastructure.Persistence;

using MarketData.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Asset> Assets => this.Set<Asset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ExternalId).IsUnique();
            entity.Property(e => e.Symbol).IsRequired().HasMaxLength(20);
            entity.Property(e => e.LastPrice).HasPrecision(18, 8);
        });
    }
}