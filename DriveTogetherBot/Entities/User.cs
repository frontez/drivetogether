using System;

namespace DriveTogetherBot.Entities;

public class User
{
    public FormStep CurrentStep { get; set; }
    public TripStep TripStepEnum { get; set; }
    public long Id { get; set; }
    public string Username { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }    
    public TripOffer TripOffer { get; set; }
}

public enum FormStep
{
    Name,
    Phone,
    Complete
}

public enum TripStep
{
    TripStartLocation,
    TripEndLocation,
    DepartureDate,
    DepartureTime,
    AvailableSeats,
    Price,
    Description,
    Complete
}
