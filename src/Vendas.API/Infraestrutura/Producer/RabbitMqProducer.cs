using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Messaging.Contracts;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Vendas.API.Infraestrutura.Producer
{
    public class RabbitMqProducer : IRabbitMqProducer, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchange;

        public RabbitMqProducer(IConfiguration config)
        {
            var factory = new ConnectionFactory
            {
                HostName = config["RabbitMq:HostName"] ?? "localhost",
                UserName = config["RabbitMq:UserName"] ?? "guest",
                Password = config["RabbitMq:Password"] ?? "guest"
            };

            _exchange = config["RabbitMq:Exchange"] ?? "ecommerce.exchange";

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: _exchange, type: ExchangeType.Direct, durable: true);
        }

        public Task PublicarPedido(PedidoMensagem mensagem)
        {
            var routingKey = "pedido.criado";
            var json = JsonSerializer.Serialize(mensagem);
            var body = Encoding.UTF8.GetBytes(json);

            var props = _channel.CreateBasicProperties();
            props.Persistent = true;
            props.CorrelationId = mensagem.CorrelationId;

            _channel.BasicPublish(
                exchange: _exchange,
                routingKey: routingKey,
                basicProperties: props,
                body: body
            );

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }

    public interface IRabbitMqProducer
    {
        Task PublicarPedido(PedidoMensagem mensagem);
    }
}
