using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TripService.Enums;

namespace TripService.Models;

public class TripRequest
{
    [Key]
    public long Id { get; set; }

    [Required]
    public long TripOfferId { get; set; }

    [Required]
    public long PassengerId { get; set; }

    [Required]
    public int RequestedSeats { get; set; }

    [Required]
    public DateTime RequestTime { get; set; } = DateTime.UtcNow;

    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    public long? TripId { get; set; }
    
    [JsonIgnore]
    public TripOffer TripOffer { get; set; }

    [JsonIgnore]
    public Trip Trip { get; set; }
}