using Microsoft.EntityFrameworkCore;
using Quivi.Application.Extensions;
using Quivi.Application.OAuth2.Extensions;

namespace Quivi.OAuth2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.RegisterAll(builder.Configuration);
            builder.Services.RegisterOAuth2(builder.Configuration);

            builder.Services.AddCors(options =>
            {
                var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
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

            app.UseDeveloperExceptionPage();

            app.UseForwardedHeaders();

            app.UseRouting();
            app.UseCors("CorsPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHttpsRedirection();
            app.MapControllers();
            app.MapDefaultControllerRoute();

            app.Run();
        }
    }
}
