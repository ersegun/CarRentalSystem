using CarRental.Application.Configuration;
using CarRental.Application.DTOs;
using CarRental.Application.Exceptions;
using CarRental.Application.Services;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CarRental.Tests.Services;

public class RentalServiceTests
{
    private readonly Mock<IRentalRepository> _mockRepository;
    private readonly Mock<ILogger<RentalService>> _mockLogger;
    private readonly IPricingService _pricingService;
    private readonly RentalService _service;

    public RentalServiceTests()
    {
        _mockRepository = new Mock<IRentalRepository>();
        _mockLogger = new Mock<ILogger<RentalService>>();
        
        var pricingConfig = Options.Create(new PricingConfiguration
        {
            BaseDayRental = 100m,
            BaseKmPrice = 5m
        });
        
        _pricingService = new PricingService(pricingConfig);
        _service = new RentalService(_mockRepository.Object, _pricingService, _mockLogger.Object);
    }

    #region Pickup Tests

    [Fact]
    public async Task RegisterPickup_ValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new PickupRequest
        {
            BookingNumber = "BK001",
            RegistrationNumber = "ABC123",
            CustomerSocialSecurityNumber = "19900101-1234",
            Category = CarCategory.SmallCar,
            PickupDateTime = DateTime.Now,
            PickupMeterReading = 10000
        };

