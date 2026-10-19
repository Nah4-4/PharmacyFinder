using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PharmacyFinder.Api.Models;

namespace PharmacyFinder.Api.Data;

public class PFContext(DbContextOptions<PFContext> options)
    : IdentityDbContext<User, IdentityRole<int>, int>(options)
{
    public DbSet<Pharmacy> Pharmacies => Set<Pharmacy>();
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<PharmacyStock> PharmacyStocks => Set<PharmacyStock>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(user => user.Email)
                .HasMaxLength(200);

            entity.Property(user => user.Role)
                .HasConversion<string>();

            entity.HasIndex(user => user.Email)
                .IsUnique();

            entity.HasMany(user => user.Pharmacies)
                .WithOne(pharmacy => pharmacy.Owner)
                .HasForeignKey(pharmacy => pharmacy.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Pharmacy>(entity =>
        {
            entity.Property(pharmacy => pharmacy.ApprovalStatus)
                .HasConversion<string>();

            entity.HasIndex(pharmacy => pharmacy.LicenseNumber)
                .IsUnique();

            entity.HasIndex(pharmacy => new
            {
                pharmacy.ApprovalStatus,
                pharmacy.Latitude,
                pharmacy.Longitude
            });
        });

        modelBuilder.Entity<PharmacyStock>(entity =>
        {
            entity.HasIndex(stock => new
            {
                stock.PharmacyId,
                stock.MedicineId
            })
            .IsUnique();
        });
    }
}
