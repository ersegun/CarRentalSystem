using CarRental.Application.Configuration;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using Microsoft.Extensions.Options;

namespace CarRental.Application.Services;

public class PricingService : IPricingService
{
    private readonly decimal _baseDayRental;
    private readonly decimal _baseKmPrice;

    public PricingService(IOptions<PricingConfiguration> pricingConfig)
    {
        _baseDayRental = pricingConfig.Value.BaseDayRental;
        _baseKmPrice = pricingConfig.Value.BaseKmPrice;
    }
    
    public decimal CalculatePrice(Rental rental)
    {
        var numberOfDays = rental.GetNumberOfDays();
        var numberOfKm = rental.GetNumberOfKilometers();

        return rental.Category switch
        {
            CarCategory.SmallCar => CalculateSmallCarPrice(numberOfDays),
            CarCategory.Combi => CalculateCombiPrice(numberOfDays, numberOfKm),
            CarCategory.Truck => CalculateTruckPrice(numberOfDays, numberOfKm),
            _ => throw new ArgumentException($"Unknown car category: {rental.Category}")
        };
    }
    
    private decimal CalculateSmallCarPrice(int numberOfDays)
    {
        return _baseDayRental * numberOfDays;
    }

    private decimal CalculateCombiPrice(int numberOfDays, int numberOfKm)
    {
        return (_baseDayRental * numberOfDays * 1.3m) + (_baseKmPrice * numberOfKm);
    }
    
    private decimal CalculateTruckPrice(int numberOfDays, int numberOfKm)
    {
        return (_baseDayRental * numberOfDays * 1.5m) + (_baseKmPrice * numberOfKm * 1.5m);
    }
}