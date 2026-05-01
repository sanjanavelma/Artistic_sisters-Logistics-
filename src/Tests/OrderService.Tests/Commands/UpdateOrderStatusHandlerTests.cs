using Artistic_Sisters.Shared.Events.Order;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OrderService.Application.Commands.UpdateOrderStatus;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Infrastructure.Persistence;
using System.Text.Json;
using Xunit;

namespace OrderService.Tests.Commands;

/// <summary>
/// Unit tests for <see cref="UpdateOrderStatusHandler"/>.
/// </summary>
public class UpdateOrderStatusHandlerTests : IDisposable
{
    private readonly OrderDbContext _db;
    private readonly Mock<IPublishEndpoint> _publisherMock;
    private readonly IConfiguration _config;

    public UpdateOrderStatusHandlerTests()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db           = new OrderDbContext(options);
        _publisherMock = new Mock<IPublishEndpoint>();

        var configData = new Dictionary<string, string?>
        {
            ["Email:ArtistEmail"] = "admin@artisticsisters.com"
        };
        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helper: seed an order and an outbox entry for it
    // ─────────────────────────────────────────────────────────────────────────
    private async Task<Order> SeedOrderAsync(OrderStatus initialStatus = OrderStatus.Confirmed)
    {
        var order = new Order
        {
            Id        = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Type      = OrderType.ReadyMade,
            Status    = initialStatus,
            PaymentMode = PaymentMode.Card,
            TotalAmount = 500
        };
        _db.Orders.Add(order);

        // Add a matching outbox message so the handler can find customer info
        var evt = new OrderPlacedEvent
        {
            OrderId       = order.Id,
            CustomerId    = order.CustomerId,
            CustomerName  = "Sanjana Velma",
            CustomerEmail = "sanjana@test.com",
            ShippingAddress = "Hyderabad",
            OrderType     = "Standard",
            TotalAmount   = 500,
            PaymentMode   = "Online",
            PlacedAt      = DateTime.UtcNow,
            Items         = new()
        };
        _db.OutboxMessages.Add(new OutboxMessage
        {
            Id         = Guid.NewGuid(),
            Type       = nameof(OrderPlacedEvent),
            Payload    = JsonSerializer.Serialize(evt),
            OccurredAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return order;
    }

    private UpdateOrderStatusHandler BuildHandler() =>
        new(_db, _publisherMock.Object, _config, NullLogger<UpdateOrderStatusHandler>.Instance);

    // ─────────────────────────────────────────────────────────────────────────
    // Test 1 – Order not found → returns failure
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_OrderNotFound_ReturnsFailure()
    {
        // Arrange
        var handler = BuildHandler();
        var command = new UpdateOrderStatusCommand { OrderId = Guid.NewGuid(), Status = OrderStatus.Delivered };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Order not found", result.Message);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 2 – Valid update → order status is persisted in DB
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_ValidUpdate_PersistsNewStatusInDatabase()
    {
        // Arrange
        var order   = await SeedOrderAsync(OrderStatus.Confirmed);
        var handler = BuildHandler();
        var command = new UpdateOrderStatusCommand { OrderId = order.Id, Status = OrderStatus.InProduction };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _db.Orders.FindAsync(order.Id);
        Assert.Equal(OrderStatus.InProduction, updated!.Status);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 3 – Valid update → success result returned
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_ValidUpdate_ReturnsSuccess()
    {
        // Arrange
        var order   = await SeedOrderAsync();
        var handler = BuildHandler();
        var command = new UpdateOrderStatusCommand { OrderId = order.Id, Status = OrderStatus.InProduction };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Contains("InProduction", result.Message);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 4 – Any status update publishes OrderStatusChangedEvent
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_ValidUpdate_PublishesOrderStatusChangedEvent()
    {
        // Arrange
        var order   = await SeedOrderAsync();
        var handler = BuildHandler();
        var command = new UpdateOrderStatusCommand { OrderId = order.Id, Status = OrderStatus.Delivered };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _publisherMock.Verify(
            p => p.Publish(It.IsAny<OrderStatusChangedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 5 – Transitioning to ReadyForDelivery also publishes ReadyForDispatchEvent
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_StatusChangedToReadyForDelivery_PublishesDispatchEvent()
    {
        // Arrange: start from InProduction so the transition fires
        var order   = await SeedOrderAsync(OrderStatus.InProduction);
        var handler = BuildHandler();
        var command = new UpdateOrderStatusCommand { OrderId = order.Id, Status = OrderStatus.ReadyForDelivery };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert – two Publish calls: ReadyForDispatchEvent + OrderStatusChangedEvent
        _publisherMock.Verify(
            p => p.Publish(It.IsAny<ReadyForDispatchEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _publisherMock.Verify(
            p => p.Publish(It.IsAny<OrderStatusChangedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 6 – Already ReadyForDelivery → no second dispatch event fired
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_AlreadyReadyForDelivery_DoesNotPublishDispatchEvent()
    {
        // Arrange: order already at ReadyForDelivery
        var order   = await SeedOrderAsync(OrderStatus.ReadyForDelivery);
        var handler = BuildHandler();
        var command = new UpdateOrderStatusCommand { OrderId = order.Id, Status = OrderStatus.ReadyForDelivery };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert: dispatch event should NOT be published again
        _publisherMock.Verify(
            p => p.Publish(It.IsAny<ReadyForDispatchEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    public void Dispose() => _db.Dispose();
}
