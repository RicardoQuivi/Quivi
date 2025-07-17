using Quivi.Application.Extensions;
using Quivi.Application.Filters;
using Quivi.Infrastructure.Apis;

namespace Quivi.Guests.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.RegisterAll(builder.Configuration);
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

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<InvalidModelStateExceptionHandler>();
            }).AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.Converters.Add(new OptionalApiJsonConverterFactory());
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}