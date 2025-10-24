using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using Quivi.Application.Queries.Merchants;
using Quivi.Domain.Entities.Merchants;
using Quivi.Domain.Repositories.EntityFramework.Identity;
using Quivi.Infrastructure.Abstractions.Configurations;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Claims;
using Quivi.Infrastructure.Extensions;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace Quivi.OAuth2.Handlers
{
    public class GrantTypeRejectException : Exception
    {
        public string Error { get; }
        public string? Description { get; }

        public GrantTypeRejectException(string error, string? description)
        {
            Error = error;
            Description = description;
        }
    }

    public interface IUserContext
    {
        int Id { get; }
        string Email { get; }
        string? FullName { get; }
        IEnumerable<string> Roles { get; }
    }

    public abstract class AGrantTypeHandler<TUserContext> : IOpenIddictServerHandler<HandleTokenRequestContext> where TUserContext : IUserContext
    {
        protected readonly UserManager<ApplicationUser> userManager;
        protected readonly IIdConverter idConverter;
        protected readonly IQueryProcessor queryProcessor;
        private readonly IOpenIddictServerDispatcher serverDispatcher;
        private readonly IJwtSettings jwtSettings;
        private readonly IAppHostsSettings appHostsSettings;

        protected AGrantTypeHandler(UserManager<ApplicationUser> userManager,
                                        IIdConverter idConverter,
                                        IQueryProcessor queryProcessor,
                                        IOpenIddictServerDispatcher serverDispatcher,
                                        IJwtSettings jwtSettings,
                                        IAppHostsSettings appHostsSettings)
        {
            this.userManager = userManager;
            this.idConverter = idConverter;
            this.queryProcessor = queryProcessor;
            this.serverDispatcher = serverDispatcher;
            this.jwtSettings = jwtSettings;
            this.appHostsSettings = appHostsSettings;
        }

        private HandleTokenRequestContext? Context { get; set; }

        protected abstract bool CanProcess(OpenIddictRequest request);
        protected abstract Task<TUserContext?> GetUserContext(OpenIddictRequest request);
        protected abstract Task<int?> TargetMerchantId(OpenIddictRequest request, TUserContext userContext);
        protected abstract Task<IEnumerable<Claim>> GetExtraClaims(OpenIddictRequest request, TUserContext userContext);

        public async ValueTask HandleAsync(HandleTokenRequestContext context)
        {
            try
            {
                if (CanProcess(context.Request) == false)
                    return;

                Context = context;
                var user = await GetUserContext(context.Request);
                if (user == null)
                {
                    context.Reject(error: Errors.AccessDenied, description: "Invalid credentials");
                    return;
                }

                var identity = await ProcessIdentity(user, async identity =>
                {
                    var merchantId = await TargetMerchantId(context.Request, user);
                    await SetQuiviClaims(identity, user.Id, merchantId);
                    var extraClaims = await GetExtraClaims(context.Request, user);
                    identity.AddClaims(extraClaims);
                }, context.Request.ClientId ?? "");

                context.SignIn(identity);
            }
            catch (GrantTypeRejectException e)
            {
                context.Reject(error: e.Error, description: e.Description);
                return;
            }
            catch
            {
                context.Reject(error: Errors.ServerError, "Internal server error");
                return;
            }
        }

        private async Task<ClaimsPrincipal> ProcessIdentity(IUserContext user, Func<ClaimsIdentity, Task> identityFunc, string audience)
        {
            var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType, QuiviClaims.Email, QuiviClaims.Role);
            identity.SetClaim(QuiviClaims.UserId, idConverter.ToPublicId(user.Id));
            identity.SetClaim(QuiviClaims.Email, user.Email);
            if (string.IsNullOrWhiteSpace(user.FullName) == false)
                identity.SetClaim(QuiviClaims.Name, user.FullName);

            identity.AddClaims(user.Roles.Select(r => new Claim(QuiviClaims.Role, r)));
            await identityFunc(identity);

            SetDestinations(identity);

            var principal = new ClaimsPrincipal(identity);
            principal.SetAudiences(audience);
            principal.SetScopes(Scopes.OpenId, Scopes.Profile, Scopes.Email, Scopes.OfflineAccess);

            return principal;
        }

        private void SetDestinations(ClaimsIdentity identity)
        {
            identity.SetDestinations(static claim => claim.Type switch
            {
                QuiviClaims.Role => [Destinations.AccessToken, Destinations.IdentityToken],
                QuiviClaims.MerchantId => [Destinations.AccessToken, Destinations.IdentityToken],
                QuiviClaims.SubMerchantId => [Destinations.AccessToken, Destinations.IdentityToken],
                QuiviClaims.EmployeeId => [Destinations.AccessToken, Destinations.IdentityToken],
                _ => [Destinations.AccessToken],
            });
        }

        private async Task SetQuiviClaims(ClaimsIdentity identity, int userId, int? merchantId)
        {
            if (merchantId.HasValue == false)
            {
                await SetQuiviClaims(identity, userId);
                return;
            }

            var isAdmin = IsAdmin(identity);
            var merchant = await GetMerchantId(userId, merchantId.Value, isAdmin);
            identity.SetClaim(QuiviClaims.MerchantId, idConverter.ToPublicId(merchant.ParentMerchant?.Id ?? merchant.Id));

            if (merchant.ParentMerchant != null)
            {
                identity.SetClaim(QuiviClaims.ActivatedAt, merchant.ParentMerchant.TermsAndConditionsAcceptedDate.HasValue ? new DateTimeOffset(merchant.ParentMerchant.TermsAndConditionsAcceptedDate.Value.ToUniversalTime()).ToUnixTimeSeconds() : null);
                identity.SetClaim(QuiviClaims.SubMerchantId, idConverter.ToPublicId(merchant.Id));
                return;
            }

            identity.SetClaim(QuiviClaims.ActivatedAt, merchant.TermsAndConditionsAcceptedDate.HasValue ? new DateTimeOffset(merchant.TermsAndConditionsAcceptedDate.Value.ToUniversalTime()).ToUnixTimeSeconds() : null);
        }

        private async Task SetQuiviClaims(ClaimsIdentity identity, int userId)
        {
            var isAdmin = IsAdmin(identity);
            var merchant = await GetDefaultMerchantId(userId, null, isAdmin);
            if (merchant == null)
                return;

            identity.SetClaim(QuiviClaims.MerchantId, idConverter.ToPublicId(merchant.Id));
            identity.SetClaim(QuiviClaims.ActivatedAt, merchant.TermsAndConditionsAcceptedDate.HasValue ? new DateTimeOffset(merchant.TermsAndConditionsAcceptedDate.Value.ToUniversalTime()).ToUnixTimeSeconds() : null);

            var subMerchant = await GetDefaultMerchantId(userId, merchant.Id, isAdmin);
            if (subMerchant != null)
                identity.SetClaim(QuiviClaims.SubMerchantId, idConverter.ToPublicId(subMerchant.Id));
        }

        private bool IsAdmin(ClaimsIdentity identity) => new ClaimsPrincipal(identity).IsAdmin();

        private async Task<Merchant> GetMerchantId(int userId, int merchantId, bool isAdmin)
        {
            var merchantQuery = await queryProcessor.Execute(new GetMerchantsAsyncQuery
            {
                ApplicationUserIds = isAdmin ? null : [userId],
                Ids = [merchantId],
                PageSize = 1,
                IsDeleted = false,
                IncludeParentMerchant = true,
            });

            if (merchantQuery.Any() == false)
                throw new UnauthorizedAccessException();

            return merchantQuery.First();
        }

        private async Task<Merchant?> GetDefaultMerchantId(int userId, int? parentMerchantId, bool isAdmin)
        {
            var merchantQuery = await queryProcessor.Execute(new GetMerchantsAsyncQuery
            {
                ApplicationUserIds = isAdmin ? null : [userId],
                ParentIds = parentMerchantId.HasValue ? [parentMerchantId.Value] : null,
                IsParentMerchant = parentMerchantId.HasValue == false,
                PageSize = 1,
                IsDeleted = false,
            });

            return merchantQuery.FirstOrDefault();
        }

        protected async Task<ClaimsPrincipal?> GetIdentity(string token, string audience)
        {
            var validatedContext = new ValidateTokenContext(Context?.Transaction ?? throw new Exception("This should never happen"))
            {
                Token = token,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = new Uri(appHostsSettings.OAuth, UriKind.Absolute).ToString(),
                    ValidAudience = audience,
                    IssuerSigningKey = new RsaSecurityKey(jwtSettings.SigningCertificate.GetRSAPublicKey()),
                    RoleClaimType = QuiviClaims.Role,
                },
            };
            await serverDispatcher.DispatchAsync(validatedContext);
            return validatedContext.Principal;
        }
    }
}