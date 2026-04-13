// RabbitMQ Consumer — listens for AssignAgentCommand from Saga Orchestrator
// When Payment Service sends this command via RabbitMQ,
// this consumer receives it and triggers the AssignAgentHandler
using LogisticsService.Application.Commands.AssignAgent;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LogisticsService.Application.Consumers;

public class AssignAgentConsumer : IConsumer<Artistic_Sisters.Shared.Events.Payment.AssignAgentCommand>
{
    private readonly IMediator _mediator;
    private readonly ILogger<AssignAgentConsumer> _logger;

    public AssignAgentConsumer(IMediator mediator,
        ILogger<AssignAgentConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // Consume is called automatically when a message arrives on the queue
    public async Task Consume(ConsumeContext<Artistic_Sisters.Shared.Events.Payment.AssignAgentCommand> context)
    {
        _logger.LogInformation(
            "Received AssignAgent command for order {OrderId}",
            context.Message.OrderId);

        // Forward to MediatR handler
        await _mediator.Send(
            new AssignAgentCommand
            {
                OrderId = context.Message.OrderId,
                SLAHours = context.Message.SLAHours,
                CustomerName = context.Message.CustomerName,
                CustomerEmail = context.Message.CustomerEmail,
                CustomerAddress = context.Message.CustomerAddress
            });
    }
}
