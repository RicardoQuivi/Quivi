namespace Quivi.Hangfire.Hangfire
{
    public class AuthenticateMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticateMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Quivi Hangfire\"");
            return _next(context);
        }
    }
}
