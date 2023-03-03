using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using TestHostTestContainers.Database;

namespace IntegrationTests;

public class DataControllerTests
{
    private WebApplication _app = null!;
    private HttpClient _client = null!;
    private IDataClient _refitClient = null!;

    [SetUp]
    public async Task Setup()
    {
        var builder = WebApplication.CreateBuilder()
            .ConfigureServices();
        _app = builder.CreateApplication();
        _app.Urls.Add("http://*:8080");
        await _app.StartAsync();
        _client = new HttpClient { BaseAddress = new Uri("http://localhost:8080") };
        _refitClient = RestService.For<IDataClient>(_client);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _app.StopAsync();
        _client.Dispose();
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
        var context = _app.Services.GetRequiredService<DataContext>();
        var cntBefore = await context.Set<UserData>().CountAsync();
        
        //act
        var id = await _refitClient.Create("test creation");
        
        //assert
        context.Set<UserData>().Count().Should().BeGreaterThan(cntBefore);
        context.Set<UserData>().Any(x => x.Id == id).Should().BeTrue();
        context.Set<UserData>().Single(x => x.Id == id).Data.Should().Be("test creation");
    }
    
    [Test]
    public async Task GetData_WhenCalledForExistingId_ReturnsData()
    {
        //arrange
        var context = _app.Services.GetRequiredService<DataContext>();
        var id = Guid.NewGuid();
        context.Set<UserData>().Add(new UserData { Id = id, Data = "test get" });
        await context.SaveChangesAsync();
        
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
