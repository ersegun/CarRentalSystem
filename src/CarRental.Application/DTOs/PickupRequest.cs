using CarRental.Domain.Enums;

namespace CarRental.Application.DTOs;

public class PickupRequest
{
    public string BookingNumber { get; set; } = string.Empty;
    
    public string RegistrationNumber { get; set; } = string.Empty;
    
    public string CustomerSocialSecurityNumber { get; set; } = string.Empty;
    
    public CarCategory Category { get; set; }
    
    public DateTime PickupDateTime { get; set; }
    
    public int PickupMeterReading { get; set; }
}