        _mockRepository.Setup(r => r.BookingNumberExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Rental>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.RegisterPickupAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.BookingNumber, result.BookingNumber);
        Assert.Equal(request.RegistrationNumber, result.RegistrationNumber);
        Assert.Equal(request.Category, result.Category);
        Assert.False(result.IsReturned);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Rental>()), Times.Once);
    }

    [Fact]
    public async Task RegisterPickup_DuplicateBookingNumber_ThrowsDuplicateBookingException()
    {
        // Arrange
        var request = new PickupRequest
        {
            BookingNumber = "BK001",
            RegistrationNumber = "ABC123",
            CustomerSocialSecurityNumber = "19900101-1234",
            Category = CarCategory.SmallCar,
            PickupDateTime = DateTime.Now,
            PickupMeterReading = 10000
        };

        _mockRepository.Setup(r => r.BookingNumberExistsAsync(request.BookingNumber))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateBookingException>(() =>
            _service.RegisterPickupAsync(request));
    }

    [Theory]
    [InlineData("", "ABC123", "19900101-1234")]
    [InlineData("BK001", "", "19900101-1234")]
    [InlineData("BK001", "ABC123", "")]
    public async Task RegisterPickup_InvalidRequest_ThrowsValidationException(
        string bookingNumber, string regNumber, string ssn)
    {
        // Arrange
        var request = new PickupRequest
        {
            BookingNumber = bookingNumber,
            RegistrationNumber = regNumber,
            CustomerSocialSecurityNumber = ssn,
            Category = CarCategory.SmallCar,
            PickupDateTime = DateTime.Now,
            PickupMeterReading = 10000
        };

        // Act & Assert
        await Assert.ThrowsAsync<RentalValidationException>(() =>
            _service.RegisterPickupAsync(request));
    }

    [Fact]
    public async Task RegisterPickup_NegativeMeterReading_ThrowsValidationException()
    {
        // Arrange
        var request = new PickupRequest
        {
            BookingNumber = "BK001",
            RegistrationNumber = "ABC123",
            CustomerSocialSecurityNumber = "19900101-1234",
            Category = CarCategory.SmallCar,
            PickupDateTime = DateTime.Now,
            PickupMeterReading = -100
        };

        // Act & Assert
        await Assert.ThrowsAsync<RentalValidationException>(() =>
            _service.RegisterPickupAsync(request));
    }

    #endregion

    #region Return Tests

    [Fact]
    public async Task RegisterReturn_ValidSmallCar_CalculatesCorrectPrice()
    {
        // Arrange
        var pickupDate = new DateTime(2024, 1, 1, 10, 0, 0);
        var returnDate = new DateTime(2024, 1, 3, 14, 0, 0); // 2.17 days -> ceiling = 3 days

        var existingRental = new Rental
        {
            BookingNumber = "BK001",
            RegistrationNumber = "ABC123",
            CustomerSocialSecurityNumber = "19900101-1234",
            Category = CarCategory.SmallCar,
            PickupDateTime = pickupDate,
            PickupMeterReading = 10000
        };

        var returnRequest = new ReturnRequest
        {
            BookingNumber = "BK001",
            ReturnDateTime = returnDate,
            ReturnMeterReading = 10150
        };

        _mockRepository.Setup(r => r.GetByBookingNumberAsync(returnRequest.BookingNumber))
            .ReturnsAsync(existingRental);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Rental>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.RegisterReturnAsync(returnRequest);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsReturned);
        // Price = 100 * 3 = 300 SEK
        Assert.Equal(300m, result.TotalPrice);
    }

    [Fact]
    public async Task RegisterReturn_ValidCombi_CalculatesCorrectPrice()
    {
        // Arrange
        var pickupDate = new DateTime(2024, 1, 1, 10, 0, 0);
        var returnDate = new DateTime(2024, 1, 3, 10, 0, 0); // Exactly 2 days

        var existingRental = new Rental
        {
            BookingNumber = "BK002",
            RegistrationNumber = "XYZ789",
            CustomerSocialSecurityNumber = "19900101-1234",
            Category = CarCategory.Combi,
            PickupDateTime = pickupDate,
            PickupMeterReading = 20000
        };

        var returnRequest = new ReturnRequest
        {
            BookingNumber = "BK002",
            ReturnDateTime = returnDate,
            ReturnMeterReading = 20200 // 200 km
        };

        _mockRepository.Setup(r => r.GetByBookingNumberAsync(returnRequest.BookingNumber))
            .ReturnsAsync(existingRental);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Rental>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.RegisterReturnAsync(returnRequest);

        // Assert
        // Price = 100 * 2 * 1.3 + 5 * 200 = 260 + 1000 = 1260 SEK
        Assert.Equal(1260m, result.TotalPrice);
    }

    [Fact]
    public async Task RegisterReturn_ValidTruck_CalculatesCorrectPrice()
    {
        // Arrange
        var pickupDate = new DateTime(2024, 1, 1, 10, 0, 0);
        var returnDate = new DateTime(2024, 1, 4, 10, 0, 0); // Exactly 3 days

        var existingRental = new Rental
        {
            BookingNumber = "BK003",
            RegistrationNumber = "TRK456",
            CustomerSocialSecurityNumber = "19900101-1234",
            Category = CarCategory.Truck,
            PickupDateTime = pickupDate,
            PickupMeterReading = 50000
        };

        var returnRequest = new ReturnRequest
        {
            BookingNumber = "BK003",
            ReturnDateTime = returnDate,
            ReturnMeterReading = 50300 // 300 km
        };

        _mockRepository.Setup(r => r.GetByBookingNumberAsync(returnRequest.BookingNumber))
            .ReturnsAsync(existingRental);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Rental>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.RegisterReturnAsync(returnRequest);

        // Assert
        // Price = 100 * 3 * 1.5 + 5 * 300 * 1.5 = 450 + 2250 = 2700 SEK
        Assert.Equal(2700m, result.TotalPrice);
    }

    [Fact]
    public async Task RegisterReturn_NonExistentBooking_ThrowsNotFoundException()
    {
        // Arrange
        var returnRequest = new ReturnRequest
        {
            BookingNumber = "NONEXISTENT",
            ReturnDateTime = DateTime.Now,
            ReturnMeterReading = 10100
        };

        _mockRepository.Setup(r => r.GetByBookingNumberAsync(returnRequest.BookingNumber))
            .ReturnsAsync((Rental?)null);

        // Act & Assert
        await Assert.ThrowsAsync<RentalNotFoundException>(() =>
            _service.RegisterReturnAsync(returnRequest));
    }

    [Fact]
    public async Task RegisterReturn_AlreadyReturned_ThrowsValidationException()
    {
        // Arrange
        var existingRental = new Rental
        {
            BookingNumber = "BK001",
            RegistrationNumber = "ABC123",
            CustomerSocialSecurityNumber = "19900101-1234",
            Category = CarCategory.SmallCar,
            PickupDateTime = DateTime.Now.AddDays(-3),
            PickupMeterReading = 10000,
            ReturnDateTime = DateTime.Now.AddDays(-1),
            ReturnMeterReading = 10150,
            TotalPrice = 300m
        };

        var returnRequest = new ReturnRequest
        {
            BookingNumber = "BK001",
            ReturnDateTime = DateTime.Now,
            ReturnMeterReading = 10200
        };

        _mockRepository.Setup(r => r.GetByBookingNumberAsync(returnRequest.BookingNumber))
            .ReturnsAsync(existingRental);

        // Act & Assert
        await Assert.ThrowsAsync<RentalValidationException>(() =>
            _service.RegisterReturnAsync(returnRequest));
    }

    [Fact]
    public async Task RegisterReturn_ReturnDateBeforePickup_ThrowsValidationException()
    {
        // Arrange
        var existingRental = new Rental
        {
            BookingNumber = "BK001",
            RegistrationNumber = "ABC123",
            CustomerSocialSecurityNumber = "19900101-1234",
            Category = CarCategory.SmallCar,
            PickupDateTime = new DateTime(2024, 1, 10),
            PickupMeterReading = 10000
        };

        var returnRequest = new ReturnRequest
        {
            BookingNumber = "BK001",
            ReturnDateTime = new DateTime(2024, 1, 8), // Before pickup
            ReturnMeterReading = 10100
        };

        _mockRepository.Setup(r => r.GetByBookingNumberAsync(returnRequest.BookingNumber))
            .ReturnsAsync(existingRental);

        // Act & Assert
        await Assert.ThrowsAsync<RentalValidationException>(() =>
            _service.RegisterReturnAsync(returnRequest));
    }

    [Fact]
    public async Task RegisterReturn_ReturnMeterReadingLessThanPickup_ThrowsValidationException()
    {
        // Arrange
        var existingRental = new Rental
        {
            BookingNumber = "BK001",
            RegistrationNumber = "ABC123",
            CustomerSocialSecurityNumber = "19900101-1234",
            Category = CarCategory.SmallCar,
            PickupDateTime = DateTime.Now.AddDays(-2),
            PickupMeterReading = 10000
        };

        var returnRequest = new ReturnRequest
        {
            BookingNumber = "BK001",
            ReturnDateTime = DateTime.Now,
            ReturnMeterReading = 9900 // Less than pickup
        };

        _mockRepository.Setup(r => r.GetByBookingNumberAsync(returnRequest.BookingNumber))
            .ReturnsAsync(existingRental);

        // Act & Assert
        await Assert.ThrowsAsync<RentalValidationException>(() =>
            _service.RegisterReturnAsync(returnRequest));
    }

    #endregion

    #region GetRental Tests

    [Fact]
    public async Task GetRental_ExistingBooking_ReturnsRental()
    {
        // Arrange
        var rental = new Rental
        {
            BookingNumber = "BK001",
            RegistrationNumber = "ABC123",
            CustomerSocialSecurityNumber = "19900101-1234",
            Category = CarCategory.SmallCar,
            PickupDateTime = DateTime.Now,
            PickupMeterReading = 10000
        };

        _mockRepository.Setup(r => r.GetByBookingNumberAsync(rental.BookingNumber))
            .ReturnsAsync(rental);

        // Act
        var result = await _service.GetRentalAsync(rental.BookingNumber);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(rental.BookingNumber, result.BookingNumber);
    }

    [Fact]
    public async Task GetRental_NonExistentBooking_ReturnsNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByBookingNumberAsync(It.IsAny<string>()))
            .ReturnsAsync((Rental?)null);

        // Act
        var result = await _service.GetRentalAsync("NONEXISTENT");

        // Assert
        Assert.Null(result);
    }

    #endregion
}