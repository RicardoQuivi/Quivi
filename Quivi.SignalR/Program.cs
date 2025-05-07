using Microsoft.AspNetCore.Authentication.JwtBearer;
using Quivi.Application.Extensions;
using Quivi.SignalR.Hubs.Backoffice;
using Quivi.SignalR.Hubs.Pos;

namespace Quivi.SignalR
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.RegisterAll(builder.Configuration, options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (string.IsNullOrEmpty(accessToken))
                            return Task.CompletedTask;

                        context.Token = accessToken;
                        return Task.CompletedTask;
                    },
                };
            });
            builder.Services.AddSignalR();

            builder.Services.AddCors(options =>
            {
                var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Enumerable.Empty<string>();
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder
                        .SetIsOriginAllowed(origin => allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            builder.Services.AddControllers();

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseCors("CorsPolicy");
            app.MapHub<BackofficeHub>("/backoffice");
            app.MapHub<PosHub>("/pos");
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
