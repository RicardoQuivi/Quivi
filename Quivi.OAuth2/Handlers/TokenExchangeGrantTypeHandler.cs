using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using Quivi.Application.OAuth2;
using Quivi.Application.OAuth2.Extensions;
using Quivi.Domain.Repositories.EntityFramework.Identity;
using Quivi.Infrastructure.Abstractions.Configurations;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Extensions;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace Quivi.OAuth2.Handlers
{
    public class TokenExchangeGrantTypeHandler : AGrantTypeHandler<PrincipalUserContext>, IOpenIddictServerHandler<HandleTokenRequestContext>
    {

        public TokenExchangeGrantTypeHandler(UserManager<ApplicationUser> userManager,
                                            SignInManager<ApplicationUser> signInManager,
                                            IIdConverter idConverter,
                                            IQueryProcessor queryProcessor,
                                            IOpenIddictServerDispatcher serverDispatcher,
                                            IJwtSettings jwtSettings,
                                            IAppHostsSettings appHostsSettings) : base(userManager, idConverter, queryProcessor, serverDispatcher, jwtSettings, appHostsSettings)
        {
        }

        protected override bool CanProcess(OpenIddictRequest request) => request.IsTokenExchangeGrantType();

        protected override async Task<PrincipalUserContext?> GetUserContext(OpenIddictRequest request)
        {
            var subjectToken = request.SubjectToken() ?? throw new GrantTypeRejectException(Errors.InvalidRequest, $"Parameter {RequestParameters.SubjectToken} was not provided");

            var tokenIdentity = await GetIdentity(subjectToken, "backoffice");
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
            var subMerchantId = userContext.Principal.SubMerchantId();
            if (string.IsNullOrWhiteSpace(subMerchantId))
                throw new GrantTypeRejectException(Errors.AccessDenied, $"Parameter {RequestParameters.RefreshToken} is invalid");

            return Task.FromResult<int?>(idConverter.FromPublicId(subMerchantId));
        }

        protected override Task<IEnumerable<Claim>> GetExtraClaims(OpenIddictRequest request, PrincipalUserContext userContext) => Task.FromResult<IEnumerable<Claim>>([]);
    }
}