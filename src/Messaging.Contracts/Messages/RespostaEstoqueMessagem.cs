using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messaging.Contracts.Messages
{
    public class RespostaEstoqueMessagem
    {
        public static implicit operator RespostaEstoqueMessagem(RespostaEstoque v)
        {
            throw new NotImplementedException();
        }

        public record RespostaEstoque(int PedidoId, bool Aprovado, string CorrelationId, string? Motivo = null);
    }
}