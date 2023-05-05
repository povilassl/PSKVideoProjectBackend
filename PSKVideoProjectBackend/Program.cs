using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PSKVideoProjectBackend;
using PSKVideoProjectBackend.Repositories;
using System.Diagnostics;
using System.Reflection;
using log4net;
using log4net.Config;

internal class Program
{
    private static readonly ILog _log = LogManager.GetLogger(typeof(Program));

    private static void Main(string[] args)
    {
        var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
        XmlConfigurator.ConfigureAndWatch(logRepository, new FileInfo("log4net.config"));
        _log.Info("Application starting");

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c => {
            c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
            $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
        });

        builder.Services.AddCors(options => {
            options.AddDefaultPolicy(
                builder => {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });

        builder.Logging.AddLog4Net();

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options => {
                options.LoginPath = new PathString("/UserInteractionsController/Login");
                options.Cookie.Name = "VideotekaAuthentication";
                options.ExpireTimeSpan = TimeSpan.FromDays(10);
            });

        builder.Services.AddAuthorization(options => {
            options.AddPolicy("Public", policy => {
                policy.RequireAuthenticatedUser();
            });
        });

        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        string dataSource = isDevelopment
            ? "Data source=DB/ProjectDatabase.db"
            : "Data source=C:/home/site/wwwroot/ProjectDatabase.db";

        builder.Services.AddDbContext<ApiDbContext>(o => o.UseSqlite(dataSource));

        //repositories
        builder.Services.AddScoped<VideoRepository>();
        builder.Services.AddScoped<UserRepository>();

        var app = builder.Build();

        AzureMediaManager.Instance.InitManager();

        app.UseCors();

        //enable swagger in both Debug and Release
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseAuthentication();

        app.UseMiddleware<LoggingMiddleware>();

        app.UseEndpoints(endpoints => {
            endpoints.MapControllers();
        });

        app.Run();
    }
}