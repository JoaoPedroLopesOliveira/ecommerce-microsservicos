using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Estoque.API.DTOs
{
    public class AutorizarVendaDTO
    {
        public List<int> ProdutoIds { get; set; } = new();
        public List<int> Quantidades { get; set; } = new();
        public int PedidoId { get; set; }
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    }

}