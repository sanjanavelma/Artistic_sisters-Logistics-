using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;
using OrderService.Application.Queries.GetCustomerOrders;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OrderService.Application.Queries.GetOrderById;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, CustomerOrderDto?>
{
    private readonly OrderDbContext _db;

    public GetOrderByIdHandler(OrderDbContext db)
    {
        _db = db;
    }

    public async Task<CustomerOrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null) return null;

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

        if (order is CustomCommissionOrder customOrder)
        {
            dto = dto with { ArtworkType = customOrder.ArtworkType, Medium = customOrder.Medium };
        }

        return dto;
    }
}
