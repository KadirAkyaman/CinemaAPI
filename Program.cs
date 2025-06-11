using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder();
var connectionString = builder.Configuration.GetConnectionString("PostgreSQLConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();

var app = builder.Build();

app.Run();
