
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Quizzler_Backend.Data;

namespace Quizzler_Backend
{
    public class QuizzlerDbContextFactory : IDesignTimeDbContextFactory<QuizzlerDbContext>
    {

        public QuizzlerDbContext CreateDbContext(string[] args)
        {
            var DB_CONNECTION_STRING = Environment.GetEnvironmentVariable("DbConnection") ?? throw new InvalidOperationException("DbConnection env variable is null or empty.");
            var optionsBuilder = new DbContextOptionsBuilder<QuizzlerDbContext>();
            optionsBuilder.UseLazyLoadingProxies();
            optionsBuilder.UseMySQL(DB_CONNECTION_STRING);

            return new QuizzlerDbContext(optionsBuilder.Options);
        }
    }
}
