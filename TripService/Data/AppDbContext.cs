using System;
using Microsoft.EntityFrameworkCore;
using TripService.Models;

namespace TripService.Data;

public class AppDbContext : DbContext
{
    public DbSet<Trip> Trips { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}