using CarRental.Application.DTOs;
using CarRental.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RentalsController : ControllerBase
{
    private readonly IRentalService _rentalService;

    public RentalsController(IRentalService rentalService)
    {
        _rentalService = rentalService;
    }

    /// <summary>
    /// Pickup endpoint.
    /// </summary>
    [HttpPost("pickup")]
    [ProducesResponseType(typeof(RentalResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RentalResponse>> RegisterPickup([FromBody] PickupRequest request)
    {
        var result = await _rentalService.RegisterPickupAsync(request);
        
        return CreatedAtAction(
            nameof(GetRental),
            new { bookingNumber = result.BookingNumber },
            result);
    }

    /// <summary>
    /// Return endpoint.
    /// </summary>
    [HttpPost("return")]
    [ProducesResponseType(typeof(RentalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RentalResponse>> RegisterReturn([FromBody] ReturnRequest request)
    {
        var result = await _rentalService.RegisterReturnAsync(request);
        
        return Ok(result);
    }

    /// <summary>
    /// Get rental details by booking number
    /// </summary>
    [HttpGet("{bookingNumber}")]
    [ProducesResponseType(typeof(RentalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RentalResponse>> GetRental(string bookingNumber)
    {
        var result = await _rentalService.GetRentalAsync(bookingNumber);
        
        if (result == null)
        {
            return NotFound(new { message = $"Rental with booking number '{bookingNumber}' not found." });
        }
        
        return Ok(result);
    }
}
