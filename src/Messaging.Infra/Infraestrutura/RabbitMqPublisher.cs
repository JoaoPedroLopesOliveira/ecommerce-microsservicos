using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Messaging.Contracts.Interface;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Messaging.Contracts.Infraestrutura
{
    public class RabbitMqPublisher : IMessagePublisher
    {
        private readonly IConfiguration _config;

        public RabbitMqPublisher(IConfiguration config)
        {
            _config = config;
        }

        public Task PublicarMensagemAsync<T>(string exchange, string routingKey, T message)
        {
            var factory = new ConnectionFactory
            {
                HostName = _config["RabbitMq:HostName"] ?? "rabbitmq",
                UserName = _config["RabbitMq:UserName"] ?? "guest",
                Password = _config["RabbitMq:Password"] ?? "guest"
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Direct, durable: true);

            var json = System.Text.Json.JsonSerializer.Serialize(message);
            var body = System.Text.Encoding.UTF8.GetBytes(json);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: exchange,
                                 routingKey: routingKey,
                                 basicProperties: properties,
                                 body: body);

            return Task.CompletedTask;
        }
    }
}