using System;

namespace TripService.DTOs;

public class TripRequestDTO
{
    public long TripOfferId { get; set; }
    public long PassengerId { get; set; }
    public int RequestedSeats { get; set; }
}
