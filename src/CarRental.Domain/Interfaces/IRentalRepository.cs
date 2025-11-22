using CarRental.Domain.Entities;

namespace CarRental.Domain.Interfaces;

public interface IRentalRepository
{
    Task<Rental?> GetByBookingNumberAsync(string bookingNumber);
    
    Task AddAsync(Rental rental);
    
    Task UpdateAsync(Rental rental);
    
    Task<bool> BookingNumberExistsAsync(string bookingNumber);
}
