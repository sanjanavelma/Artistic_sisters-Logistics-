using MediatR;
using System;
using OrderService.Application.Queries.GetCustomerOrders;

namespace OrderService.Application.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<CustomerOrderDto?>;
