using Microsoft.EntityFrameworkCore;
using TestHostTestContainers.Database;

var app = WebApplication.CreateBuilder(args)
    .ConfigureServices()
    .CreateApplication();

app.Run();

public static partial class Program
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddMvc().AddApplicationPart(typeof(Program).Assembly).AddControllersAsServices();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddDbContextPool<DataContext>(opts =>
            opts.UseNpgsql(builder.Configuration.GetConnectionString("default")));
        return builder;
    }
    
    public static WebApplication CreateApplication(this WebApplicationBuilder webApplicationBuilder)
    {
        var app = webApplicationBuilder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();
        app.MapControllers();
        return app;
    }

};
