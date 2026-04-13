using MediatR;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;
using OrderService.Application.Commands.PlaceOrder;
using Artistic_Sisters.Shared.Events.Order;
using System.Text.Json;
using MassTransit;

namespace OrderService.Application.Commands.PlaceCommission;

public class PlaceCommissionHandler : IRequestHandler<PlaceCommissionCommand, PlaceOrderResult>
{
    private readonly OrderDbContext _db;
    private readonly IPublishEndpoint _publisher;
    public PlaceCommissionHandler(OrderDbContext db, IPublishEndpoint publisher) { _db = db; _publisher = publisher; }
    public async Task<PlaceOrderResult> Handle(PlaceCommissionCommand request, CancellationToken ct)
    {
        var order = new CustomCommissionOrder
        {
            Id = Guid.NewGuid(), CustomerId = request.CustomerId,
            Type = OrderService.Domain.Enums.OrderType.CustomCommission,
            Status = OrderService.Domain.Enums.OrderStatus.Pending,
            PlacedAt = DateTime.UtcNow,
            TotalAmount = request.BudgetMax,
            ArtworkType = request.ArtworkType,
            Medium = request.Medium,
            Size = request.Size,
            ReferencePhotoUrl = request.ReferencePhotoUrl,
            SpecialInstructions = request.SpecialInstructions
        };
        _db.CustomCommissionOrders.Add(order);
        var evt = new CustomCommissionPlacedEvent
        {
            OrderId = order.Id, CustomerId = order.CustomerId,
            CustomerName = request.CustomerName, CustomerEmail = request.CustomerEmail,
            ArtworkType = request.ArtworkType, Medium = request.Medium,
            Size = request.Size, ReferencePhotoUrl = request.ReferencePhotoUrl,
            SpecialInstructions = request.SpecialInstructions,
            BudgetMin = request.BudgetMin, BudgetMax = request.BudgetMax,
            PlacedAt = order.PlacedAt
        };
        _db.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(), Type = nameof(CustomCommissionPlacedEvent),
            Payload = JsonSerializer.Serialize(evt), OccurredAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
        await _publisher.Publish(evt);
        return new PlaceOrderResult { Success = true, Message = "Custom commission placed successfully", OrderId = order.Id };
    }
}
