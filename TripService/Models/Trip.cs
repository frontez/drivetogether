using System;

namespace TripService.Models;

public class Trip
{
    public long Id { get; set; }
    public string DriverId { get; set; }
    public int Seats {get; set;}
}