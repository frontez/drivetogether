using System;
using System.ComponentModel.DataAnnotations;

namespace TripService.Models;

public class Location
{
    public long Id { get; set; }
    
    [Required]
    public string Name { get; set; }
}