using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;
using Artistic_Sisters.Shared.Events.Order;
using System.Text.Json;

namespace OrderService.Application.Commands.PlaceOrder;

public class PlaceOrderHandler : IRequestHandler<PlaceOrderCommand, PlaceOrderResult>
{
    private readonly OrderDbContext _db;
    private readonly IPublishEndpoint _publisher;
    public PlaceOrderHandler(OrderDbContext db, IPublishEndpoint publisher)
    { _db = db; _publisher = publisher; }
    public async Task<PlaceOrderResult> Handle(PlaceOrderCommand request, CancellationToken ct)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Type = request.Type,
            Status = OrderService.Domain.Enums.OrderStatus.Pending,
            PaymentMode = request.PaymentMode,
            PlacedAt = DateTime.UtcNow,
            TotalAmount = request.Items.Sum(i => i.Quantity * i.UnitPrice)
        };
        foreach (var it in request.Items)
        {
            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(), OrderId = order.Id,
                ArtworkId = it.ArtworkId, ArtworkTitle = it.ArtworkTitle,
                Quantity = it.Quantity, UnitPrice = it.UnitPrice
            });
        }
        _db.Orders.Add(order);
        // add outbox message
        var evt = new OrderPlacedEvent
        {
            OrderId = order.Id, CustomerId = order.CustomerId,
            OrderType = request.Type.ToString(), TotalAmount = order.TotalAmount,
            PlacedAt = order.PlacedAt,
            Items = order.Items.Select(i => new OrderItemDto
            {
                ArtworkId = i.ArtworkId, ArtworkTitle = i.ArtworkTitle,
                Quantity = i.Quantity, UnitPrice = i.UnitPrice
            }).ToList()
        };
        _db.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(), Type = nameof(OrderPlacedEvent),
            Payload = JsonSerializer.Serialize(evt), OccurredAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
        // try publish immediately (best effort)
        await _publisher.Publish(evt);
        return new PlaceOrderResult { Success = true, Message = "Order placed successfully", OrderId = order.Id };
    }
}
