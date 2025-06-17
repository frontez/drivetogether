using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TripService.Enums;

namespace TripService.Models;

public class TripOffer
{
    [Key]
    public long Id { get; set; }

    [Required]
    public long DriverId { get; set; }

    [Required]
    [ForeignKey("StartLocation")]
    public long StartLocationId { get; set; }
    public Location StartLocation { get; set; }

    [Required]
    [ForeignKey("EndLocation")]
    public long EndLocationId { get; set; }
    public Location EndLocation { get; set; }

    [Required]
    public DateTime DepartureTime { get; set; }

    [Required]
    [Range(1, 10)]
    public int AvailableSeats { get; set; }

    [Required]
    public decimal PricePerSeat { get; set; }

    public string Description { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public TripStatus Status { get; set; } = TripStatus.Pending;

    public ICollection<TripRequest> TripRequests { get; set; }
    public Trip ConfirmedTrip { get; set; }
}