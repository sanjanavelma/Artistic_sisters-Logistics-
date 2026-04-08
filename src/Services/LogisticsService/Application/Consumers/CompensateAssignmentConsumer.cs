// Listens for CompensateAssignmentCommand from Saga Orchestrator
// When Payment Service detects a failure, it sends this command
// to trigger rollback of the agent assignment
using LogisticsService.Application.Commands.CompensateAssignment;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LogisticsService.Application.Consumers;

public class CompensateAssignmentConsumer
    : IConsumer<Artistic_Sisters.Shared.Events.Payment.CompensateAssignmentCommand>
{
    private readonly IMediator _mediator;
    private readonly ILogger<CompensateAssignmentConsumer> _logger;

    public CompensateAssignmentConsumer(IMediator mediator,
        ILogger<CompensateAssignmentConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(
        ConsumeContext<Artistic_Sisters.Shared.Events.Payment.CompensateAssignmentCommand> context)
    {
        _logger.LogInformation(
            "Received compensation command for order {OrderId}",
            context.Message.OrderId);

        await _mediator.Send(new CompensateAssignmentCommand
        {
            OrderId = context.Message.OrderId,
            Reason = context.Message.Reason
        });
    }
}
