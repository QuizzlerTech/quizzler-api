
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MySql.Data.MySqlClient;
using System;


namespace Quizzler_Backend
{
    public class QuizzlerDbContextFactory : IDesignTimeDbContextFactory<QuizzlerDbContext>
    {

        public QuizzlerDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<QuizzlerDbContext>();
            optionsBuilder.UseLazyLoadingProxies();
            optionsBuilder.UseMySQL(Environment.GetEnvironmentVariable("DbConnection")); 

            return new QuizzlerDbContext(optionsBuilder.Options);
        }
    }
}
