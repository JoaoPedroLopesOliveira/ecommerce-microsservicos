using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gateway.API.Infraestrutura.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Vendas.API.Infraestrutura.Db
{
    public class DbContextoFactory : IDesignTimeDbContextFactory<DbContexto> {    
      public DbContexto CreateDbContext(string[] args)
      {
        var optionsBuilder = new DbContextOptionsBuilder<DbContexto>();
        
        var connectionString = Environment.GetEnvironmentVariable("MYSQL_COMN")??"server=localhost;database=usuarios;user=root;password=root";

        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        return new DbContexto(optionsBuilder.Options);
      }  
    }
}
