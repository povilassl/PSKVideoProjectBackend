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

        bool isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        string dataSource = "Data source = ";

        if (isDevelopment)
        {
            dataSource += "DB/ProjectDatabase.db";
        }
        else
        {
            dataSource += "C:/home/site/wwwroot/ProjectDatabase.db";
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

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}