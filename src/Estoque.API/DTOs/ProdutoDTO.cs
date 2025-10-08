using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Estoque.API.DTOs
{
    public class ProdutoDTO
    {
        public string Nome { get; set; } = default!;
        public decimal Preco { get; set; } = default!;
        public int QuantidadeEstoque { get; set; } = default!;

    }
}