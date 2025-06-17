using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TripService.Models;
using TripService.Data;

namespace TripService.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TripController : ControllerBase
    {
        private readonly ITripService _tripService;

        public TripController(ITripService tripService)
        {
            _tripService = tripService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Trip>> GetTripDetails(int id)
        {
            try
            {
                var trip = await _tripService.GetTripDetailsAsync(id);
                if (trip == null)
                    return NotFound();

                return Ok(trip);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}