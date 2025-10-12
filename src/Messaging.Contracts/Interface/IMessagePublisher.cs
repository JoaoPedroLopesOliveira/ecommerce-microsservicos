using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messaging.Contracts.Interface
{
    public interface IMessagePublisher
    {
        Task PublicarMensagemAsync<T>(string exchange, string routingKey, T message);
    }
}