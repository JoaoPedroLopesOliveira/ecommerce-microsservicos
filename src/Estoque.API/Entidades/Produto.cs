using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.API.Entidades
{
    public class Produto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Nome { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        [Column (TypeName = "decimal(18,2)")]
        public decimal Preco { get; set; }
        public int QuantidadeEstoque { get; set; }

    }
}