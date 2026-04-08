// Represents a delivery agent in our system
// Each agent can handle one delivery at a time
using LogisticsService.Domain.Enums;

namespace LogisticsService.Domain.Entities;

public class DeliveryAgent
{
    // Unique ID for this agent
    public Guid Id { get; set; }

    // Agent's full name
    public string Name { get; set; } = string.Empty;

    // Contact phone number — shown to dealer after dispatch
    public string Phone { get; set; } = string.Empty;

    // Email for internal communication
    public string Email { get; set; } = string.Empty;

    // Current availability status
    public AgentStatus Status { get; set; } = AgentStatus.Available;

    // Which order the agent is currently delivering
    // null = no active delivery
    public Guid? CurrentOrderId { get; set; }

    // Is this agent active in the system
    public bool IsActive { get; set; } = true;

    // When agent was added to system
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Check if agent can take a new order
    public bool IsAvailable() => Status == AgentStatus.Available && IsActive;

    // Assign agent to an order — marks them as busy
    public void AssignToOrder(Guid orderId)
    {
        // Safety check — should not assign if already on delivery
        if (!IsAvailable())
            throw new InvalidOperationException(
                $"Agent {Name} is not available for assignment");

        Status = AgentStatus.OnDelivery;
        CurrentOrderId = orderId;
    }

    // Release agent after delivery or compensation
    // Makes them available for next order
    public void Release()
    {
        Status = AgentStatus.Available;
        CurrentOrderId = null;
    }
}
