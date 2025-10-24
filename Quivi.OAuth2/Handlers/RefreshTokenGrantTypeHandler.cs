using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using Quivi.Application.OAuth2;
using Quivi.Application.OAuth2.Extensions;
using Quivi.Domain.Repositories.EntityFramework.Identity;
using Quivi.Infrastructure.Abstractions.Configurations;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Claims;
using Quivi.Infrastructure.Extensions;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace Quivi.OAuth2.Handlers
{
    public class RefreshTokenGrantTypeHandler : AGrantTypeHandler<PrincipalUserContext>, IOpenIddictServerHandler<HandleTokenRequestContext>
    {
        public RefreshTokenGrantTypeHandler(UserManager<ApplicationUser> userManager,
                                            SignInManager<ApplicationUser> signInManager,
                                            IIdConverter idConverter,
                                            IQueryProcessor queryProcessor,
                                            IOpenIddictServerDispatcher serverDispatcher,
                                            IJwtSettings jwtSettings,
                                            IAppHostsSettings appHostsSettings) : base(userManager, idConverter, queryProcessor, serverDispatcher, jwtSettings, appHostsSettings)
        {
        }

        protected override bool CanProcess(OpenIddictRequest request) => request.IsRefreshTokenGrantType();

        protected override async Task<PrincipalUserContext?> GetUserContext(OpenIddictRequest request)
        {
            var refreshToken = request.RefreshToken() ?? throw new GrantTypeRejectException(Errors.InvalidRequest, $"Parameter {RequestParameters.RefreshToken} was not provided");
            var rawMerchantId = request.MerchantId();
            string? merchantId = string.IsNullOrWhiteSpace(rawMerchantId) ? null : rawMerchantId;
            var audience = request.ClientId ?? "backoffice";

            var tokenIdentity = await GetIdentity(refreshToken, audience);
            if (tokenIdentity == null)
                return null;

            var rawUserId = tokenIdentity.UserId();
            if (string.IsNullOrWhiteSpace(rawUserId) == true)
                return null;

            int id = idConverter.FromPublicId(rawUserId);
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return null;

            var roles = await userManager.GetRolesAsync(user);
            return new PrincipalUserContext(user, roles, tokenIdentity);
        }

        protected override Task<int?> TargetMerchantId(OpenIddictRequest request, PrincipalUserContext userContext)
        {
            var rawMerchantId = request.MerchantId();
            string? merchantId = string.IsNullOrWhiteSpace(rawMerchantId) ? null : rawMerchantId;

            if (merchantId == null)
            {
                var audience = request.ClientId ?? "backoffice";
                var identitySubMerchantId = userContext.Principal.SubMerchantId();
                if (string.IsNullOrWhiteSpace(identitySubMerchantId) == false)
                    merchantId = identitySubMerchantId;
                else
                {
                    var identityMerchantId = userContext.Principal.MerchantId();
                    if (string.IsNullOrWhiteSpace(identityMerchantId) == false)
                        merchantId = identityMerchantId;
                }
            }
            if (merchantId == null)
                return Task.FromResult<int?>(null);

            return Task.FromResult<int?>(idConverter.FromPublicId(merchantId));
        }

        protected override Task<IEnumerable<Claim>> GetExtraClaims(OpenIddictRequest request, PrincipalUserContext userContext)
        {
            var employeeId = userContext.Principal.EmployeeId();
            return Task.FromResult<IEnumerable<Claim>>(string.IsNullOrWhiteSpace(employeeId) ? [] : [new Claim(QuiviClaims.EmployeeId, employeeId)]);
        }
    }
}