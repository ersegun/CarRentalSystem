using CarRental.Domain.Enums;

namespace CarRental.Application.DTOs;

public class RentalResponse
{
    public string BookingNumber { get; set; } = string.Empty;
    
    public string RegistrationNumber { get; set; } = string.Empty;
    
    public string CustomerSocialSecurityNumber { get; set; } = string.Empty;
    
    public CarCategory Category { get; set; }
    
    public DateTime PickupDateTime { get; set; }
    
    public int PickupMeterReading { get; set; }
    
    public DateTime? ReturnDateTime { get; set; }
    
    public int? ReturnMeterReading { get; set; }
    
    public decimal? TotalPrice { get; set; }
    
    public bool IsReturned { get; set; }
}
