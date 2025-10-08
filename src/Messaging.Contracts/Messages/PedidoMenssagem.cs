using System.Collections.Generic;

namespace Messaging.Contracts
{
    public record ItemPedidoMensagem(int ProdutoId, int Quantidade);

    public record PedidoMensagem(int PedidoId, List<ItemPedidoMensagem> Itens, string CorrelationId);
}
