namespace CarRental.Application.Exceptions;

public class RentalNotFoundException(string bookingNumber)
    : Exception($"Rental with booking number '{bookingNumber}' was not found.");

public class RentalValidationException(string message) : Exception(message);

public class DuplicateBookingException(string bookingNumber)
    : Exception($"A rental with booking number '{bookingNumber}' already exists.");
