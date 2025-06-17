using System;
using TripService.DTOs;
using TripService.Models;

namespace TripService;

public interface ITripService
{
    Task<TripOffer> CreateTripOfferAsync(TripOfferDTO tripOfferDTO);
    Task<IEnumerable<TripOffer>> GetAvailableTripOffersAsync();
    Task<TripRequest> CreateTripRequestAsync(TripRequestDTO tripRequestDTO);
    Task ApproveTripRequestAsync(long requestId, long driverId);
    Task RejectTripRequestAsync(long requestId, long driverId);
    Task<Trip> GetTripDetailsAsync(long tripId);
    Task<IEnumerable<TripRequest>> GetTripRequestsForOfferAsync(long tripOfferId, long driverId);
    Task CancelTripOfferAsync(long tripOfferId, long driverId);
    Task<TripOfferDetailsDTO> GetTripOfferByIdAsync(long id);
}
