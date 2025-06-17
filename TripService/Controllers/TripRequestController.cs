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
    public class TripRequestController : ControllerBase
    {
        private readonly ITripService _tripService;

        public TripRequestController(ITripService tripService)
        {
            _tripService = tripService;
        }

        [HttpPost]
        public async Task<ActionResult<TripRequest>> CreateRequest([FromBody] TripRequestDTO tripRequestDTO)
        {
            try
            {
                var request = await _tripService.CreateTripRequestAsync(tripRequestDTO);
                return CreatedAtAction(nameof(CreateRequest), new { id = request.Id }, request);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveRequest(int id, [FromQuery] string driverId)
        {
            try
            {
                await _tripService.ApproveTripRequestAsync(id, driverId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectRequest(int id, [FromQuery] string driverId)
        {
            try
            {
                await _tripService.RejectTripRequestAsync(id, driverId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}