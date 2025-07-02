using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quivi.Application.OAuth2.Database;
using Quivi.Application.OAuth2.OpenIddict.Handlers;
using Quivi.Infrastructure.Abstractions.Configurations;
using Quivi.Infrastructure.Configurations;
using System.Security.Cryptography.X509Certificates;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace Quivi.Application.OAuth2.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterOAuth2(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddDbContext<OAuthDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("OAuth"));
                options.UseOpenIddict();
            });

            serviceCollection.AddOpenIddict()
                                .AddCore(options =>
                                {
                                    // Configure OpenIddict to use the Entity Framework Core stores and models.
                                    // Note: call ReplaceDefaultEntities() to replace the default entities.
                                    options.UseEntityFrameworkCore()
                                           .UseDbContext<OAuthDbContext>();
                                })
                                // Register the OpenIddict server components.
                                .AddServer(options =>
                                {
                                    IJwtSettings jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()!;

                                    options.DisableAccessTokenEncryption();

                                    options.SetAccessTokenLifetime(jwtSettings.ExpireTimeSpan)
                                            .SetRefreshTokenLifetime(jwtSettings.RefreshTokenExpireTimeSpan);

                                    options.SetTokenEndpointUris("/connect/token");
                                    options.SetIntrospectionEndpointUris("/connect/introspect");

                                    options.AllowCustomFlow(CustomGrantTypes.TokenExchange);
                                    options.AllowCustomFlow(CustomGrantTypes.Employee);
                                    options.AllowClientCredentialsFlow();
                                    options.AllowPasswordFlow();
                                    options.AllowRefreshTokenFlow();

                                    options.AddEventHandler<HandleTokenRequestContext>(builder =>
                                    {
                                        builder.UseScopedHandler<EmployeeGrantTypeHandler>()
                                                .SetOrder(900_000);
                                    });

                                    var certificateBytes = Convert.FromBase64String(jwtSettings.Certificate.Base64);
                                    var cert = new X509Certificate2(certificateBytes, jwtSettings.Certificate.Password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
                                    options.AddSigningCertificate(cert);
                                    options.AddEncryptionCertificate(cert);

                                    options.UseAspNetCore()
                                           .EnableTokenEndpointPassthrough()
                                           .DisableTransportSecurityRequirement();
                                });

            return serviceCollection;
        }
    }
}