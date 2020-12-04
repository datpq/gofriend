using goFriend.Services.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace goFriend.Services
{
    public static class Startup
    {
        public static void ConfigureServices(IServiceCollection services, string connectionString, bool initData = false)
        {
            services.AddDbContext<FriendDbContext>(options =>
                options.UseSqlServer(connectionString, x => x.UseNetTopologySuite().MigrationsAssembly("goFriend.WebApi")));
            //.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)

            if (initData)
            {
                var sp = services.BuildServiceProvider();
                var dbContext = sp.GetRequiredService<FriendDbContext>();
                DbInitializer.Initialize(dbContext);
            }

            services.AddOptions<AppSettings>().Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection("AppSettings").Bind(settings);
            });
            //services.Configure<AppSettings>(config.GetSection("AppSettings"));
            services.AddScoped<DbContext, FriendDbContext>(); // same http session requests have the same object
            services.AddScoped<IDataRepository, DataRepository>();
            services.AddSingleton<ICacheService, CacheService>(); // all requests have the same object
        }
    }
}
