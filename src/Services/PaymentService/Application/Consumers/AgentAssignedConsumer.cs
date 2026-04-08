// Listens for AgentAssignedEvent from Logistics Service
// Saga step succeeded — mark saga complete and publish DispatchConfirmedEvent
using Artistic_Sisters.Shared.Events.Logistics;
using Artistic_Sisters.Shared.Events.Payment;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentService.Domain.Enums;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Application.Consumers;

public class AgentAssignedConsumer : IConsumer<AgentAssignedEvent>
{
    private readonly PaymentDbContext _db;
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<AgentAssignedConsumer> _logger;

    public AgentAssignedConsumer(
        PaymentDbContext db,
        IPublishEndpoint publisher,
        ILogger<AgentAssignedConsumer> logger)
    {
        _db = db;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AgentAssignedEvent> context)
    {
        var evt = context.Message;

        var saga = await _db.DispatchSagas
            .FirstOrDefaultAsync(s => s.OrderId == evt.OrderId);

        if (saga == null) return;

        // Mark saga as complete
        saga.AgentAssigned = true;
        saga.State = SagaState.Completed;
        saga.CompletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // ── SAGA COMPLETE — Notify customer via Notification Service ──────────
        await _publisher.Publish(new DispatchConfirmedEvent
        {
            OrderId = evt.OrderId,
            CustomerId = saga.CustomerId,
            AgentName = evt.AgentName,
            AgentPhone = evt.AgentPhone,
            VehicleNumber = evt.VehicleNumber,
            DispatchedAt = DateTime.UtcNow
        });

        _logger.LogInformation(
            "Dispatch Saga COMPLETED for order {OrderId}. Agent: {AgentName}",
            evt.OrderId, evt.AgentName);
    }
}
