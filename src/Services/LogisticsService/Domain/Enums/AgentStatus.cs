// Current availability status of a delivery agent
namespace LogisticsService.Domain.Enums;

public enum AgentStatus
{
    // Agent is free and can be assigned to a new order
    Available = 1,

    // Agent is currently delivering an order
    OnDelivery = 2,

    // Agent is not working today
    Offline = 3
}
