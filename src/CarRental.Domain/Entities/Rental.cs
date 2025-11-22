using CarRental.Domain.Enums;

namespace CarRental.Domain.Entities;

// Represents a car rental transaction
public class Rental
{
    public string BookingNumber { get; set; } = string.Empty;
    
    public string RegistrationNumber { get; set; } = string.Empty;
    
    public string CustomerSocialSecurityNumber { get; set; } = string.Empty;
    
    public CarCategory Category { get; set; }
    
    // Pickup information
    public DateTime PickupDateTime { get; set; }
    
    public int PickupMeterReading { get; set; }
    
    // Return information
    public DateTime? ReturnDateTime { get; set; }
    
    public int? ReturnMeterReading { get; set; }
    
    // Calculated values
    public decimal? TotalPrice { get; set; }
    
    public bool IsReturned => ReturnDateTime.HasValue;
    
    public int GetNumberOfDays()
    {
        if (!ReturnDateTime.HasValue)
            throw new InvalidOperationException("Cannot calculate days for a rental that hasn't been returned");
        
        var timeSpan = ReturnDateTime.Value - PickupDateTime;
        return (int)Math.Ceiling(timeSpan.TotalDays);
    }
    
    public int GetNumberOfKilometers()
    {
        if (!ReturnMeterReading.HasValue)
            throw new InvalidOperationException("Cannot calculate kilometers for a rental that hasn't been returned");
        
        return ReturnMeterReading.Value - PickupMeterReading;
    }
}
