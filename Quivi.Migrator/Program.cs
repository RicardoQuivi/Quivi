using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using Quivi.Application.Extensions;
using Quivi.Application.OAuth2;
using Quivi.Application.OAuth2.Database;
using Quivi.Application.OAuth2.Extensions;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Domain.Repositories.EntityFramework.Identity;
using Quivi.Infrastructure.Roles;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Quivi.Migrator
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.RegisterAll(builder.Configuration);
            builder.Services.RegisterOAuth2(builder.Configuration);

            var host = builder.Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                using var quiviContext = services.GetRequiredService<QuiviContext>();
                await quiviContext.Database.MigrateAsync();
                await SeedRolesAsync(services);

                using var oauthContext = services.GetRequiredService<OAuthDbContext>();
                await oauthContext.Database.MigrateAsync();
                await SeedOAuthAsync(services);
            }
        }

        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            string[] roleNames = { QuiviRoles.Admin, QuiviRoles.SuperAdmin };

            foreach (var roleName in roleNames)
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName);
                if (roleExists)
                    continue;

                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = roleName,
                });
            }
        }

        async static Task SeedOAuthAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var appManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            var backofficeApp = await appManager.FindByClientIdAsync("backoffice");
            if (backofficeApp is null)
            {
                await appManager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "backoffice",
                    Permissions =
                    {
                        Permissions.Endpoints.Introspection,

                        Permissions.Endpoints.Token,

                        // Enable these based on your grant type
                        Permissions.GrantTypes.Password,
                        Permissions.GrantTypes.RefreshToken,

                        // Allow requesting scopes (optional)
                        Permissions.Prefixes.Scope + "api",
                    },
                });
            }

            var posApp = await appManager.FindByClientIdAsync("pos");
            if (posApp is null)
            {
                await appManager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "pos",
                    Permissions =
                    {
                        Permissions.Endpoints.Introspection,

                        Permissions.Endpoints.Token,

                        // Enable these based on your grant type
                        Permissions.Prefixes.GrantType + CustomGrantTypes.TokenExchange,
                        Permissions.Prefixes.GrantType + CustomGrantTypes.Employee,
                        Permissions.GrantTypes.RefreshToken,

                        // Allow requesting scopes (optional)
                        Permissions.Prefixes.Scope + "api",
                    }
                });
            }
        }
    }
}