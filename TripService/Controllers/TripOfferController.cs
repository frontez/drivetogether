using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TripService.Models;
using TripService.Data;
using TripService.DTOs;

namespace TripService.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TripOfferController : ControllerBase
    {
        private readonly ITripService _tripService;
        private readonly MessagePublisher _publisher;

        public TripOfferController(ITripService tripService, MessagePublisher publisher)
        {
            _tripService = tripService;
            _publisher = publisher;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TripOffer>>> GetAvailableOffers()
        {
            try
            {
                var offers = await _tripService.GetAvailableTripOffersAsync();
                return Ok(offers);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TripOffer>> GetTripOffer(int id)
        {
            try
            {
                var offer = await _tripService.GetTripOfferByIdAsync(id);
                if (offer == null)
                {
                    return NotFound();
                }
                return Ok(offer);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<TripOffer>> CreateOffer([FromBody] TripOfferDTO tripOfferDTO)
        {
            try
            {
                var offer = await _tripService.CreateTripOfferAsync(tripOfferDTO);
                _publisher.Publish(offer);
                return CreatedAtAction(nameof(GetAvailableOffers), new { id = offer.Id }, offer);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelOffer(long id, [FromQuery] long driverId)
        {
            try
            {
                await _tripService.CancelTripOfferAsync(id, driverId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/requests")]
        public async Task<ActionResult<IEnumerable<TripRequest>>> GetRequests(long id, [FromQuery] long driverId)
        {
            try
            {
                var requests = await _tripService.GetTripRequestsForOfferAsync(id, driverId);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}