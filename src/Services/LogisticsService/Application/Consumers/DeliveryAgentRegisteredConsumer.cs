using Artistic_Sisters.Shared.Events.Identity;
using LogisticsService.Domain.Entities;
using LogisticsService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LogisticsService.Application.Consumers;

public class DeliveryAgentRegisteredConsumer : IConsumer<DeliveryAgentRegisteredEvent>
{
    private readonly LogisticsDbContext _db;
    private readonly ILogger<DeliveryAgentRegisteredConsumer> _logger;

    public DeliveryAgentRegisteredConsumer(LogisticsDbContext db, ILogger<DeliveryAgentRegisteredConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DeliveryAgentRegisteredEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Received DeliveryAgentRegisteredEvent for Agent {AgentId} ({Email})", msg.AgentId, msg.Email);

        // Check if agent already exists (e.g. duplicate events, idempotency)
        var exists = await _db.Agents.FindAsync(msg.AgentId);
        if (exists != null)
        {
            _logger.LogInformation("Agent {AgentId} already exists in LogisticsDB, skipping creation.", msg.AgentId);
            return;
        }

        // Create the Agent record in LogisticsService
        var agent = new DeliveryAgent
        {
            Id = msg.AgentId,
            Name = msg.Name,
            Phone = msg.Phone,
            Email = msg.Email,
            Status = Domain.Enums.AgentStatus.Available,
            IsActive = true,
            CreatedAt = msg.RegisteredAt == default ? DateTime.UtcNow : msg.RegisteredAt
        };

        _db.Agents.Add(agent);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Successfully created Delivery Agent {AgentId} in LogisticsDB", msg.AgentId);
    }
}
