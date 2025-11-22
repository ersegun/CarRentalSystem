using CarRental.Application.DTOs;

namespace CarRental.Application.Services;

public interface IRentalService
{
    Task<RentalResponse> RegisterPickupAsync(PickupRequest request);
    
    Task<RentalResponse> RegisterReturnAsync(ReturnRequest request);
    
    Task<RentalResponse?> GetRentalAsync(string bookingNumber);
}
