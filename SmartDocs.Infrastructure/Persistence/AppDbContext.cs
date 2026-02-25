using Microsoft.EntityFrameworkCore;
using SmartDocs.Domain.Entities;

namespace SmartDocs.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(builder =>
        {
            builder.HasKey(d => d.Id);
            builder.Property(d => d.FileName).IsRequired();
            builder.Property(d => d.BlobName).IsRequired();
            builder.Property(d => d.Status).IsRequired();
        });
    }
}