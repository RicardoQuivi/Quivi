using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using Quivi.Application.OAuth2;
using Quivi.Application.OAuth2.Extensions;
using Quivi.Application.Queries.Employees;
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
    public class EmployeeGrantTypeHandler : AGrantTypeHandler<PrincipalUserContext>, IOpenIddictServerHandler<HandleTokenRequestContext>
    {
        public EmployeeGrantTypeHandler(UserManager<ApplicationUser> userManager,
                                            SignInManager<ApplicationUser> signInManager,
                                            IIdConverter idConverter,
                                            IQueryProcessor queryProcessor,
                                            IOpenIddictServerDispatcher serverDispatcher,
                                            IJwtSettings jwtSettings,
                                            IAppHostsSettings appHostsSettings) : base(userManager, idConverter, queryProcessor, serverDispatcher, jwtSettings, appHostsSettings)
        {
        }

        protected override bool CanProcess(OpenIddictRequest request) => request.IsEmployeeGrantType();

        protected override async Task<PrincipalUserContext?> GetUserContext(OpenIddictRequest request)
        {
            if (request.ClientId != "pos")
                throw new GrantTypeRejectException(Errors.InvalidRequest, "Invalid audience");

            var subjectToken = request.SubjectToken() ?? "";
            if (string.IsNullOrEmpty(subjectToken))
                throw new GrantTypeRejectException(Errors.InvalidRequest, $"Parameter {RequestParameters.SubjectToken} is missing");

            var tokenIdentity = await GetIdentity(subjectToken, "pos");
            if (tokenIdentity == null)
                throw new GrantTypeRejectException(Errors.InvalidRequest, $"Parameter {RequestParameters.SubjectToken} is invalid");

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
            var rawSubmerchantId = userContext.Principal.SubMerchantId();
            if (string.IsNullOrWhiteSpace(rawSubmerchantId))
                throw new GrantTypeRejectException(Errors.AccessDenied, $"Parameter {RequestParameters.SubjectToken} does not have a {QuiviClaims.EmployeeId} claim");

            return Task.FromResult<int?>(idConverter.FromPublicId(rawSubmerchantId));
        }

        protected override async Task<IEnumerable<Claim>> GetExtraClaims(OpenIddictRequest request, PrincipalUserContext userContext)
        {
            var rawEmployeeId = request.Username() ?? "";
            if (string.IsNullOrWhiteSpace(rawEmployeeId))
                throw new GrantTypeRejectException(Errors.AccessDenied, $"Parameter {RequestParameters.Username} is missing");

            var rawSubmerchantId = userContext.Principal.SubMerchantId() ?? "";
            var pinCode = request.Password() ?? "";
            Domain.Entities.Pos.Employee? employee = await GetEmployee(rawEmployeeId, pinCode, rawSubmerchantId, userContext.Principal.IsAdmin());
            if (employee == null)
                throw new GrantTypeRejectException(Errors.AccessDenied, $"Invalid credentials");

            return
            [
                new Claim(QuiviClaims.EmployeeId, idConverter.ToPublicId(employee.Id)),
            ];
        }

        private async Task<Domain.Entities.Pos.Employee?> GetEmployee(string rawEmployeeId, string pinCode, string rawSubmerchantId, bool isAdmin)
        {
            int merchantId = idConverter.FromPublicId(rawSubmerchantId);
            int employeeId = idConverter.FromPublicId(rawEmployeeId);
            if (isAdmin)
            {
                var employeesQuery = await queryProcessor.Execute(new GetEmployeesAsyncQuery
                {
                    MerchantIds = [merchantId],
                    Ids = [employeeId],
                    PageSize = 1,
                });
                return employeesQuery.SingleOrDefault();
            }

            return await queryProcessor.Execute(new GetEmployeeByLoginAsyncQuery
            {
                Id = employeeId,
                MerchantId = merchantId,
                PinCode = pinCode,
            });
        }
    }
}