using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Application.Queries.GetCustomerOrders;
using OrderService.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OrderService.Application.Queries.GetAllOrders;

public class GetAllOrdersHandler : IRequestHandler<GetAllOrdersQuery, List<AdminOrderDto>>
{
    private readonly OrderDbContext _db;

    public GetAllOrdersHandler(OrderDbContext db)
    {
        _db = db;
    }

    public async Task<List<AdminOrderDto>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _db.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.PlacedAt)
            .ToListAsync(cancellationToken);

        var result = new List<AdminOrderDto>();

        foreach (var order in orders)
        {
            var dto = new AdminOrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
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

            if (order is CustomCommissionOrder customOrder)
            {
                dto = dto with { ArtworkType = customOrder.ArtworkType, Medium = customOrder.Medium };
            }

            result.Add(dto);
        }

        return result;
    }
}
