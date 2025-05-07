using Hangfire.Dashboard;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Quivi.Hangfire.Hangfire
{
    public class BasicAuthenticationFilter : IDashboardAuthorizationFilter, IDashboardAsyncAuthorizationFilter
    {
        public required string User { get; set; }
        public required string Pass { get; set; }

        public bool Authorize([NotNull] DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            var header = httpContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(header))
                return false;

            var authHeader = header.ToString();
            if (!authHeader.StartsWith("Basic "))
                return false;

            var encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
            var encoding = Encoding.GetEncoding("iso-8859-1");
            var usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));

            var split = usernamePassword.Split(':');
            if (split.Length != 2)
                return false;

            var username = split[0];
            var password = split[1];

            return username == User && password == Pass;
        }

        public Task<bool> AuthorizeAsync([NotNull] DashboardContext context) => Task.FromResult(Authorize(context));
    }
}
