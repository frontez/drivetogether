using System;
using System.ComponentModel.DataAnnotations;
using TripService.Enums;

namespace TripService.Models;

public class Trip
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TripOfferId { get; set; }

    [Required]
    public string DriverId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    [Required]
    public TripStatus Status { get; set; } = TripStatus.Confirmed;

    public TripOffer TripOffer { get; set; }
    public ICollection<TripRequest> Passengers { get; set; }
}

