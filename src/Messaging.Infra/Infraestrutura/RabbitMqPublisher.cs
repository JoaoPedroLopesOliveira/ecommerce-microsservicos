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
        public Task PublicarMensagemAsync<T>(string exchange, string routingKey, T message)
        {
            throw new NotImplementedException();
        }
    }
}