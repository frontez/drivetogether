using System;

namespace TripService.Models;

public class TripRequest
{
    public long Id { get; set; }
    public long UserId { get; set; }
}