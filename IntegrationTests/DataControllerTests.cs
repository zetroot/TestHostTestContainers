using System.Net;
using System.Net.Http.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using TestHostTestContainers.Database;

namespace IntegrationTests;

public class DataControllerTests
{
    private IContainer _pgContainer = null!;
    private CustomAppFactory _factory = null!;
    private DataContext _context = null!;
    private HttpClient _client = null!;
    private IDataClient _refitClient = null!;
    private IServiceScope _scope = null!;
    
    [SetUp]
    public void Setup()
    {
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<DataContext>();
        _client = _factory.CreateClient();
        _refitClient = RestService.For<IDataClient>(_client);
    }

    [OneTimeSetUp]
    public async Task SetupContainer()
    {
        const string postgresPwd = "pgpwd";
        
        _pgContainer = new ContainerBuilder()
            .WithName(Guid.NewGuid().ToString("N"))
            .WithImage("postgres:15")
            .WithHostname(Guid.NewGuid().ToString("N"))
            .WithExposedPort(5432)
            .WithPortBinding(5432, true)
            .WithEnvironment("POSTGRES_PASSWORD", postgresPwd)
            .WithEnvironment("PGDATA", "/pgdata")
            .WithTmpfsMount("/pgdata")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("psql -U postgres -c \"select 1\""))
            .Build();
        await _pgContainer.StartAsync();
        
        _factory = new(_pgContainer.Hostname, _pgContainer.GetMappedPublicPort(5432), postgresPwd);
    }

    [TearDown]
    public void TearDown()
    {
        _scope.Dispose();
        _client.Dispose();
    }

    [OneTimeTearDown]
    public async Task DisposeContainer()
    {
        await _pgContainer.DisposeAsync();
    }

    [Test]
    public async Task PostData_WhenCalled_Returns200()
    {
        //act
        var response = await _client.PostAsJsonAsync(new Uri("data", UriKind.Relative), "test");

        //assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task PostData_WhenCalled_ReturnsIdOfAddedRecord()
    {
        //arrange
        var cntBefore = await _context.Set<UserData>().CountAsync();
        
        //act
        var id = await _refitClient.Create("test creation");
        
        //assert
        _context.Set<UserData>().Count().Should().BeGreaterThan(cntBefore);
        _context.Set<UserData>().Any(x => x.Id == id).Should().BeTrue();
        _context.Set<UserData>().Single(x => x.Id == id).Data.Should().Be("test creation");
    }
    
    [Test]
    public async Task GetData_WhenCalledForExistingId_ReturnsData()
    {
        //arrange
        var id = Guid.NewGuid();
        _context.Set<UserData>().Add(new UserData { Id = id, Data = "test get" });
        await _context.SaveChangesAsync();
        
        //act
        var actualData = await _refitClient.Get(id);
        
        //assert
        actualData.Should().Be("test get");
    }
    
    [Test]
    public void GetData_WhenCalledForUnknownId_ReturnsNotFound()
    {
        //arrange
        var id = Guid.NewGuid();
        
        //act
        var exception = Assert.CatchAsync<ApiException>(async () => await _refitClient.Get(id));
        
        //assert
        exception.Should().NotBeNull();
        exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
