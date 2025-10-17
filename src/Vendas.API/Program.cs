using Vendas.API.Infraestrutura.Db;
using Vendas.API.Servicos;
using Messaging.Contracts.Infraestrutura;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "vendas";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "root";
var dbPass = Environment.GetEnvironmentVariable("DB_PASS") ?? "root";

var connectionString = $"server={dbHost};port={dbPort};database={dbName};user={dbUser};password={dbPass}";

builder.Services.AddDbContext<DbContexto>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));


builder.Services.AddScoped<PedidoServico>();
builder.Services.AddSingleton<RabbitMqPublisher>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DbContexto>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
