// Represents a delivery vehicle
// Each vehicle can be assigned to one delivery at a time
namespace LogisticsService.Domain.Entities;

public class Vehicle
{
    // Unique ID for this vehicle
    public Guid Id { get; set; }

    // Vehicle registration number — shown to dealer
    // e.g. "MH 01 AB 1234"
    public string RegistrationNumber { get; set; } = string.Empty;

    // Type of vehicle — "Bike", "Van", "Truck"
    public string VehicleType { get; set; } = string.Empty;

    // Is vehicle currently free to use
    public bool IsAvailable { get; set; } = true;

    // Is vehicle active in the system
    public bool IsActive { get; set; } = true;
}
