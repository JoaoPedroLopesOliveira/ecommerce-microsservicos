using Messaging.Contracts.Infraestrutura;
using Microsoft.EntityFrameworkCore;
using Vendas.API.Infraestrutura.Db;
using Vendas.API.Servicos;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("mysql")
    ?? Environment.GetEnvironmentVariable("MYSQL_VENDAS")
    ?? "server=localhost;database=vendas;user=root;password=root";


builder.Services.AddScoped<PedidoServico>();
builder.Services.AddDbContext<DbContexto>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<RabbitMqPublisher>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();


app.MapControllers();

app.Run();
