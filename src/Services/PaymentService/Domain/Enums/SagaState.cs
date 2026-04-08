namespace PaymentService.Domain.Enums;

// Tracks which step of the Dispatch Saga we are in
public enum SagaState
{
    // Saga just started
    Started = 1,

    // Payment locked successfully
    PaymentLocked = 2,

    // Agent assigned successfully — Saga complete
    AgentAssigned = 3,

    // Something failed — compensation running
    Compensating = 4,

    // Saga completed successfully
    Completed = 5,

    // Saga failed — all compensation done
    Failed = 6
}
