using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vendas.API.DTOs
{
    public class PedidoDTO
    {
        public int Id { get; set; }
        public DateTime DataPedido { get; set; }
        public string Status { get; set; } = null!;
        public List<ItemPedidoDTO> Itens { get; set; } = [];
    }
}