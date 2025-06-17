using System;

namespace TripService.DTOs;

public class TripRequestDTO
{
    public int TripOfferId { get; set; }
    public string PassengerId { get; set; }
    public int RequestedSeats { get; set; }
}
