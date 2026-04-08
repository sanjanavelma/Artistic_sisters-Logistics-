using MediatR;
using OrderService.Domain.Enums;
using System.Collections.Generic;
using System;

namespace OrderService.Application.Queries.GetCustomerOrders;

public record GetCustomerOrdersQuery(Guid CustomerId) : IRequest<List<CustomerOrderDto>>;

public record CustomerOrderDto
{
    public Guid Id { get; init; }
    public OrderType Type { get; init; }
    public OrderStatus Status { get; init; }
    public decimal TotalAmount { get; init; }
    public PaymentMode PaymentMode { get; init; }
    public DateTime PlacedAt { get; init; }
    
    // For custom commissions
    public string ArtworkType { get; init; } = string.Empty;
    public string Medium { get; init; } = string.Empty;
    
    public List<CustomerOrderItemDto> Items { get; init; } = new();
}

public record CustomerOrderItemDto
{
    public Guid ArtworkId { get; init; }
    public string ArtworkTitle { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}
