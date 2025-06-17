using System;
using TripService.DTOs;
using TripService.Models;

namespace TripService;

public interface ITripService
{
    Task<TripOffer> CreateTripOfferAsync(TripOfferDTO tripOfferDTO);
    Task<IEnumerable<TripOffer>> GetAvailableTripOffersAsync();
    Task<TripRequest> CreateTripRequestAsync(TripRequestDTO tripRequestDTO);
    Task ApproveTripRequestAsync(int requestId, string driverId);
    Task RejectTripRequestAsync(int requestId, string driverId);
    Task<Trip> GetTripDetailsAsync(int tripId);
    Task<IEnumerable<TripRequest>> GetTripRequestsForOfferAsync(int tripOfferId, string driverId);
    Task CancelTripOfferAsync(int tripOfferId, string driverId);
    Task<TripOfferDetailsDTO> GetTripOfferByIdAsync(int id);
}
