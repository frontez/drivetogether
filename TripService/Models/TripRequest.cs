using System;
using System.ComponentModel.DataAnnotations;
using TripService.Enums;

namespace TripService.Models;

public class TripRequest
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TripOfferId { get; set; }

    [Required]
    public string PassengerId { get; set; }

    [Required]
    public int RequestedSeats { get; set; }

    [Required]
    public DateTime RequestTime { get; set; } = DateTime.UtcNow;

    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    public int? TripId { get; set; }

    public TripOffer TripOffer { get; set; }
    public Trip Trip { get; set; }
}