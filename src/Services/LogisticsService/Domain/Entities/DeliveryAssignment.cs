// Links an order to an agent and vehicle
// Created when logistics manager assigns an agent
// This is the core entity of Logistics Service
using LogisticsService.Domain.Enums;

namespace LogisticsService.Domain.Entities;

public class DeliveryAssignment
{
    // Unique ID for this assignment
    public Guid Id { get; set; }

    // Which order this assignment is for
    // Links to OrderService — but NO direct DB join
    public Guid OrderId { get; set; }

    // Which agent is handling this delivery
    public Guid AgentId { get; set; }

    // Which vehicle is being used
    public Guid VehicleId { get; set; }

    // Current delivery status
    public DeliveryStatus Status { get; set; } = DeliveryStatus.PickedUp;

    // When agent was assigned
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Deadline by which delivery must complete
    // Used by Hangfire SLA monitor
    public DateTime SLADeadline { get; set; }

    // Was this assignment rolled back by Saga compensation?
    // true = compensation ran, agent and vehicle were released
    public bool IsCompensated { get; set; } = false;

    // Last known GPS coordinates — updated by agent
    public double? LastLatitude { get; set; }
    public double? LastLongitude { get; set; }

    // When GPS was last updated
    public DateTime? LastGPSUpdate { get; set; }

    // Navigation properties — EF Core loads these with .Include()
    public DeliveryAgent Agent { get; set; } = null!;
    public Vehicle Vehicle { get; set; } = null!;

    // Factory method — creates a new assignment
    // SLAHours = how many hours delivery must complete in
    public static DeliveryAssignment Create(
        Guid orderId, Guid agentId, Guid vehicleId, int slaHours = 24)
    {
        return new DeliveryAssignment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            AgentId = agentId,
            VehicleId = vehicleId,
            Status = DeliveryStatus.PickedUp,
            AssignedAt = DateTime.UtcNow,
            // SLA deadline = now + slaHours
            SLADeadline = DateTime.UtcNow.AddHours(slaHours),
            IsCompensated = false
        };
    }

    // Calculate how much SLA time is remaining as a percentage
    // Used by Hangfire job to detect at-risk deliveries
    public double GetSLARemainingPercent()
    {
        // Total window in minutes
        var totalMinutes = (SLADeadline - AssignedAt).TotalMinutes;

        // Remaining minutes
        var remainingMinutes = (SLADeadline - DateTime.UtcNow).TotalMinutes;

        // If already past deadline return 0
        if (remainingMinutes <= 0) return 0;

        // Return percentage remaining
        return (remainingMinutes / totalMinutes) * 100;
    }

    // Mark assignment as cancelled — called by compensation handler
    public void Compensate()
    {
        IsCompensated = true;
        Status = DeliveryStatus.Cancelled;
    }
}
