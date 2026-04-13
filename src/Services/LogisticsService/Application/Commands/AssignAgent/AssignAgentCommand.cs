// Command to assign a delivery agent to an order
// Sent by Saga Orchestrator (PaymentService) via RabbitMQ
// OR directly by Logistics Manager via API
using MediatR;

namespace LogisticsService.Application.Commands.AssignAgent;

public record AssignAgentCommand : IRequest<AssignAgentResult>
{
    // Which order needs an agent
    public Guid OrderId { get; init; }

    // Optional — Logistics Manager can pick a specific agent
    // If null — system picks any available agent automatically
    public Guid? PreferredAgentId { get; init; }

    // How many hours the delivery must complete in
    public int SLAHours { get; init; } = 24;

    // Output target for notifications
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerAddress { get; init; } = string.Empty;
}

public record AssignAgentResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public Guid? AssignmentId { get; init; }
    public string? AgentName { get; init; }
    public string? AgentPhone { get; init; }
    public string? VehicleNumber { get; init; }
    public DateTime? SLADeadline { get; init; }

    // If failed — why it failed
    public string? FailureReason { get; init; }
}
