using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicTacToe.Data
{
    public class GameDbContextFactory : IDesignTimeDbContextFactory<GameDbContext> // Poinformowanie w jaki sposób ma utworzyc context bazy dancyh, aplikacja pomija pozostałe metody swtorzenia. 
		//Implement this interface to enable design-time services for context types that do not have a public default constructor. 
	{
        public GameDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GameDbContext>();
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=WindowsFormsApp1.DbContext;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            return new GameDbContext(optionsBuilder.Options);
        }
    }
}

