using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messaging.Contracts.Messages
{
    public class RespostaEstoqueMensagem
    {
        public int PedidoId { get; set; }
        public bool Aprovado { get; set; }
        public string? Motivo { get; set; }
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    }
}