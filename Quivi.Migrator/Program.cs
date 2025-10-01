using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using Quivi.Application.Extensions;
using Quivi.Application.OAuth2;
using Quivi.Application.OAuth2.Database;
using Quivi.Application.OAuth2.Extensions;
using Quivi.Domain.Entities.Financing;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Domain.Repositories.EntityFramework.Identity;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
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

            await using (var scope = host.Services.CreateAsyncScope())
            {
                var services = scope.ServiceProvider;

                using var quiviContext = services.GetRequiredService<QuiviContext>();
                await quiviContext.Database.MigrateAsync();
                await SeedRolesAsync(services);
                await SeedAnonymousConsumer(services);
                await SeedQuiviConsumer(services);

                using var oauthContext = services.GetRequiredService<OAuthDbContext>();
                await oauthContext.Database.MigrateAsync();
                await SeedOAuthAsync(services);
            }
        }

        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            string[] roleNames = [QuiviRoles.Admin, QuiviRoles.SuperAdmin];

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

        private static async Task SeedAnonymousConsumer(IServiceProvider services)
        {
            var peopleRepository = services.GetRequiredService<IPeopleRepository>();

            var anonymousConsumer = await peopleRepository.GetAsync(new GetPeopleCriteria
            {
                IsAnonymous = true,
                PageSize = 0,
            });
            if (anonymousConsumer.TotalItems > 0)
                return;

            var dateTimeProvider = services.GetRequiredService<IDateTimeProvider>();
            var now = dateTimeProvider.GetUtcNow();
            peopleRepository.Add(new Person
            {
                PersonType = PersonType.Consumer,
                IsAnonymous = true,
                CreatedDate = now,
                ModifiedDate = now,
            });
            await peopleRepository.SaveChangesAsync();
        }

        private static async Task SeedQuiviConsumer(IServiceProvider services)
        {
            var peopleRepository = services.GetRequiredService<IPeopleRepository>();

            var quiviConsumer = await peopleRepository.GetAsync(new GetPeopleCriteria
            {
                PersonTypes = [PersonType.Quivi],
                IsAnonymous = true,
                PageSize = 0,
            });
            if (quiviConsumer.TotalItems > 0)
                return;

            var dateTimeProvider = services.GetRequiredService<IDateTimeProvider>();
            var now = dateTimeProvider.GetUtcNow();
            peopleRepository.Add(new Person
            {
                PersonType = PersonType.Quivi,
                IsAnonymous = false,
                CreatedDate = now,
                ModifiedDate = now,
            });
            await peopleRepository.SaveChangesAsync();
        }

        async static Task SeedOAuthAsync(IServiceProvider serviceProvider)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
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

            var guestsApp = await appManager.FindByClientIdAsync("guests");
            if (guestsApp is null)
            {
                await appManager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "guests",
                    Permissions =
                    {
                        Permissions.Endpoints.Introspection,

                        Permissions.Endpoints.Token,

                        // Enable these based on your grant type
                        Permissions.GrantTypes.Password,
                        Permissions.GrantTypes.RefreshToken,

                        // Allow requesting scopes (optional)
                        Permissions.Prefixes.Scope + "api",
                    }
                });
            }
        }
    }
}