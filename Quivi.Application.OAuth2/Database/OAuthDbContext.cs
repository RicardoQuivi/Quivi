using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;

namespace Quivi.Application.OAuth2.Database
{
    public class OAuthDbContext : DbContext
    {
        public OAuthDbContext(DbContextOptions<OAuthDbContext> options) : base(options) { }

        public DbSet<OpenIddictEntityFrameworkCoreApplication> Applications { get; set; }
        public DbSet<OpenIddictEntityFrameworkCoreAuthorization> Authorizations { get; set; }
        public DbSet<OpenIddictEntityFrameworkCoreScope> Scopes { get; set; }
        public DbSet<OpenIddictEntityFrameworkCoreToken> Tokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.UseOpenIddict();
            builder.HasDefaultSchema("auth");
            base.OnModelCreating(builder);
        }
    }
}