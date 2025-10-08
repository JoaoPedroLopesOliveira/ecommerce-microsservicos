using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;

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
            throw new NotImplementedException();
        }
    }
    
}