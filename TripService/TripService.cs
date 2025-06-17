using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TripService;
using TripService.Data;
using TripService.DTOs;
using TripService.Enums;
using TripService.Models;

namespace TripService;

public class TripService : ITripService
{
    private readonly AppDbContext _context;

    public TripService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TripOffer> CreateTripOfferAsync(TripOfferDTO tripOfferDTO)
    {
        var tripOffer = new TripOffer
        {
            DriverId = tripOfferDTO.DriverId,
            StartLocationId = tripOfferDTO.StartLocationId,
            EndLocationId = tripOfferDTO.EndLocationId,
            DepartureTime = tripOfferDTO.DepartureTime,
            AvailableSeats = tripOfferDTO.AvailableSeats,
            PricePerSeat = tripOfferDTO.PricePerSeat,
            Description = tripOfferDTO.Description,
            Status = TripStatus.Pending
        };

        _context.TripOffers.Add(tripOffer);
        await _context.SaveChangesAsync();
        return tripOffer;
    }

    public async Task<IEnumerable<TripOffer>> GetAvailableTripOffersAsync()
    {
        return await _context.TripOffers
            .Where(to => to.Status == TripStatus.Pending && to.AvailableSeats > 0)
            .OrderBy(to => to.DepartureTime)
            .ToListAsync();
    }

    public async Task<TripRequest> CreateTripRequestAsync(TripRequestDTO tripRequestDTO)
    {
        var tripOffer = await _context.TripOffers
            .FirstOrDefaultAsync(to => to.Id == tripRequestDTO.TripOfferId && to.Status == TripStatus.Pending);

        if (tripOffer == null)
            throw new Exception("Trip offer not found or not available");

        if (tripOffer.DriverId == tripRequestDTO.PassengerId)
            throw new Exception("Driver cannot request their own trip");

        if (tripOffer.AvailableSeats < tripRequestDTO.RequestedSeats)
            throw new Exception("Not enough available seats");

        var existingRequest = await _context.TripRequests
            .FirstOrDefaultAsync(tr => tr.TripOfferId == tripRequestDTO.TripOfferId &&
                                      tr.PassengerId == tripRequestDTO.PassengerId &&
                                      tr.Status == RequestStatus.Pending);

        if (existingRequest != null)
            throw new Exception("You already have a pending request for this trip");

        var tripRequest = new TripRequest
        {
            TripOfferId = tripRequestDTO.TripOfferId,
            PassengerId = tripRequestDTO.PassengerId,
            RequestedSeats = tripRequestDTO.RequestedSeats,
            Status = RequestStatus.Pending
        };

        _context.TripRequests.Add(tripRequest);
        await _context.SaveChangesAsync();
        return tripRequest;
    }

    public async Task ApproveTripRequestAsync(long requestId, long driverId)
    {
        var request = await _context.TripRequests
            .Include(tr => tr.TripOffer)
            .FirstOrDefaultAsync(tr => tr.Id == requestId);

        if (request == null)
            throw new Exception("Request not found");

        if (request.TripOffer.DriverId != driverId)
            throw new Exception("Only the driver can approve requests");

        if (request.TripOffer.AvailableSeats < request.RequestedSeats)
            throw new Exception("Not enough available seats");

        request.Status = RequestStatus.Approved;
        request.TripOffer.AvailableSeats -= request.RequestedSeats;

        await _context.SaveChangesAsync();
        await CheckAndConfirmTripAsync(request.TripOfferId);
    }

    public async Task RejectTripRequestAsync(long requestId, long driverId)
    {
        var request = await _context.TripRequests
            .Include(tr => tr.TripOffer)
            .FirstOrDefaultAsync(tr => tr.Id == requestId);

        if (request == null)
            throw new Exception("Request not found");

        if (request.TripOffer.DriverId != driverId)
            throw new Exception("Only the driver can reject requests");

        request.Status = RequestStatus.Rejected;
        await _context.SaveChangesAsync();
    }

