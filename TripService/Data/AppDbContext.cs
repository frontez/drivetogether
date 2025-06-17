using System;
using Microsoft.EntityFrameworkCore;
using TripService.Models;

namespace TripService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
                : base(options)
    {
    }

    public DbSet<Location> Locations { get; set; }
    public DbSet<TripOffer> TripOffers { get; set; }
    public DbSet<TripRequest> TripRequests { get; set; }
    public DbSet<Trip> Trips { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Location configuration
        modelBuilder.Entity<Location>()
            .HasIndex(l => l.Name)
            .IsUnique();

        // TripOffer relationships with Location
        modelBuilder.Entity<TripOffer>()
            .HasOne(to => to.StartLocation)
            .WithMany()
            .HasForeignKey(to => to.StartLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TripOffer>()
            .HasOne(to => to.EndLocation)
            .WithMany()
            .HasForeignKey(to => to.EndLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TripOffer>()
            .HasMany(to => to.TripRequests)
            .WithOne(tr => tr.TripOffer)
            .HasForeignKey(tr => tr.TripOfferId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TripOffer>()
            .HasOne(to => to.ConfirmedTrip)
            .WithOne(t => t.TripOffer)
            .HasForeignKey<Trip>(t => t.TripOfferId)
            .OnDelete(DeleteBehavior.Cascade);

        // Other relationships remain the same
        modelBuilder.Entity<TripRequest>()
            .HasOne(tr => tr.Trip)
            .WithMany(t => t.Passengers)
            .HasForeignKey(tr => tr.TripId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Trip>()
            .HasMany(t => t.Passengers)
            .WithOne(tr => tr.Trip)
            .HasForeignKey(tr => tr.TripId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TripOffer>()
            .Property(t => t.DepartureTime)
            .HasConversion(
                v => v.ToUniversalTime(), // Convert to UTC when saving
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc).ToLocalTime()); // Mark as UTC when reading
    }
}