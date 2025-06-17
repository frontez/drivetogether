using System;
using System.ComponentModel.DataAnnotations;
using TripService.Enums;

namespace TripService.DTOs;

public class TripOfferDTO
{
    [Required]
    public string DriverId { get; set; }

    [Required]
    public long StartLocationId { get; set; }

    [Required]
    public long EndLocationId { get; set; }

    [Required]
    public DateTime DepartureTime { get; set; }

    [Required]
    [Range(1, 10)]
    public int AvailableSeats { get; set; }

    [Required]
    public decimal PricePerSeat { get; set; }

    public string Description { get; set; }
}

    public class TripOfferDetailsDTO
    {
        public int Id { get; set; }
        public string DriverId { get; set; }
        public LocationDTO StartLocation { get; set; }
        public LocationDTO EndLocation { get; set; }
        public DateTime DepartureTime { get; set; }
        public int AvailableSeats { get; set; }
        public decimal PricePerSeat { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public TripStatus Status { get; set; }
        public int PendingRequestsCount { get; set; }
        public int ApprovedRequestsCount { get; set; }
        //public TripDTO ConfirmedTrip { get; set; } // If you want to include trip details
    }
