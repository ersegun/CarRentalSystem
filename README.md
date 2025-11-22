# Car Rental System

A simple and clean architecture .NET 8.0 REST API for managing car rentals with automatic price calculation.

## Features

- **Pickup & Return Management** - Track car pickups and returns with automatic pricing
- **Pricing Engine** - Calculates rental costs based on days and kilometers driven
- **In-Memory Storage** - Simple data persistence (easily replaceable with database)
- **Swagger UI** - Interactive API documentation
- **Health Checks** - Monitor service health
- **Docker Support** - Containerized deployment

## Tech Stack

- .NET 8.0
- ASP.NET Core Web API
- Serilog for logging
- xUnit for testing
- Docker & Docker Compose

## Quick Start

### Using Docker Compose (Recommended)

```bash
docker-compose up -d
```

Access the API at: **http://localhost:5000/swagger**

Stop the container:
```bash
docker-compose down
```

### Using Docker

```bash
docker build -t car-rental-api .
docker run -p 5000:8080 car-rental-api
```

### Using .NET CLI

```bash
cd src/CarRental.API
dotnet run
```

The API automatically opens Swagger UI at **http://localhost:5000/swagger**

## Running Tests

```bash
cd tests/CarRental.Tests
dotnet test
```

## Key Assumptions

- Days calculated as whole days (25 hours = 2 days)
- Return meter reading must be ≥ pickup meter reading
- Return datetime must be ≥ pickup datetime
- Booking numbers are unique system-wide
- Prices in SEK (Swedish Krona)


