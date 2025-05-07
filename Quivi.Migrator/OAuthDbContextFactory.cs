using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Quivi.Application.OAuth2.Database;

namespace Quivi.Migrator
{
    public class OAuthDbContextFactory : IDesignTimeDbContextFactory<OAuthDbContext>
    {
        public OAuthDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();

            // Set up configuration
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("OAuth");

            var optionsBuilder = new DbContextOptionsBuilder<OAuthDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new OAuthDbContext(optionsBuilder.Options);
        }
    }
}
