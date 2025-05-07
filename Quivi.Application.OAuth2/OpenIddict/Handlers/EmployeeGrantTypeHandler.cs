using OpenIddict.Server;
using Quivi.Application.OAuth2.Extensions;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace Quivi.Application.OAuth2.OpenIddict.Handlers
{
    public class EmployeeGrantTypeHandler : IOpenIddictServerHandler<HandleTokenRequestContext>
    {
        private readonly IOpenIddictServerDispatcher serverDispatcher;

        public EmployeeGrantTypeHandler(IOpenIddictServerDispatcher serverDispatcher)
        {
            this.serverDispatcher = serverDispatcher;
        }

        public async ValueTask HandleAsync(HandleTokenRequestContext context)
        {
            if (context.Request.IsEmployeeGrantType() == false)
                return;

            if(context.Request.ClientId != "pos")
            {
                context.Reject(error: Errors.InvalidRequest, description: "Invalid audience.");
                return;
            }

            var subjectToken = context.Request.SubjectToken();
            if (string.IsNullOrEmpty(subjectToken))
            {
                context.Reject(error: Errors.InvalidRequest, description: "The subject_token is missing.");
                return;
            }

            bool isValid = await IsValidToken(context.Transaction, subjectToken);
            if (!isValid)
            {
                context.Reject(error: Errors.AccessDenied, description: "Invalid subject_token.");
                return;
            }
        }

        private async Task<bool> IsValidToken(OpenIddictServerTransaction transaction, string? subjectToken)
        {
            if (string.IsNullOrEmpty(subjectToken))
                return false;

            var validatedContext = new ValidateTokenContext(transaction)
            {
                Token = subjectToken,
            };
            await serverDispatcher.DispatchAsync(validatedContext);
            return validatedContext.Principal?.Identity?.IsAuthenticated == true;
        }
    }
}