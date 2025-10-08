using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Vendas.API.Entidades
{
    public class ItemPedido
    {
        [Key]
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public Pedido Pedido { get; set; } = null!;
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
        
    }
}