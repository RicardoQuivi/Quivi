using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Quivi.Application.Queries.Employees;
using Quivi.Application.Queries.Merchants;
using Quivi.Domain.Entities.Merchants;
using Quivi.Domain.Repositories.EntityFramework.Identity;
using Quivi.Infrastructure.Abstractions.Configurations;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Claims;
using Quivi.Infrastructure.Extensions;
using Quivi.Application.OAuth2.Extensions;
using System.Data;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Quivi.OAuth2.Controllers
{
    public class AuthController : Controller
    {
        public readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IQueryProcessor queryProcessor;
        private readonly IIdConverter idConverter;
        private readonly IAppHostsSettings appHostsSettings;
        private readonly IJwtSettings jwtSettings;

        public AuthController(UserManager<ApplicationUser> userManager,
                                SignInManager<ApplicationUser> signInManager,
                                IQueryProcessor queryProcessor,
                                IIdConverter idConverter,
                                IAppHostsSettings appHostsSettings,
                                IJwtSettings jwtSettings)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.queryProcessor = queryProcessor;
            this.idConverter = idConverter;
            this.appHostsSettings = appHostsSettings;
            this.jwtSettings = jwtSettings;
        }

        [HttpPost("~/connect/token")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            
            if (request == null)
                throw new InvalidOperationException("No request provided");

            if (request.IsPasswordGrantType())
                return await AuthenticateWithCredentials(request);

            if (request.IsRefreshTokenGrantType())
                return await AuthenticateWithRefreshToken(request);

            if (request.IsTokenExchangeGrantType())
                return await AuthenticateTokenExchange(request);

            if (request.IsEmployeeGrantType())
                return await AuthenticateEmployee(request);

            throw new NotImplementedException("The specified grant is not implemented.");
        }

        private async Task<IActionResult> AuthenticateWithCredentials(OpenIddictRequest request)
        {
            var email = request.Username() ?? "";
            var password = request.Password() ?? "";

            var user = await GetUserByCredentials(email, password);
            if (user == null)
                return BadRequest();

            return await ProcessIdentity(user, async identity =>
            {
                await SetDefaultQuiviClaims(identity, user.Id);
            }, request.ClientId ?? "");
        }

        private async Task<IActionResult> AuthenticateWithRefreshToken(OpenIddictRequest request)
        {
            var refreshToken = request.RefreshToken() ?? "";
            var rawMerchantId = request.MerchantId();
            int? merchantId = string.IsNullOrWhiteSpace(rawMerchantId) ? null : idConverter.FromPublicId(rawMerchantId);

            var user = await GetUserByToken(refreshToken);
            if (user == null)
                return BadRequest();

            return await ProcessIdentity(user, async identity =>
            {
                await SetQuiviClaims(identity, user.Id, merchantId);
            }, request.ClientId ?? "");
        }

        private async Task<IActionResult> AuthenticateTokenExchange(OpenIddictRequest request)
        {
            var subjectToken = request.SubjectToken() ?? "";
            var tokenType = request.SubjectType() ?? "";

            var user = await GetUserByToken(subjectToken);
            if (user == null)
                return BadRequest();

            return await ProcessIdentity(user, async identity =>
            {
                await SetQuiviClaims(identity, user.Id, null);
            }, request.ClientId ?? "");
        }

        private async Task<IActionResult> AuthenticateEmployee(OpenIddictRequest request)
        {
            var subjectToken = request.SubjectToken() ?? "";
            var rawEmployeeId = request.Username() ?? "";
            var pinCode = request.Password() ?? "";

            if (string.IsNullOrWhiteSpace(rawEmployeeId))
                return BadRequest();

            var tokenIdentity = await GetIdentity(subjectToken, "pos");
            if (tokenIdentity == null)
                return BadRequest();

            var rawSubmerchantId = tokenIdentity.SubMerchantId();
            if (string.IsNullOrWhiteSpace(rawSubmerchantId))
                return BadRequest();

            var employee = await queryProcessor.Execute(new GetEmployeeByLoginAsyncQuery
            {
                Id = idConverter.FromPublicId(rawEmployeeId),
                MerchantId = idConverter.FromPublicId(rawSubmerchantId),
                PinCode = pinCode,
            });
            if(employee == null)
                return BadRequest();

            var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType, QuiviClaims.Email, QuiviClaims.Role);
            identity.AddClaims(tokenIdentity.Claims);

            identity.SetClaim(QuiviClaims.EmployeeId, rawEmployeeId);
            identity.SetDestinations(static claim => claim.Type switch
            {
                _ => [Destinations.AccessToken]
            });
            var principal = new ClaimsPrincipal(identity);
            principal.SetAudiences("pos");
            principal.SetScopes(Scopes.OpenId, Scopes.Profile, Scopes.Email, Scopes.OfflineAccess);

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        private async Task<IActionResult> ProcessIdentity(ApplicationUser user, Func<ClaimsIdentity, Task> identityFunc, string audience)
        {
            var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType, QuiviClaims.Email, QuiviClaims.Role);
            identity.SetClaim(QuiviClaims.UserId, idConverter.ToPublicId(user.Id));
            identity.SetClaim(QuiviClaims.Email, user.Email);

            await SetRoles(identity, user);
            await identityFunc(identity);

            identity.SetDestinations(static claim => claim.Type switch
            {
                _ => [Destinations.AccessToken]
            });
            var principal = new ClaimsPrincipal(identity);
            principal.SetAudiences(audience);
            principal.SetScopes(Scopes.OpenId, Scopes.Profile, Scopes.Email, Scopes.OfflineAccess);

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        private async Task<ApplicationUser?> GetUserByCredentials(string email, string password)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
                return null;

            var result = await signInManager.CheckPasswordSignInAsync(user, password, false);
            if (result.Succeeded == false)
                return null;

            return user;
        }

        private async Task<ApplicationUser?> GetUserByToken(string token)
        {
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (!result.Succeeded)
                return null;

            var identity = await GetIdentity(token, "backoffice");
            if (identity == null)
                return null;

            var rawUserId = identity.UserId();
            if (string.IsNullOrWhiteSpace(rawUserId) == true)
                return null;

            int id = idConverter.FromPublicId(rawUserId);
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return null;

            return user;
        }

        private async Task<ClaimsPrincipal?> GetIdentity(string token, string audience)
        {
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (!result.Succeeded)
                return null;

            var claims = result.Principal.Claims;
            if (result.Principal.Identity?.IsAuthenticated != true)
            {
                var tokenHandler = new JsonWebTokenHandler();
                var certificateBytes = Convert.FromBase64String(jwtSettings.Certificate.Base64);
                var cert = new X509Certificate2(certificateBytes, jwtSettings.Certificate.Password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
                var tokenValidationResult = await tokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = appHostsSettings.OAuth,
                    ValidAudience = audience,
                    IssuerSigningKey = new RsaSecurityKey(cert.GetRSAPublicKey()),
                });
                if (tokenValidationResult.IsValid == false)
                    return null;

                claims = tokenValidationResult.ClaimsIdentity.Claims;
            }

            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            return principal;
        }

        private async Task SetRoles(ClaimsIdentity identity, ApplicationUser user)
        {
            var roles = await userManager.GetRolesAsync(user);
            identity.AddClaims(roles.Select(r => new Claim(QuiviClaims.Role, r)));
        }

        private async Task SetQuiviClaims(ClaimsIdentity identity, int userId, int? merchantId)
        {
            if (merchantId.HasValue == false)
            {
                await SetDefaultQuiviClaims(identity, userId);
                return;
            }

            var merchant = await GetMerchantId(userId, merchantId.Value);
            identity.SetClaim(QuiviClaims.MerchantId, idConverter.ToPublicId(merchant.ParentMerchant?.Id ?? merchant.Id));

            if (merchant.ParentMerchant != null)
            {
                identity.SetClaim(QuiviClaims.ActivatedAt, merchant.ParentMerchant.TermsAndConditionsAcceptedDate.HasValue ? new DateTimeOffset(merchant.ParentMerchant.TermsAndConditionsAcceptedDate.Value.ToUniversalTime()).ToUnixTimeSeconds() : null);
                identity.SetClaim(QuiviClaims.SubMerchantId, idConverter.ToPublicId(merchant.Id));
            }
            else
            {
                identity.SetClaim(QuiviClaims.ActivatedAt, merchant.TermsAndConditionsAcceptedDate.HasValue ? new DateTimeOffset(merchant.TermsAndConditionsAcceptedDate.Value.ToUniversalTime()).ToUnixTimeSeconds() : null);
            }
        }

        private async Task SetDefaultQuiviClaims(ClaimsIdentity identity, int userId)
        {
            var merchant = await GetDefaultMerchantId(userId, null);
            if (merchant == null)
                return;

            identity.SetClaim(QuiviClaims.MerchantId, idConverter.ToPublicId(merchant.Id));
            identity.SetClaim(QuiviClaims.ActivatedAt, merchant.TermsAndConditionsAcceptedDate.HasValue ? new DateTimeOffset(merchant.TermsAndConditionsAcceptedDate.Value.ToUniversalTime()).ToUnixTimeSeconds() : null);

            var subMerchant = await GetDefaultMerchantId(userId, merchant.Id);
            if (subMerchant != null)
                identity.SetClaim(QuiviClaims.SubMerchantId, idConverter.ToPublicId(subMerchant.Id));
        }

        private async Task<Merchant> GetMerchantId(int userId, int merchantId)
        {
            var merchantQuery = await queryProcessor.Execute(new GetMerchantsAsyncQuery
            {
                ApplicationUserIds = [userId],
                Ids = [merchantId],
                PageSize = 1,
                IsDeleted = false,
                IncludeParentMerchant = true,
            });

            if (merchantQuery.Any() == false)
                throw new UnauthorizedAccessException();

            return merchantQuery.First();
        }

        private async Task<Merchant?> GetDefaultMerchantId(int userId, int? parentMerchantId)
        {
            var merchantQuery = await queryProcessor.Execute(new GetMerchantsAsyncQuery
            {
                ApplicationUserIds = [userId],
                ParentIds = parentMerchantId.HasValue ? [parentMerchantId.Value] : null,
                IsParentMerchant = parentMerchantId.HasValue == false,
                PageSize = 1,
                IsDeleted = false,
            });

            return merchantQuery.FirstOrDefault();
        }
    }
}