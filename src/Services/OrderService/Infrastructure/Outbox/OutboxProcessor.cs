using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Persistence;
using System.Text.Json;

namespace OrderService.Infrastructure.Outbox;

public class OutboxProcessor
{
    private readonly OrderDbContext _db;
    private readonly IPublishEndpoint _publisher;
    public OutboxProcessor(OrderDbContext db, IPublishEndpoint publisher)
    { _db = db; _publisher = publisher; }
    public async Task ProcessPendingAsync(CancellationToken ct = default)
    {
        var items = await _db.OutboxMessages
            .Where(o => !o.Processed).ToListAsync(ct);
        foreach (var item in items)
        {
            // simple payload dispatch by type
            try
            {
                switch (item.Type)
                {
                    case "OrderPlacedEvent":
                        var op = JsonSerializer.Deserialize<dynamic>(item.Payload);
                        await _publisher.Publish(op);
                        break;
                    default:
                        break;
                }
                item.Processed = true;
            }
            catch { /* log and continue */ }
        }
        await _db.SaveChangesAsync(ct);
    }
}
