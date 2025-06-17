using System;
using System.ComponentModel.DataAnnotations;

namespace DriveTogetherBot.Entities;

public class TripOffer
{
    [Required]
    public long DriverId { get; set; }
    
    [Required]
    public long StartLocationId { get; set; }
    
    [Required]
    public long EndLocationId { get; set; }
    
    [Required]
    public DateTime DepartureTime { get; set; }
    
    [Range(1, 10)]
    public int AvailableSeats { get; set; }
    
    [Range(0, int.MaxValue)]
    public int PricePerSeat { get; set; }
    
    [StringLength(500)]
    public string Description { get; set; }
}
