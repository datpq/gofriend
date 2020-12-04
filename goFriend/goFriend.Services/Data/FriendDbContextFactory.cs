using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace goFriend.Services.Data
{
    public class FriendDbContextFactory : IDesignTimeDbContextFactory<FriendDbContext>
    {
        public FriendDbContext CreateDbContext(string[] args)
        {
            var builder = new ConfigurationBuilder()
                //.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.Production.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            var optionsBuilder = new DbContextOptionsBuilder<FriendDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("GoFriendConnection"),
                x => x.UseNetTopologySuite().MigrationsAssembly("goFriend.WebApi"));

            return new FriendDbContext(optionsBuilder.Options);
        }
    }
}

