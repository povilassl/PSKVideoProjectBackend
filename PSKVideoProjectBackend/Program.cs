using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PSKVideoProjectBackend;
using PSKVideoProjectBackend.Repositories;
using System.Diagnostics;
using System.Reflection;
using log4net;
using log4net.Config;
using PSKVideoProjectBackend.Hubs;
using PSKVideoProjectBackend.Helpers;
using PSKVideoProjectBackend.Middleware;
using PSKVideoProjectBackend.Managers;
using PSKVideoProjectBackend.Factories;

internal class Program
{
    private static readonly ILog _log = LogManager.GetLogger(typeof(Program));

    private static void Main(string[] args)
    {
        var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
        XmlConfigurator.ConfigureAndWatch(logRepository, new FileInfo("log4net.config"));
        _log.Info("Application starting");

        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers()
            .AddNewtonsoftJson(options => {
                options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.RoundtripKind;
                options.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
            });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c => {
            c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
            $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
        });

        string[] origins = { "http://localhost:3000", "https://localhost:3000", "https://videoteka.tech" };

        string domain = isDevelopment ? "localhost" : ".videoteka.tech";

        builder.Services.AddCors(options => {
            options.AddDefaultPolicy(
                builder => {
                    builder.WithOrigins(origins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
        });

        builder.Logging.AddLog4Net();

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options => {
                options.LoginPath = new PathString("/UserInteractionsController/Login");
                options.Cookie.Name = "VideotekaAuthentication";
                options.ExpireTimeSpan = TimeSpan.FromDays(10);
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.Path = "/";
                options.Cookie.HttpOnly = false;
                options.Cookie.Domain = domain;
            });

        builder.Services.AddAuthorization(options => {
            options.AddPolicy("Public", policy => {
                policy.RequireAuthenticatedUser();
            });
        });

        builder.Services.AddSignalR();

        string dataSource = isDevelopment
            ? "Data source=DB/ProjectDatabase.db"
            : "Data source=C:/home/site/wwwroot/ProjectDatabase.db";

        builder.Services.AddDbContext<ApiDbContext>(o => o.UseSqlite(dataSource));

        //repositories
        builder.Services.AddScoped<VideoRepository>();
        builder.Services.AddScoped<UserRepository>();

        //Singletons
        builder.Services.AddSingleton<SignalRConnectionMapping>();
        builder.Services.AddSingleton<SignalRManager>();

        //Downside is that InitManager() fires upon first request to the server
        builder.Services.AddSingleton(provider => {
            var signalRManager = provider.GetService<SignalRManager>();
            var logger = provider.GetService<ILoggerFactory>()!.CreateLogger<AzureMediaManager>();

            var scopeFactory = provider.GetService<IServiceScopeFactory>();

            var myService = new AzureMediaManager(signalRManager!, logger, scopeFactory!);

            //Enable Manager only in Production (video upload is disabled in development because of permissions)
            if (!isDevelopment)
                myService.InitManager().Wait();

            return myService;
        });

        builder.Services.AddSingleton(provider => {
            var hostEnvironment = provider.GetRequiredService<IHostEnvironment>();
            var logger = provider.GetRequiredService<ILogger<ValidatorFactory>>();
            return new ValidatorFactory("validationConfig.json", hostEnvironment, logger);
        });

        var app = builder.Build();

        app.UseCors();

        if (isDevelopment)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<LoggingMiddleware>();

        app.UseEndpoints(endpoints => {
            endpoints.MapControllers();
            endpoints.MapHub<NotificationHub>("/notificationHub");
        });

        app.Run();
    }
}