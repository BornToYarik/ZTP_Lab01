using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ZTP_Lab01.Domains.Entities;

namespace ZTP_Lab01.Infarstructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ForbiddenPhrase> ForbiddenPhrases => Set<ForbiddenPhrase>();
    public DbSet<ProductHistory>  ProductHistories => Set<ProductHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Product>()
            .HasIndex(p=>p.Name)
            .IsUnique();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var modifiedProducts = ChangeTracker.Entries<Product>()
            .Where(e => e.State == EntityState.Modified)
            .ToList();
        foreach (var entity in modifiedProducts)
        {
            foreach (var property in entity.Properties)
            {
                if (property.IsModified && !Equals(property.OriginalValue, property.CurrentValue))
                {
                    ProductHistories.Add(new ProductHistory
                    {
                        ProductId = entity.Entity.Id,
                        PropertyName = property.Metadata.Name,
                        oldValue = property.OriginalValue?.ToString(),
                        newValue = property.CurrentValue?.ToString(),
                        ChangedAt = DateTime.UtcNow
                    });
                }
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}