    public async Task<Trip> GetTripDetailsAsync(long tripId)
    {
        return await _context.Trips
            .Include(t => t.TripOffer)
            .Include(t => t.Passengers)
            //.ThenInclude(p => p.Passenger) // Assuming you have a User model
            .FirstOrDefaultAsync(t => t.Id == tripId);
    }

    public async Task<IEnumerable<TripRequest>> GetTripRequestsForOfferAsync(long tripOfferId, long driverId)
    {
        var tripOffer = await _context.TripOffers.FindAsync(tripOfferId);
        if (tripOffer == null || tripOffer.DriverId != driverId)
            throw new Exception("Trip offer not found or unauthorized");

        return await _context.TripRequests
            .Where(tr => tr.TripOfferId == tripOfferId && tr.Status == RequestStatus.Pending)
            //.Include(tr => tr.Passenger) // Assuming you have a User model
            .ToListAsync();
    }

    public async Task CancelTripOfferAsync(long tripOfferId, long driverId)
    {
        var tripOffer = await _context.TripOffers
            .Include(to => to.TripRequests)
            .FirstOrDefaultAsync(to => to.Id == tripOfferId);

        if (tripOffer == null)
            throw new Exception("Trip offer not found");

        if (tripOffer.DriverId != driverId)
            throw new Exception("Only the driver can cancel the trip offer");

        tripOffer.Status = TripStatus.Cancelled;

        foreach (var request in tripOffer.TripRequests.Where(tr => tr.Status == RequestStatus.Pending))
        {
            request.Status = RequestStatus.Cancelled;
        }

        await _context.SaveChangesAsync();
    }

    private async Task CheckAndConfirmTripAsync(long tripOfferId)
    {
        var tripOffer = await _context.TripOffers
            .Include(to => to.TripRequests)
            .FirstOrDefaultAsync(to => to.Id == tripOfferId);

        if (tripOffer == null || tripOffer.Status != TripStatus.Pending)
            return;

        if (tripOffer.AvailableSeats <= 0)
        {
            var trip = new Trip
            {
                TripOfferId = tripOffer.Id,
                DriverId = tripOffer.DriverId,
                StartTime = tripOffer.DepartureTime,
                Status = TripStatus.Confirmed
            };

            _context.Trips.Add(trip);
            tripOffer.Status = TripStatus.Confirmed;

            var approvedRequests = tripOffer.TripRequests
                .Where(tr => tr.Status == RequestStatus.Approved);

            foreach (var request in approvedRequests)
            {
                request.Trip = trip;
            }

            await _context.SaveChangesAsync();
        }
    }

    public async Task<TripOfferDetailsDTO> GetTripOfferByIdAsync(long id)
    {
        var offer = await _context.TripOffers
            .Include(to => to.StartLocation)
            .Include(to => to.EndLocation)
            .Include(to => to.TripRequests)
            .Include(to => to.ConfirmedTrip)
            .FirstOrDefaultAsync(to => to.Id == id);

        return new TripOfferDetailsDTO
        {
            Id = offer.Id,
            DriverId = offer.DriverId,
            StartLocation = new LocationDTO { Id = offer.StartLocation.Id, Name = offer.StartLocation.Name },
            EndLocation = new LocationDTO { Id = offer.EndLocation.Id, Name = offer.EndLocation.Name },
            DepartureTime = offer.DepartureTime,
            AvailableSeats = offer.AvailableSeats,
            PricePerSeat = offer.PricePerSeat,
            Description = offer.Description,
            CreatedAt = offer.CreatedAt,
            Status = offer.Status,
            PendingRequestsCount = offer.TripRequests.Count(r => r.Status == RequestStatus.Pending),
            ApprovedRequestsCount = offer.TripRequests.Count(r => r.Status == RequestStatus.Approved),
            /*
            ConfirmedTrip = offer.ConfirmedTrip != null ? new TripDTO
            {
                Id = offer.ConfirmedTrip.Id,
                StartTime = offer.ConfirmedTrip.StartTime,
                Status = offer.ConfirmedTrip.Status
            } : null
            */
        };
    }
}