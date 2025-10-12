using System.Collections.Generic;
using Messaging.Contracts.Messages;

namespace Messaging.Contracts
{

    public class PedidoMensagem
    {
        public int PedidoId { get; set; }
        public List<ItemPedidoMensagem> Itens { get; set; } = new();
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    }
}
