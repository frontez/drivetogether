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
    public class LocationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LocationController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/locations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Location>>> GetLocations()
        {
            return await _context.Locations.ToListAsync();
        }

        // GET: api/locations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Location>> GetLocation(long id)
        {
            var location = await _context.Locations.FindAsync(id);

            if (location == null)
            {
                return NotFound();
            }

            return location;
        }

        // POST: api/locations
        [HttpPost]
        public async Task<ActionResult<Location>> PostLocation(LocationDTO locationDTO)
        {
            if (await _context.Locations.AnyAsync(l => l.Name == locationDTO.Name))
            {
                return Conflict("Location with this name already exists");
            }

            var location = new Location
            {
                Name = locationDTO.Name
            };

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLocation), new { id = location.Id }, location);
        }

        // DELETE: api/locations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(long id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null)
            {
                return NotFound();
            }

            // Check if location is being used in any trip offers
            var isUsed = await _context.TripOffers
                .AnyAsync(to => to.StartLocationId == id || to.EndLocationId == id);

            if (isUsed)
            {
                return BadRequest("Cannot delete location as it's being used in trip offers");
            }

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}