using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vendas.API.Entidades;

namespace Vendas.API.Infraestrutura.Db
{
    public class DbContexto : DbContext
    {
        public DbContexto(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Pedido> Pedidos { get; set; } = default!;
        public DbSet<ItemPedido> ItensPedidos { get; set; } = default!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<Pedido>()
                .Property(p => p.Status)
                .HasConversion<string>();

            
            
            modelBuilder.Entity<Pedido>()
                .HasMany(p => p.Itens)
                .WithOne(i => i.Pedido)
                .HasForeignKey(i => i.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
}
}
 