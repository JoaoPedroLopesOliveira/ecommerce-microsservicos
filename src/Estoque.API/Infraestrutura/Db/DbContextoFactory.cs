using Estoque.API.Infraestrutura.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class DbContextoFactory : IDesignTimeDbContextFactory<DbContexto>
{
    public DbContexto CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DbContexto>();
        
        var connectionString = Environment.GetEnvironmentVariable("MYSQL_COMN")??"server=localhost;database=estoque;user=root;password=root";

        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        return new DbContexto(optionsBuilder.Options);
    }
}
