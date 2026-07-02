using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AddressValidator.Infrastructure.Persistence;


public sealed class AddressDbContextDesignTimeFactory
    : IDesignTimeDbContextFactory<AddressDbContext>
{
    public AddressDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AddressDbContext>()
            .UseNpgsql("Host=localhost;Database=addresses;Username=addr;Password=addr_secret",
                npgsql => npgsql.UseNetTopologySuite())
            .Options;

        return new AddressDbContext(options);
    }
}
