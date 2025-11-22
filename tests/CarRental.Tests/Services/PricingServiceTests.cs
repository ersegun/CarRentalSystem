using CarRental.Application.Configuration;
using CarRental.Application.Services;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using Microsoft.Extensions.Options;
using Xunit;

namespace CarRental.Tests.Services;

public class PricingServiceTests
{
    private readonly PricingService _pricingService;

    public PricingServiceTests()
    {
        var pricingConfig = Options.Create(new PricingConfiguration
        {
            BaseDayRental = 100m,
            BaseKmPrice = 5m
        });
        
        _pricingService = new PricingService(pricingConfig);
    }

    [Theory]
    [InlineData(1, 0, 100)]
    [InlineData(3, 0, 300)]
    [InlineData(7, 0, 700)]
    public void CalculatePrice_SmallCar_ReturnsCorrectPrice(int days, int km, decimal expectedPrice)
    {
        // Arrange
        var rental = CreateRental(CarCategory.SmallCar, days, km);

        // Act
        var price = _pricingService.CalculatePrice(rental);

        // Assert
        Assert.Equal(expectedPrice, price);
    }

    [Theory]
    [InlineData(1, 0, 130)]
    [InlineData(2, 100, 760)]
    [InlineData(3, 200, 1390)]
    [InlineData(5, 500, 3150)]
    public void CalculatePrice_Combi_ReturnsCorrectPrice(int days, int km, decimal expectedPrice)
    {
        // Arrange
        var rental = CreateRental(CarCategory.Combi, days, km);

        // Act
        var price = _pricingService.CalculatePrice(rental);

        // Assert
        Assert.Equal(expectedPrice, price);
    }

    [Theory]
    [InlineData(1, 0, 150)]
    [InlineData(2, 100, 1050)]
    [InlineData(3, 200, 1950)]
    [InlineData(5, 500, 4500)]
    public void CalculatePrice_Truck_ReturnsCorrectPrice(int days, int km, decimal expectedPrice)
    {
        // Arrange
        var rental = CreateRental(CarCategory.Truck, days, km);

        // Act
        var price = _pricingService.CalculatePrice(rental);

        // Assert
        Assert.Equal(expectedPrice, price);
    }

    [Fact]
    public void CalculatePrice_AllCategories_CalculatesCorrectly()
    {
        var days = 2;
        var km = 100;

        // SmallCar
        var smallCarRental = CreateRental(CarCategory.SmallCar, days, km);
        var smallCarPrice = _pricingService.CalculatePrice(smallCarRental);
        Assert.Equal(200m, smallCarPrice);

        // Combi
        var combiRental = CreateRental(CarCategory.Combi, days, km);
        var combiPrice = _pricingService.CalculatePrice(combiRental);
        Assert.Equal(760m, combiPrice);

        // Truck
        var truckRental = CreateRental(CarCategory.Truck, days, km);
        var truckPrice = _pricingService.CalculatePrice(truckRental);
        Assert.Equal(1050m, truckPrice);
    }

    private static Rental CreateRental(CarCategory category, int days, int km)
    {
        var pickupDate = new DateTime(2024, 1, 1, 10, 0, 0);
        var returnDate = pickupDate.AddDays(days);

        return new Rental
        {
            BookingNumber = "TEST",
            RegistrationNumber = "TEST123",
            CustomerSocialSecurityNumber = "19900101-1234",
            Category = category,
            PickupDateTime = pickupDate,
            PickupMeterReading = 10000,
            ReturnDateTime = returnDate,
            ReturnMeterReading = 10000 + km
        };
    }
}