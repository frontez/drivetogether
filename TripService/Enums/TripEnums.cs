using System;

namespace TripService.Enums;

public enum TripStatus
{
    Pending, Confirmed, InProgress, Completed, Cancelled
}

public enum RequestStatus
{
    Pending, Approved, Rejected, Cancelled
}
