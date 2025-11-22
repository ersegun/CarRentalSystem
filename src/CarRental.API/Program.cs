using CarRental.API.Middleware;
using CarRental.Application.Configuration;
using CarRental.Application.Services;
using CarRental.Domain.Interfaces;
using CarRental.Infrastructure.Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Car Rental API", Version = "v1" });
});

// Configure pricing settings
builder.Services.Configure<PricingConfiguration>(
    builder.Configuration.GetSection("PricingConfiguration"));

// Register application services
builder.Services.AddSingleton<IRentalRepository, InMemoryRentalRepository>();
builder.Services.AddSingleton<IPricingService, PricingService>();
builder.Services.AddScoped<IRentalService, RentalService>();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Always enable Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Car Rental API v1");
    c.RoutePrefix = "swagger"; // Access Swagger at /swagger
});

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Add a simple home page that redirects to Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

// Auto-open browser in development - ONLY SWAGGER
if (app.Environment.IsDevelopment())
{
    var urls = builder.Configuration.GetValue<string>("ASPNETCORE_URLS") ?? "http://localhost:5000";
    var url = urls.Split(';')[0];
    
    Task.Run(async () =>
    {
        await Task.Delay(2000);
        try
        {
            var swaggerUrl = $"{url}/swagger";
            OpenBrowser(swaggerUrl);
            Console.WriteLine($"Opening Swagger UI: {swaggerUrl}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not open browser: {ex.Message}");
        }
    });
}

app.Run();

static void OpenBrowser(string url)
{
    try
    {
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        };
        System.Diagnostics.Process.Start(psi);
    }
    catch
    {
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
        {
            System.Diagnostics.Process.Start("xdg-open", url);
        }
        else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
        {
            System.Diagnostics.Process.Start("open", url);
        }
    }
}