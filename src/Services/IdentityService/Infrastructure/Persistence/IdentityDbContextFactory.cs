// This class helps EF Core find the DbContext at design time
// Only used for migrations — not used at runtime
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IdentityService.Infrastructure.Persistence;

public class IdentityDbContextFactory
    : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        // Build configuration to read appsettings.json
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder =
            new DbContextOptionsBuilder<IdentityDbContext>();

        // Use the connection string from appsettings.json
        optionsBuilder.UseSqlServer(
            config.GetConnectionString("IdentityDB"));

        return new IdentityDbContext(optionsBuilder.Options);
    }
}