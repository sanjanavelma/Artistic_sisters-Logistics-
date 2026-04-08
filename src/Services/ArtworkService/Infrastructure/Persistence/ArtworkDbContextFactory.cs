using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ArtworkService.Infrastructure.Persistence;

public class ArtworkDbContextFactory : IDesignTimeDbContextFactory<ArtworkDbContext>
{
    public ArtworkDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<ArtworkDbContext>();
        // Design-time connection string (matches workspace format)
        var conn = "Server=localhost\\SQLEXPRESS;Database=ArtworkDB;Trusted_Connection=True;TrustServerCertificate=True;";
        builder.UseSqlServer(conn);
        return new ArtworkDbContext(builder.Options);
    }
}
