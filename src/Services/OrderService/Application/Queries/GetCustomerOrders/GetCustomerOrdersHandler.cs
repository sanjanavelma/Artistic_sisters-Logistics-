using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OrderService.Application.Queries.GetCustomerOrders;

public class GetCustomerOrdersHandler : IRequestHandler<GetCustomerOrdersQuery, List<CustomerOrderDto>>
{
    private readonly OrderDbContext _db;

    public GetCustomerOrdersHandler(OrderDbContext db)
    {
        _db = db;
    }

    public async Task<List<CustomerOrderDto>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _db.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == request.CustomerId)
            .OrderByDescending(o => o.PlacedAt)
            .ToListAsync(cancellationToken);

        var result = new List<CustomerOrderDto>();

        foreach (var order in orders)
        {
            var dto = new CustomerOrderDto
            {
                Id = order.Id,
                Type = order.Type,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                PaymentMode = order.PaymentMode,
                PlacedAt = order.PlacedAt,
                Items = order.Items.Select(i => new CustomerOrderItemDto
                {
                    ArtworkId = i.ArtworkId,
                    ArtworkTitle = i.ArtworkTitle,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            // Custom commission fields if applicable
            if (order is CustomCommissionOrder customOrder)
            {
                dto = dto with { ArtworkType = customOrder.ArtworkType, Medium = customOrder.Medium };
            }

            result.Add(dto);
        }

        return result;
    }
}
