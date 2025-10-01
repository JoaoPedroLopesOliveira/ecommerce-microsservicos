using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gateway.API.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Estoque.API.Infraestrutura.Db
{
    public class DbContexto : DbContext
    {
        private readonly IConfiguration? _configurationAppSettings;

        public DbContexto(IConfiguration configurationAppSettings)
        {
            _configurationAppSettings = configurationAppSettings;
        }

        public DbContexto(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Produto> Produtos { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var stringConexao = _configurationAppSettings.GetConnectionString("mysql")?.ToString();
            if (!string.IsNullOrEmpty(stringConexao))
            {
                optionsBuilder.UseMySql(stringConexao, ServerVersion.AutoDetect(stringConexao));
            }
        }
    }
}