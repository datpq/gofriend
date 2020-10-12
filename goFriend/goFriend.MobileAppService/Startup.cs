using System.Threading.Tasks;
using goFriend.MobileAppService.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NetTopologySuite.Geometries;
using NLog;

namespace goFriend.MobileAppService
{
    public class Startup
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Services.Startup.ConfigureServices(services, Configuration.GetConnectionString("GoFriendConnection"));
            //services.AddAuthentication().AddFacebook(facebookOptions =>
            //{
            //    facebookOptions.AppId = Configuration["Authentication:Facebook:AppId"];
            //    facebookOptions.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
            //});


            //Error Operation does not support GeometryCollection arguments ==> ignore Point in validation
            var validator = new SuppressChildValidationMetadataProvider(typeof(Point));
            services.AddMvc(options => options.ModelMetadataDetailsProviders.Add(validator));
            //services.AddMvc();


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "GoFriend API", Version = "v1" });
            });

            //SignalR
            var azureSignalRConnectionString = Configuration.GetSection("AppSettings")["AzureSignalRConnectionString"];
            //Logger.Debug($"azureSignalRConnectionString = {azureSignalRConnectionString}");
            services.AddSignalR().AddAzureSignalR(azureSignalRConnectionString);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime appLifetime)
        {
            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug();

            appLifetime.ApplicationStopping.Register(OnShutdown);

            app.UseMvc();
            //SignalR
            app.UseFileServer();
            app.UseAzureSignalR(routes =>
            {
                routes.MapHub<ChatHub>("/chat");
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "GoFriend API v1");
            });

            app.Run(async (context) => await Task.Run(() => context.Response.Redirect("/swagger")));
        }

        private void OnShutdown()
        {
            Logger.Debug("OnShutdown.BEGIN");
            Logger.Debug("OnShutdown.END");
        }
    }
}
