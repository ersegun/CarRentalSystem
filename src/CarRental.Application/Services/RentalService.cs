using CarRental.Application.DTOs;
using CarRental.Application.Exceptions;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CarRental.Application.Services;

public class RentalService : IRentalService
{
    private readonly IRentalRepository _repository;
    private readonly IPricingService _pricingService;
    private readonly ILogger<RentalService> _logger;

    public RentalService(IRentalRepository repository, IPricingService pricingService, ILogger<RentalService> logger)
    {
        _repository = repository;
        _pricingService = pricingService;
        _logger = logger;
    }

    public async Task<RentalResponse> RegisterPickupAsync(PickupRequest request)
    {
        try
        {
            // Validate request
            ValidatePickupRequest(request);

            // Check if booking number already exists
            if (await _repository.BookingNumberExistsAsync(request.BookingNumber))
            {
                _logger.LogWarning("Duplicate booking number attempted: {BookingNumber}", request.BookingNumber);
                throw new DuplicateBookingException(request.BookingNumber);
            }
            
            var rental = new Rental
            {
                BookingNumber = request.BookingNumber,
                RegistrationNumber = request.RegistrationNumber,
                CustomerSocialSecurityNumber = request.CustomerSocialSecurityNumber,
                Category = request.Category,
                PickupDateTime = request.PickupDateTime,
                PickupMeterReading = request.PickupMeterReading
            };

            await _repository.AddAsync(rental);

            return MapToResponse(rental);
        }
        catch (Exception ex) when (ex is not DuplicateBookingException and not RentalValidationException)
        {
            _logger.LogError(ex, "Error registering pickup for booking number: {BookingNumber}", request.BookingNumber);
            throw;
        }
    }

    public async Task<RentalResponse> RegisterReturnAsync(ReturnRequest request)
    {
        try
        {
            // Validate request
            ValidateReturnRequest(request);

            // Get existing rental
            var rental = await _repository.GetByBookingNumberAsync(request.BookingNumber);
            if (rental == null)
            {
                _logger.LogWarning("Rental not found for booking number: {BookingNumber}", request.BookingNumber);
                throw new RentalNotFoundException(request.BookingNumber);
            }

            // Check if already returned
            if (rental.IsReturned)
            {
                _logger.LogWarning("Rental already returned for booking number: {BookingNumber}", request.BookingNumber);
                throw new RentalValidationException($"Rental with booking number '{request.BookingNumber}' has already been returned.");
            }

            // Validate return data
            ValidateReturnData(rental, request);

            // Update rental with return information
            rental.ReturnDateTime = request.ReturnDateTime;
            rental.ReturnMeterReading = request.ReturnMeterReading;

            // Calculate price using PricingService
            rental.TotalPrice = _pricingService.CalculatePrice(rental);

            await _repository.UpdateAsync(rental);

            return MapToResponse(rental);
        }
        catch (Exception ex) when (ex is not RentalNotFoundException and not RentalValidationException)
        {
            _logger.LogError(ex, "Error registering return for booking number: {BookingNumber}", request.BookingNumber);
            throw;
        }
    }

    public async Task<RentalResponse?> GetRentalAsync(string bookingNumber)
    {
        var rental = await _repository.GetByBookingNumberAsync(bookingNumber);
        if (rental == null)
        {
            _logger.LogWarning("Rental not found for booking number: {BookingNumber}", bookingNumber);
            return null;
        }

        return MapToResponse(rental);
    }

    private static void ValidatePickupRequest(PickupRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.BookingNumber))
            throw new RentalValidationException("Booking number is required.");

        if (string.IsNullOrWhiteSpace(request.RegistrationNumber))
            throw new RentalValidationException("Registration number is required.");

        if (string.IsNullOrWhiteSpace(request.CustomerSocialSecurityNumber))
            throw new RentalValidationException("Customer social security number is required.");

        if (request.PickupMeterReading < 0)
            throw new RentalValidationException("Pickup meter reading must be non-negative.");
    }

    private static void ValidateReturnRequest(ReturnRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.BookingNumber))
            throw new RentalValidationException("Booking number is required.");

        if (request.ReturnMeterReading < 0)
            throw new RentalValidationException("Return meter reading must be non-negative.");
    }

    private static void ValidateReturnData(Rental rental, ReturnRequest request)
    {
        if (request.ReturnDateTime < rental.PickupDateTime)
        {
            throw new RentalValidationException(
                $"Return date/time ({request.ReturnDateTime}) cannot be before pickup date/time ({rental.PickupDateTime}).");
        }

        if (request.ReturnMeterReading < rental.PickupMeterReading)
        {
            throw new RentalValidationException(
                $"Return meter reading ({request.ReturnMeterReading}) cannot be less than pickup meter reading ({rental.PickupMeterReading}).");
        }
    }

    private static RentalResponse MapToResponse(Rental rental)
    {
        return new RentalResponse
        {
            BookingNumber = rental.BookingNumber,
            RegistrationNumber = rental.RegistrationNumber,
            CustomerSocialSecurityNumber = rental.CustomerSocialSecurityNumber,
            Category = rental.Category,
            PickupDateTime = rental.PickupDateTime,
            PickupMeterReading = rental.PickupMeterReading,
            ReturnDateTime = rental.ReturnDateTime,
            ReturnMeterReading = rental.ReturnMeterReading,
            TotalPrice = rental.TotalPrice,
            IsReturned = rental.IsReturned
        };
    }
}
