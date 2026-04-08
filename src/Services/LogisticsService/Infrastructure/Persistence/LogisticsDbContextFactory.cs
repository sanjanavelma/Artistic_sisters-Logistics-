using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LogisticsService.Infrastructure.Persistence;

public class LogisticsDbContextFactory : IDesignTimeDbContextFactory<LogisticsDbContext>
{
    public LogisticsDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<LogisticsDbContext>();
        var conn = "Server=localhost\\SQLEXPRESS;Database=LogisticsDB;Trusted_Connection=True;TrustServerCertificate=True;";
        builder.UseSqlServer(conn);
        return new LogisticsDbContext(builder.Options);
    }
}
