using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Gateway.API.Entidades;

namespace Gateway.API.Infraestrutura.Db
{
    public class DbContexto : DbContext
    {
        public DbContexto(DbContextOptions options) : base(options)
        {
        }
        DbSet<Usuarios> Usuarios { get; set; } = default!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}