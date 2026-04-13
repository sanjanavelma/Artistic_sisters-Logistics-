// THIS IS THE SAGA ORCHESTRATOR ENTRY POINT
// When order is ready to dispatch, this starts the Dispatch Saga:
// Step 1 — Lock payment (mark as Confirmed)
// Step 2 — Send AssignAgentCommand to Logistics Service
using Artistic_Sisters.Shared.Events.Order;
using Artistic_Sisters.Shared.Events.Payment;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Application.Consumers;

public class ReadyForDispatchConsumer : IConsumer<ReadyForDispatchEvent>
{
    private readonly PaymentDbContext _db;
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<ReadyForDispatchConsumer> _logger;

    public ReadyForDispatchConsumer(
        PaymentDbContext db,
        IPublishEndpoint publisher,
        ILogger<ReadyForDispatchConsumer> logger)
    {
        _db = db;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReadyForDispatchEvent> context)
    {
        var evt = context.Message;

        _logger.LogInformation(
            "Starting Dispatch Saga for order {OrderId}", evt.OrderId);

        // Idempotency — don't start duplicate sagas
        var existingSaga = await _db.DispatchSagas
            .FirstOrDefaultAsync(s => s.OrderId == evt.OrderId);
        if (existingSaga != null) return;

        // ── SAGA STEP 1: Lock Payment ─────────────────────────────────────────
        var payment = await _db.Payments
            .FirstOrDefaultAsync(p => p.OrderId == evt.OrderId);

        if (payment == null)
        {
            _logger.LogError(
                "No payment record found for order {OrderId}", evt.OrderId);
            return;
        }

        // Create Saga tracking record
        var saga = new DispatchSaga
        {
            Id = Guid.NewGuid(),
            OrderId = evt.OrderId,
            CustomerId = evt.CustomerId,
            CustomerName = evt.CustomerName,
            CustomerEmail = evt.CustomerEmail,
            CustomerAddress = evt.CustomerAddress,
            State = SagaState.Started,
            StartedAt = DateTime.UtcNow
        };

        // If it's an online payment, it MUST be confirmed already via the Gateway webhook
        if (payment.PaymentMode != "COD" && payment.Status != PaymentStatus.Confirmed)
        {
            _logger.LogError("Cannot dispatch online order {OrderId} because payment is not Confirmed. Status: {Status}", evt.OrderId, payment.Status);
            saga.State = SagaState.Failed;
            saga.FailureReason = "Payment not successful via Gateway";
            _db.DispatchSagas.Add(saga);
            await _db.SaveChangesAsync();
            return;
        }

        // Lock payment — confirmed for this order
        payment.ConfirmedAt = DateTime.UtcNow;
        if (payment.PaymentMode == "COD")
        {
            // For COD, we can keep the status as Pending mathematically, but the saga allows it to pass.
        }

        saga.PaymentLocked = true;
        saga.State = SagaState.PaymentLocked;

        _db.DispatchSagas.Add(saga);
        await _db.SaveChangesAsync();

        // ── SAGA STEP 2: Send AssignAgentCommand to Logistics Service ─────────
        await _publisher.Publish(new AssignAgentCommand
        {
            OrderId = evt.OrderId,
            SLAHours = evt.SLAHours,
            CustomerName = saga.CustomerName,
            CustomerEmail = saga.CustomerEmail,
            CustomerAddress = saga.CustomerAddress
        });

        _logger.LogInformation(
            "Saga Step 1 complete — payment locked for order {OrderId}. " +
            "Waiting for agent assignment.", evt.OrderId);
    }
}
