using MediatR;
using OrderService.Domain.Enums;
using OrderService.Application.Queries.GetCustomerOrders;
using System.Collections.Generic;
using System;

namespace OrderService.Application.Queries.GetAllOrders;

// Extension of CustomerOrderDto with CustomerId for admin view
public record AdminOrderDto : CustomerOrderDto
{
    public Guid CustomerId { get; init; }
}

public record GetAllOrdersQuery : IRequest<List<AdminOrderDto>>;
