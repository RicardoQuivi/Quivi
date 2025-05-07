using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Quivi.Domain.Repositories.EntityFramework
{
    public class QuiviDbContextFactory : IDesignTimeDbContextFactory<QuiviContext>
    {
        public QuiviContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("Quivi");

            var optionsBuilder = new DbContextOptionsBuilder<QuiviContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new QuiviContext(optionsBuilder.Options);
        }
    }
}