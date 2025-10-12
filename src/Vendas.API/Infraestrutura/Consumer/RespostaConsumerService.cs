using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Messaging.Contracts.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Vendas.API.Enuns;
using Vendas.API.Infraestrutura.Db;

namespace Vendas.API.Infraestrutura.Consumer
{
    public class RespostaConsumerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _config;
        private IConnection? _connection;
        private IModel? _channel;
        private readonly string _exchange;
        private readonly string _queue = "resposta_vendas.queue";
        private readonly string _routingKey = "pedido.resposta";

        public RespostaConsumerService(IServiceProvider serviceProvider, IConfiguration config)
        {
            _serviceProvider = serviceProvider;
            _config = config;
            _exchange = config["RabbitMq:Exchange"] ?? "ecommerce.exchange";
            InitRabbit();
        }

        private void InitRabbit()
        {
            var factory = new ConnectionFactory
            {
                HostName = _config["RabbitMq:HostName"] ?? "rabbitmq",
                UserName = _config["RabbitMq:UserName"] ?? "guest",
                Password = _config["RabbitMq:Password"] ?? "guest",
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: _exchange, type: ExchangeType.Direct, durable: true);
            _channel.QueueDeclare(_queue, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(queue: _queue, exchange: _exchange, routingKey: _routingKey);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (sender, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var messageJson = Encoding.UTF8.GetString(body);

                    var resposta = JsonSerializer.Deserialize<RespostaEstoqueMensagem>(messageJson);

                    if (resposta is not null)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var contexto = scope.ServiceProvider.GetRequiredService<DbContexto>();

                        var pedido = await contexto.Pedidos.FindAsync(resposta.PedidoId);

                        if (pedido != null)
                        {
                            pedido.Status = resposta.Aprovado ? StatusPedido.APROVADO : StatusPedido.REPROVADO;
                            pedido.Motivo = resposta.Motivo;
                            await contexto.SaveChangesAsync();

                            Console.WriteLine($"[✔] Pedido {pedido.Id} atualizado: {pedido.Status} - {pedido.Motivo}");
                        }
                        else
                        {
                            Console.WriteLine($"[⚠] Pedido {resposta.PedidoId} não encontrado no banco de Vendas.");
                        }
                    }

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Erro Consumer Resposta] {ex.Message}");
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(queue: _queue, autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            try { _channel?.Close(); } catch { }
            try { _connection?.Close(); } catch { }
            base.Dispose();
        }
    }
}
