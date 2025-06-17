using System;
using System.ComponentModel.DataAnnotations;

namespace TripService.DTOs;

public class LocationDTO
{
    public long Id { get; set; }
    [Required]
    public string Name { get; set; }
}
