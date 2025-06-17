using System;

namespace TripService.Models;

public class TripOffer
{
    public long Id { get; set; }
    public long DriverId { get; set; }
    public int Seats {get; set;}
    public long StartLocationId { get; set; }
    public long FinalLocationId { get; set; }
}