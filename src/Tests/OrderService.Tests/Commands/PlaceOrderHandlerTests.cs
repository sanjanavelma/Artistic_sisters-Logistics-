using Artistic_Sisters.Shared.Events.Order;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using OrderService.Application.Commands.PlaceOrder;
using OrderService.Domain.Enums;
using OrderService.Infrastructure.Persistence;
using Xunit;

namespace OrderService.Tests.Commands;

/// <summary>
/// Unit tests for <see cref="PlaceOrderHandler"/>.
/// Uses EF Core InMemory – no real database required.
/// </summary>
public class PlaceOrderHandlerTests : IDisposable
{
    private readonly OrderDbContext _db;
    private readonly Mock<IPublishEndpoint> _publisherMock;

    public PlaceOrderHandlerTests()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db           = new OrderDbContext(options);
        _publisherMock = new Mock<IPublishEndpoint>();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helper: a valid command with the given items list
    // ─────────────────────────────────────────────────────────────────────────
    private static PlaceOrderCommand BuildCommand(List<PlaceOrderItem>? items = null) => new()
    {
        CustomerId      = Guid.NewGuid(),
        CustomerName    = "Sanjana Velma",
        CustomerEmail   = "sanjana@test.com",
        ShippingAddress = "Hyderabad, India",
        Type            = OrderType.ReadyMade,
        PaymentMode     = PaymentMode.Card,
        Items           = items ?? new List<PlaceOrderItem>
        {
            new() { ArtworkId = Guid.NewGuid(), ArtworkTitle = "Floral Canvas", Quantity = 2, UnitPrice = 500 }
        }
    };

    // ─────────────────────────────────────────────────────────────────────────
    // Test 1 – Valid order → success result returned
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_ValidOrder_ReturnsSuccess()
    {
        // Arrange
        var handler = new PlaceOrderHandler(_db, _publisherMock.Object);

        // Act
        var result = await handler.Handle(BuildCommand(), CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Order placed successfully", result.Message);
        Assert.NotNull(result.OrderId);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 2 – Order is persisted in the database
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_ValidOrder_PersistsOrderAndItems()
    {
        // Arrange
        var handler = new PlaceOrderHandler(_db, _publisherMock.Object);

        // Act
        await handler.Handle(BuildCommand(), CancellationToken.None);

        // Assert
        var orders = await _db.Orders.Include(o => o.Items).ToListAsync();
        Assert.Single(orders);
        Assert.Single(orders[0].Items);
        Assert.Equal("Floral Canvas", orders[0].Items[0].ArtworkTitle);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 3 – TotalAmount is calculated correctly (Quantity × UnitPrice)
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_ValidOrder_CalculatesTotalAmountCorrectly()
    {
        // Arrange
        var items = new List<PlaceOrderItem>
        {
            new() { ArtworkId = Guid.NewGuid(), ArtworkTitle = "Art A", Quantity = 3, UnitPrice = 200 },
            new() { ArtworkId = Guid.NewGuid(), ArtworkTitle = "Art B", Quantity = 1, UnitPrice = 100 }
        };
        var handler = new PlaceOrderHandler(_db, _publisherMock.Object);

        // Act
        await handler.Handle(BuildCommand(items), CancellationToken.None);

        // Assert: 3×200 + 1×100 = 700
        var order = await _db.Orders.FirstAsync();
        Assert.Equal(700m, order.TotalAmount);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 4 – Empty items list → DomainException thrown
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_EmptyItems_ThrowsDomainException()
    {
        // Arrange
        var handler = new PlaceOrderHandler(_db, _publisherMock.Object);
        var command = BuildCommand(new List<PlaceOrderItem>());   // empty

        // Act & Assert
        await Assert.ThrowsAsync<Artistic_Sisters.Shared.Exceptions.DomainException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 5 – OrderPlacedEvent is published after placing an order
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_ValidOrder_PublishesOrderPlacedEvent()
    {
        // Arrange
        var handler = new PlaceOrderHandler(_db, _publisherMock.Object);

        // Act
        await handler.Handle(BuildCommand(), CancellationToken.None);

        // Assert
        _publisherMock.Verify(
            p => p.Publish(It.IsAny<OrderPlacedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 6 – New order status is always set to Confirmed
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_ValidOrder_SetsStatusToConfirmed()
    {
        // Arrange
        var handler = new PlaceOrderHandler(_db, _publisherMock.Object);

        // Act
        await handler.Handle(BuildCommand(), CancellationToken.None);

        // Assert
        var order = await _db.Orders.FirstAsync();
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 7 – An outbox message is written for reliable delivery
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_ValidOrder_WritesOutboxMessage()
    {
        // Arrange
        var handler = new PlaceOrderHandler(_db, _publisherMock.Object);

        // Act
        await handler.Handle(BuildCommand(), CancellationToken.None);

        // Assert
        var outbox = await _db.OutboxMessages.ToListAsync();
        Assert.Single(outbox);
        Assert.Equal("OrderPlacedEvent", outbox[0].Type);
    }

    public void Dispose() => _db.Dispose();
}
