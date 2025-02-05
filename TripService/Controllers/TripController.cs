using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TripService.Models;
using TripService.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TripService.Controllers;


[ApiController]
[Route("api/v1/[controller]")]
public class TripController : ControllerBase
{
    private readonly AppDbContext _context;

    public TripController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Trip>> Get(long id)
    {
        var trip = await _context.Trips.FindAsync(id);
        if (trip == null)
        {
            return NotFound();
        }

        return trip;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Trip trip)
    {
        if (trip == null)
        {
            return BadRequest();
        }

        _context.Trips.Add(trip);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = trip.Id }, trip);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] Trip updatedTrip)
    {
        var trip = await _context.Trips.FindAsync(id);
        if (trip == null)
        {
            return NotFound();
        }       

        trip.DriverId = updatedTrip.DriverId;
        trip.Seats = updatedTrip.Seats;

        _context.Entry(trip).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Trips.Any(u => u.Id == id))
            {
                return NotFound($"Trip with id {id} not found");
            }
            else
            {
                throw;
            }
        }

        return NoContent(); //return Ok(trip);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var trip = await _context.Trips.FindAsync(id);
        if (trip == null)
        {
            return NotFound($"Trip with id {id} not found");
        }

        _context.Trips.Remove(trip);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}