using Microsoft.EntityFrameworkCore;
using TestHostTestContainers.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContextPool<DataContext>(opts => 
    opts.UseNpgsql(builder.Configuration.GetConnectionString("default")));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
