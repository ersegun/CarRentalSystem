using CarRental.Domain.Entities;

namespace CarRental.Application.Services;

public interface IPricingService
{
    decimal CalculatePrice(Rental rental);
}
