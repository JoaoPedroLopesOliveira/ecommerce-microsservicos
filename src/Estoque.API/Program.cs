using Estoque.API.Infraestrutura.Db;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// pega connection string do appsettings ou user-secrets
var connectionString = builder.Configuration.GetConnectionString("mysql");

builder.Services.AddDbContext<DbContexto>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddControllers();
var app = builder.Build();

app.MapControllers();

app.Run();
