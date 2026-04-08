// Tracks the state of the Dispatch Saga for each order
// Updated as each step completes or fails
using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities;

public class DispatchSaga
{
    public Guid Id { get; set; }

    // Which order this Saga is for
    public Guid OrderId { get; set; }

    // Which customer — needed for notifications
    public Guid CustomerId { get; set; }

    // Current state of the Saga
    public SagaState State { get; set; } = SagaState.Started;

    // When Saga started
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    // When Saga completed or failed
    public DateTime? CompletedAt { get; set; }

    // Why it failed — for logging and debugging
    public string? FailureReason { get; set; }

    // Track which steps completed — helps with compensation
    public bool PaymentLocked { get; set; } = false;
    public bool AgentAssigned { get; set; } = false;
}
