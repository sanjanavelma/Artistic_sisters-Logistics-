using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderService.Infrastructure.Persistence;

public class OrderDbContextFactory : IDesignTimeDbContextFactory<OrderDbContext>
{
    public OrderDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<OrderDbContext>();
        var conn = "Server=localhost\\SQLEXPRESS;Database=OrderDB;Trusted_Connection=True;TrustServerCertificate=True;";
        builder.UseSqlServer(conn);
        return new OrderDbContext(builder.Options);
    }
}
