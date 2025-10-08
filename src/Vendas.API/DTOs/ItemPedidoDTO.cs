using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vendas.API.DTOs
{
    public class ItemPedidoDTO
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
    }
}