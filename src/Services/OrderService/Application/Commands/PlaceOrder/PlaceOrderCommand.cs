using MediatR;
using OrderService.Domain.Enums;
namespace OrderService.Application.Commands.PlaceOrder;

public record PlaceOrderCommand : IRequest<PlaceOrderResult>
{
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;
    public string ShippingAddress { get; init; } = string.Empty;
    public OrderType Type { get; init; }
    public PaymentMode PaymentMode { get; init; }
    public List<PlaceOrderItem> Items { get; init; } = new();
}
public record PlaceOrderItem
{
    public Guid ArtworkId { get; init; }
    public string ArtworkTitle { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}
public record PlaceOrderResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public Guid? OrderId { get; init; }
}
