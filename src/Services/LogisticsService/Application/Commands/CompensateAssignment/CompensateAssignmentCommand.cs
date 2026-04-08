// Compensation command — sent by Saga Orchestrator when something fails
// This UNDOES the agent assignment — releases agent and vehicle
using MediatR;

namespace LogisticsService.Application.Commands.CompensateAssignment;

public record CompensateAssignmentCommand : IRequest<CompensateAssignmentResult>
{
    // Which order's assignment to cancel
    public Guid OrderId { get; init; }

    // Why compensation is happening — for logging
    public string Reason { get; init; } = string.Empty;
}

public record CompensateAssignmentResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}
