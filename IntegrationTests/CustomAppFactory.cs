using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TestHostTestContainers.Database;

namespace IntegrationTests;

public class CustomAppFactory : WebApplicationFactory<Program>
{
    private readonly string _dbConnStr;

    public CustomAppFactory(string host, int port, string password)
    {
        var sb = new NpgsqlConnectionStringBuilder
        {
            Host = host, Port = port, Database = "test_ci_database", Username = "postgres", Password = password
        };
        _dbConnStr = sb.ConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Удалим зарегистрированный DataContext
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<DataContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Зарегистрируем снова с указанием на тестовую БД
            services.AddDbContextPool<DataContext>(opts => opts.UseNpgsql(_dbConnStr));

            // Обеспечим создание БД
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var context = scopedServices.GetRequiredService<DataContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        });
    }
}
