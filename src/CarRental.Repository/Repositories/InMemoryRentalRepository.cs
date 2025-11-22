using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;
using System.Collections.Concurrent;

namespace CarRental.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of rental repository
/// Can be easily replaced with a database implementation
/// </summary>
public class InMemoryRentalRepository : IRentalRepository
{
    private readonly ConcurrentDictionary<string, Rental> _rentals = new();

    public Task<Rental?> GetByBookingNumberAsync(string bookingNumber)
    {
        _rentals.TryGetValue(bookingNumber, out var rental);
        return Task.FromResult(rental);
    }

    public Task AddAsync(Rental rental)
    {
        if (!_rentals.TryAdd(rental.BookingNumber, rental))
        {
            throw new InvalidOperationException($"Rental with booking number '{rental.BookingNumber}' already exists.");
        }
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Rental rental)
    {
        _rentals[rental.BookingNumber] = rental;
        return Task.CompletedTask;
    }

    public Task<bool> BookingNumberExistsAsync(string bookingNumber)
    {
        return Task.FromResult(_rentals.ContainsKey(bookingNumber));
    }
}
