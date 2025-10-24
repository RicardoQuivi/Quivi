using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Server;
using Quivi.Application.OAuth2.Database;
using Quivi.Infrastructure.Abstractions.Configurations;
using Quivi.Infrastructure.Configurations;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace Quivi.Application.OAuth2.Extensions
{
    public interface IScopeHandler
    {
        void AddHandler<T>() where T : IOpenIddictServerHandler<HandleTokenRequestContext>;
    }

    public static class ServiceCollectionExtensions
    {
        private class ScopeHandler : IScopeHandler
        {
            private int order = 900_000;

            private readonly OpenIddictServerBuilder serverBuilder;

            public ScopeHandler(OpenIddictServerBuilder serverBuilder)
            {
                this.serverBuilder = serverBuilder;
            }

            public void AddHandler<T>() where T : IOpenIddictServerHandler<HandleTokenRequestContext>
            {
                serverBuilder.AddEventHandler<HandleTokenRequestContext>(builder =>
                {
                    builder.UseScopedHandler<T>().SetOrder(order++);
                });
            }
        }

        public static IServiceCollection RegisterOAuth2(this IServiceCollection serviceCollection, IConfiguration configuration, Action<IScopeHandler>? action = null)
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
                                    var hostsSettings = configuration.GetSection("AppHosts").Get<AppHostsSettings>()!;

                                    options.SetIssuer(new Uri(hostsSettings.OAuth));
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

                                    if (action != null)
                                    {
                                        var handler = new ScopeHandler(options);
                                        action(handler);
                                    }

                                    options.AddSigningCertificate(jwtSettings.SigningCertificate);
                                    options.AddEncryptionCertificate(jwtSettings.EncryptionCertificate);

                                    options.UseAspNetCore()
                                           //.EnableTokenEndpointPassthrough()
                                           .EnableAuthorizationEndpointPassthrough()
                                           .DisableTransportSecurityRequirement();
                                });

            return serviceCollection;
        }
    }
}