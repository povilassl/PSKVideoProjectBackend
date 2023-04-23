using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PSKVideoProjectBackend;
using PSKVideoProjectBackend.Repositories;
using System.Diagnostics;
using System.Reflection;

internal class Program
{
    private static void Main(string[] args)
    {
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
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });

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

        bool isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        string dataSource;

        if (isDevelopment)
        {
            dataSource = "Data source = DB/ProjectDatabase.db";
        }
        else
        {
            dataSource = "Data source = C:/home/site/wwwroot/ProjectDatabase.db";
        }

        builder.Services.AddDbContext<ApiDbContext>(o => o.UseSqlite(dataSource));

        //repositories
        builder.Services.AddScoped<VideoRepository>();
        builder.Services.AddScoped<UserRepository>();

        var app = builder.Build();

        AzureMediaManager.InitManager();

        app.UseCors();

        //enable swagger in both Debug and Release
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseAuthentication();

        app.UseEndpoints(endpoints => {
            endpoints.MapControllers();
        });

        app.Run();
    }
}