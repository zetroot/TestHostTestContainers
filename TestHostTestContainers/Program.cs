using Microsoft.EntityFrameworkCore;
using TestHostTestContainers.Database;

var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder);
var app = CreateApplication(builder);

app.Run();

public partial class Program
{
    public static WebApplicationBuilder ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddMvc().AddApplicationPart(typeof(Program).Assembly).AddControllersAsServices();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddDbContextPool<DataContext>(opts =>
            opts.UseNpgsql(builder.Configuration.GetConnectionString("default")));
        return builder;
    }
    
    public static WebApplication CreateApplication(WebApplicationBuilder webApplicationBuilder)
    {
        var app = webApplicationBuilder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();
        app.MapControllers();
        return app;
    }

};
