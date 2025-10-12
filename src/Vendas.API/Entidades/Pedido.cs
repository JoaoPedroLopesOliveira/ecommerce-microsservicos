using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Vendas.API.Enuns;

namespace Vendas.API.Entidades
{
    public class Pedido
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public StatusPedido Status { get; set; }
        public string? Motivo { get; set; }
        public DateTime DataPedido { get; set; }
        public ICollection <ItemPedido> Itens { get; set; } = new List<ItemPedido>()!;
    }
}