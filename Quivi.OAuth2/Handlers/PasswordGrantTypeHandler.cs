using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using Quivi.Application.OAuth2;
using Quivi.Application.OAuth2.Extensions;
using Quivi.Domain.Repositories.EntityFramework.Identity;
using Quivi.Infrastructure.Abstractions.Configurations;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace Quivi.OAuth2.Handlers
{
    public class PasswordGrantTypeHandler : AGrantTypeHandler<NoPrincipalUserContext>, IOpenIddictServerHandler<HandleTokenRequestContext>
    {
        private readonly SignInManager<ApplicationUser> signInManager;

        public PasswordGrantTypeHandler(UserManager<ApplicationUser> userManager,
                                        SignInManager<ApplicationUser> signInManager,
                                        IIdConverter idConverter,
                                        IQueryProcessor queryProcessor,
                                        IOpenIddictServerDispatcher serverDispatcher,
                                        IJwtSettings jwtSettings,
                                        IAppHostsSettings appHostsSettings) : base(userManager, idConverter, queryProcessor, serverDispatcher, jwtSettings, appHostsSettings)
        {
            this.signInManager = signInManager;
        }

        protected override bool CanProcess(OpenIddictRequest request) => request.IsPasswordGrantType();

        protected override async Task<NoPrincipalUserContext?> GetUserContext(OpenIddictRequest request)
        {
            var email = request.Username() ?? throw new GrantTypeRejectException(Errors.InvalidRequest, $"Parameter {RequestParameters.Username} was not provided");
            var password = request.Password() ?? throw new GrantTypeRejectException(Errors.InvalidRequest, $"Parameter {RequestParameters.Password} was not provided");

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
                return null;

            var signInResult = await signInManager.CheckPasswordSignInAsync(user, password, false);
            if (signInResult.Succeeded == false)
                return null;

            var roles = await userManager.GetRolesAsync(user);
            return new NoPrincipalUserContext(user, roles);
        }

        protected override Task<int?> TargetMerchantId(OpenIddictRequest request, NoPrincipalUserContext userContext) => Task.FromResult<int?>(null);
        protected override Task<IEnumerable<Claim>> GetExtraClaims(OpenIddictRequest request, NoPrincipalUserContext userContext) => Task.FromResult<IEnumerable<Claim>>([]);
    }
}