using System;
using System.ComponentModel.DataAnnotations;

namespace DriveTogetherBot.Entities;

public class TripOffer
{
    [Required]
    public long DriverId { get; set; }

    [Required]
    public long StartLocationId { get; set; }

    public Location StartLocation { get; set; }

    [Required]
    public long EndLocationId { get; set; }

    public Location EndLocation { get; set; }

    [Required]
    public DateTime DepartureTime { get; set; }

    [Range(1, 10)]
    public int AvailableSeats { get; set; }

    [Range(0, int.MaxValue)]
    public int PricePerSeat { get; set; }

    [StringLength(500)]
    public string Description { get; set; }
}

/*
public class TripOffer
{
    public int Id { get; set; }
    public long DriverId { get; set; }
    public int StartLocationId { get; set; }
    public Location StartLocation { get; set; }
    public int EndLocationId { get; set; }
    public Location EndLocation { get; set; }
    public DateTime DepartureTime { get; set; }
    public int AvailableSeats { get; set; }
    public decimal PricePerSeat { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Status { get; set; }
    public List<TripRequest> TripRequests { get; set; }
    public ConfirmedTrip ConfirmedTrip { get; set; }
}
*/
