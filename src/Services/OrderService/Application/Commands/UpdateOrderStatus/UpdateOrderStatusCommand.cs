using MediatR;
using OrderService.Domain.Enums;

namespace OrderService.Application.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommand : IRequest<UpdateOrderStatusResult>
{
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
}

public class UpdateOrderStatusResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
