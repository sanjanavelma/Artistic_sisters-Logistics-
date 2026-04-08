// Listens for AssignAgentFailedEvent from Logistics Service
// Triggers COMPENSATION — undo payment lock and mark saga failed
using Artistic_Sisters.Shared.Events.Logistics;
using Artistic_Sisters.Shared.Events.Payment;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentService.Domain.Enums;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Application.Consumers;

public class AssignAgentFailedConsumer : IConsumer<AssignAgentFailedEvent>
{
    private readonly PaymentDbContext _db;
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<AssignAgentFailedConsumer> _logger;

    public AssignAgentFailedConsumer(
        PaymentDbContext db,
        IPublishEndpoint publisher,
        ILogger<AssignAgentFailedConsumer> logger)
    {
        _db = db;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AssignAgentFailedEvent> context)
    {
        var evt = context.Message;

        _logger.LogWarning(
            "Agent assignment FAILED for order {OrderId}. Reason: {Reason}. " +
            "Starting compensation.", evt.OrderId, evt.Reason);

        var saga = await _db.DispatchSagas
            .FirstOrDefaultAsync(s => s.OrderId == evt.OrderId);

        if (saga == null) return;

        // Mark saga as compensating
        saga.State = SagaState.Compensating;
        saga.FailureReason = evt.Reason;

        // ── COMPENSATION: Unlock payment ──────────────────────────────────────
        var payment = await _db.Payments
            .FirstOrDefaultAsync(p => p.OrderId == evt.OrderId);

        if (payment != null)
        {
            payment.Status = PaymentStatus.Pending;
            payment.ConfirmedAt = null;
        }

        saga.State = SagaState.Failed;
        saga.CompletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Tell Logistics to release the agent (idempotent — safe to always send)
        await _publisher.Publish(new CompensateAssignmentCommand
        {
            OrderId = evt.OrderId,
            Reason = evt.Reason
        });

        _logger.LogWarning(
            "Compensation complete for order {OrderId}. Saga marked as Failed.",
            evt.OrderId);
    }
}
