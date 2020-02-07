using System.Threading.Tasks;
using goFriend.MobileAppService.Data;
using goFriend.MobileAppService.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NetTopologySuite.Geometries;

namespace goFriend.MobileAppService
{
    public class Startup
    {
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
            services.AddDbContext<FriendDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("GoFriendConnection"), x => x.UseNetTopologySuite()));
            //services.AddAuthentication().AddFacebook(facebookOptions =>
            //{
            //    facebookOptions.AppId = Configuration["Authentication:Facebook:AppId"];
            //    facebookOptions.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
            //});

            var sp = services.BuildServiceProvider();
            var dbContext = sp.GetRequiredService<FriendDbContext>();
            DbInitializer.Initialize(dbContext);

            //Error Operation does not support GeometryCollection arguments ==> ignore Point in validation
            var validator = new SuppressChildValidationMetadataProvider(typeof(Point));
            services.AddMvc(options => options.ModelMetadataDetailsProviders.Add(validator));
            //services.AddMvc();
            services.Configure<AppSettingsModel>(Configuration.GetSection("AppSettings"));
            services.AddScoped<DbContext, FriendDbContext>(); // same http session requests have the same object
            services.AddScoped<IDataRepository, DataRepository>();
            services.AddSingleton<ICacheService, CacheService>(); // all requests have the same object

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "GoFriend API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "GoFriend API v1");
            });

            app.Run(async (context) => await Task.Run(() => context.Response.Redirect("/swagger")));
        }
    }
}
