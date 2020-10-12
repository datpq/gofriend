using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(goFriend.Functions.Startup))]

namespace goFriend.Functions
{
    public class Startup : FunctionsStartup
    {
        //private ILoggerFactory _loggerFactory;

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();

            var config = new ConfigurationBuilder().AddJsonFile("local.settings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables().Build();
            builder.Services.AddLogging();

            Services.Startup.ConfigureServices(builder.Services, config.GetConnectionString("GoFriendConnection"));

            //_loggerFactory = new LoggerFactory();
            //var logger = _loggerFactory.CreateLogger("Startup");
            //var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            //var logger = loggerFactory.CreateLogger<Startup>();
            //logger.LogInformation("Got Here in Startup");
        }
    }
}
