namespace CarRental.Application.DTOs;

public class ReturnRequest
{
    public string BookingNumber { get; set; } = string.Empty;
    
    public DateTime ReturnDateTime { get; set; }
    
    public int ReturnMeterReading { get; set; }
}
