using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Quivi.Application.Configurations;
using Quivi.Application.Pos;
using Quivi.Application.Pos.Invoicing;
using Quivi.Application.Services;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Domain.Repositories.EntityFramework.Identity;
using Quivi.Infrastructure;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Configurations;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Images;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Abstractions.Pos;
using Quivi.Infrastructure.Abstractions.Pos.EscPos;
using Quivi.Infrastructure.Abstractions.Pos.Invoicing;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Services;
using Quivi.Infrastructure.Abstractions.Services.Charges;
using Quivi.Infrastructure.Abstractions.Services.Mailing;
using Quivi.Infrastructure.Abstractions.Storage;
using Quivi.Infrastructure.Configurations;
using Quivi.Infrastructure.Converters;
using Quivi.Infrastructure.Cqrs;
using Quivi.Infrastructure.Events.RabbitMQ;
using Quivi.Infrastructure.Images.SixLabors.ImageSharp;
using Quivi.Infrastructure.Jobs.Hangfire.Extensions;
using Quivi.Infrastructure.Mailing.EmailEngine.Mjml;
using Quivi.Infrastructure.Mailing.SendGrid;
using Quivi.Infrastructure.Mailing.Smtp;
using Quivi.Infrastructure.Mapping;
using Quivi.Infrastructure.Pos.ESCPOS_NET;
using Quivi.Infrastructure.Pos.Facturalusa;
using Quivi.Infrastructure.Pos.Facturalusa.Abstractions;
using Quivi.Infrastructure.Pos.Facturalusa.Configurations;
using Quivi.Infrastructure.Repositories;
using Quivi.Infrastructure.Services;
using Quivi.Infrastructure.Services.Charges;
using Quivi.Infrastructure.Storage;
using Quivi.Infrastructure.Storage.Azure;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace Quivi.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterAll(this IServiceCollection serviceCollection, IConfiguration configuration, Action<JwtBearerOptions>? configureOptions = null)
        {
            LoadAllAssemblies();

            serviceCollection.AddHttpContextAccessor();
            serviceCollection.AddScoped<IPrincipal>(provider =>
            {
                var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
                return httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();
            });

            serviceCollection.RegisterSingleton<IAppHostsSettings>((p) => configuration.GetSection("AppHosts").Get<AppHostsSettings>()!);
            serviceCollection.AddDbContext<QuiviContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("Quivi");
                options.UseSqlServer(connectionString);
            });
            serviceCollection.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
            })
            //.AddUserValidator<SimpleUserValidator>()
            //.AddPasswordValidator<SimplePasswordValidator>()
            .AddEntityFrameworkStores<QuiviContext>()
            .AddDefaultTokenProviders();

            serviceCollection.RegisterSingleton<IDateTimeProvider, DateTimeProvider>();
            serviceCollection.RegisterSingleton<IRandomGenerator, RandomGenerator>();

            serviceCollection.RegisterScoped<ICommandProcessor, CommandProcessor>();
            serviceCollection.RegisterCommandHandlers();

            serviceCollection.RegisterScoped<IQueryProcessor, QueryProcessor>();
            serviceCollection.RegisterQueryHandlers();

            serviceCollection.RegisterScoped<IMapper, Mapper>();
            serviceCollection.RegisterMappersHandlers();

            serviceCollection.RegisterSingleton<IDefaultSettings>((p) => configuration.GetSection("DefaultSettings").Get<DefaultSettings>()!);
            serviceCollection.RegisterSingleton((p) => configuration.GetSection("Smtp").Get<SmtpSettings>()!);
            serviceCollection.RegisterSingleton((p) => configuration.GetSection("SendGrid").Get<SendGridSettings>()!);
            serviceCollection.RegisterScoped<IEmailService>(p =>
            {
                var settings = configuration.GetSection("Mailing").Get<MailingSettings>()!;
                if (settings.Provider.Equals("SendGrid", StringComparison.OrdinalIgnoreCase))
                {
                    var sendgridSettings = p.GetService<SendGridSettings>()!;
                    return new SendGridEmailService
                    {
                        ApiKey = sendgridSettings.ApiKey,
                        FromAddress = settings.FromAddress,
                        FromName = settings.FromName,
                    };
                }

                var smtpSettings = p.GetService<SmtpSettings>()!;
                return new SmtpEmailService
                {
                    FromAddress = settings.FromAddress,
                    FromName = settings.FromName,
                    Host = smtpSettings.Host,
                    Port = smtpSettings.Port,
                    Username = smtpSettings.Username,
                    Password = smtpSettings.Password,
                };
            });
            serviceCollection.RegisterSingleton<IHasher, Hasher>();
            serviceCollection.RegisterSingleton<IIdConverter>((p) =>
            {
                var salt = configuration["IdConverter:Salt"];
                if (salt == null)
                    throw new Exception("No IdConverver:Salt defined");
                return new IdConverter(salt);
            });

            serviceCollection.RegisterAuthentication(configuration, configureOptions);
            serviceCollection.RegisterEvents(configuration);
            serviceCollection.RegisterRepositories();
            serviceCollection.RegisterStorages(configuration);

            serviceCollection.RegisterScoped(p =>
            {
                var unitOfWork = p.GetService<SqlUnitOfWork>()!;
                var eventService = p.GetService<RabbitMQEventService>()!;
                return new CoordinatedUnitOfWork(unitOfWork, eventService);
            });
            serviceCollection.RegisterScoped<IUnitOfWork>(p => p.GetService<CoordinatedUnitOfWork>()!);
            serviceCollection.RegisterScoped<IEventService>(p => p.GetService<CoordinatedUnitOfWork>()!);
            serviceCollection.RegisterHangfireJobHandler(configuration.GetConnectionString("Quivi")!);

            serviceCollection.RegisterSingleton<IPasswordHasher, PasswordHasher>();
            serviceCollection.RegisterSingleton<ILogger, ConsoleLogger>();

            serviceCollection.RegisterPosSyncStrategies();
            serviceCollection.RegisterFacturalusa(configuration);

            //Register Default Invoicing
            serviceCollection.RegisterScoped<IInvoiceGateway>(p =>
            {
                var settings = configuration.GetSection("Invoicing").Get<InvoicingSettings>();
                if (settings?.Provider?.Equals("FacturaLusa", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var facturalusaSettings = p.GetService<IFacturalusaSettings>()!;
                    return new FacturalusaGateway(p.GetService<IFacturalusaServiceFactory>()!, facturalusaSettings.AccessToken, "Default")
                    {
                        CommandProcessor = p.GetService<ICommandProcessor>()!,
                        QueryProcessor = p.GetService<IQueryProcessor>()!
                    };
                }

                throw new NotImplementedException();
            });

            serviceCollection.RegisterChargeMethods();
            serviceCollection.RegisterScoped<IEscPosPrinterService, EscPosPrinterService>();
            serviceCollection.RegisterSingleton<IEmailEngine, MjmlEmailEngine>();

            return serviceCollection;
        }

        private static IServiceCollection RegisterFacturalusa(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.RegisterSingleton<IFacturalusaSettings>((p) => configuration.GetSection("Facturalusa").Get<FacturalusaSettings>()!);
            serviceCollection.RegisterSingleton<IFacturalusaCacheProvider, FacturalusaCacheProvider>();
            serviceCollection.RegisterSingleton<ICacheProvider, MemoryCacheProvider>();
            serviceCollection.RegisterScoped<IInvoiceGatewayFactory, InvoiceGatewayFactory>();
            serviceCollection.RegisterSingleton<IFacturalusaServiceFactory, FacturalusaServiceFactory>();
            serviceCollection.RegisterSingleton<IFacturalusaCacheProvider, FacturalusaCacheProvider>();

            return serviceCollection;
        }

        private static IServiceCollection RegisterStorages(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.RegisterSingleton<IImageProcessor, ImageSharpProcessor>();

            serviceCollection.RegisterSingleton<IFileSystemStorageSettings>((p) => configuration.GetSection("FileSystemStorage").Get<FileSystemStorageSettings>()!);

            //Register All IFileStorage
            serviceCollection.RegisterSingleton<FileSystemStorage>();
            serviceCollection.RegisterSingleton((p) =>
            {
                var config = configuration.GetSection("AzureBlobStorage").Get<AzureBlobStorageSettings>()!;
                return new AzureBlobStorage
                {
                    ConnectionString = config.ConnectionString,
                    VirtualDirectory = config.VirtualDirectory,
                };
            });

            //Register Default Storage
            serviceCollection.RegisterSingleton<IFileStorage>(p =>
            {
                var settings = configuration.GetSection("Storage").Get<StorageSettings>();
                if (settings?.Provider?.Equals("Azure", StringComparison.OrdinalIgnoreCase) == true)
                    return p.GetService<AzureBlobStorage>()!;

                return p.GetService<FileSystemStorage>()!;
            });

            //Register Collection
            serviceCollection.RegisterSingleton<IEnumerable<IFileStorage>>(p => [
                p.GetService<AzureBlobStorage>()!,
                p.GetService<FileSystemStorage>()!,
            ]);

            serviceCollection.RegisterSingleton<IStorageService, GenericStorageService>();
            return serviceCollection;
        }

        private static IServiceCollection RegisterRepositories(this IServiceCollection serviceCollection)
        {
            serviceCollection.RegisterScoped<SqlUnitOfWork>();

            var properties = typeof(IUnitOfWork).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .Where(p => p.PropertyType.GetInterfaces()
                                                            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRepository<,>))
                                                            .Any());
            foreach (var property in properties)
                serviceCollection.RegisterScoped(property.PropertyType, p =>
                {
                    var unitOfWork = p.GetService<IUnitOfWork>()!;
                    return property.GetValue(unitOfWork)!;
                });

            return serviceCollection;
        }

        private static IServiceCollection RegisterAuthentication(this IServiceCollection serviceCollection, IConfiguration configuration, Action<JwtBearerOptions>? configureOptions = null)
        {
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            serviceCollection.RegisterSingleton<IJwtSettings>((p) => configuration.GetSection("JwtSettings").Get<JwtSettings>()!);
            serviceCollection.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
                var hostsSettings = configuration.GetSection("AppHosts").Get<AppHostsSettings>()!;
                var certificateBytes = Convert.FromBase64String(jwtSettings.Certificate.Base64);
                var cert = new X509Certificate2(certificateBytes, jwtSettings.Certificate.Password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
                var authUri = new Uri(hostsSettings.OAuth, UriKind.Absolute).ToString();
                var tokenParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = new Uri(authUri, UriKind.Absolute).ToString(),
                    ValidAudiences = jwtSettings.Audiences,
                    IssuerSigningKey = new RsaSecurityKey(cert.GetRSAPublicKey()),
                    ClockSkew = TimeSpan.Zero,

                    RoleClaimType = "role",
                };

                options.Authority = authUri;
                options.RequireHttpsMetadata = options.Authority.StartsWith("https://");

                options.TokenValidationParameters = tokenParameters;
                configureOptions?.Invoke(options);
            });

            return serviceCollection;
        }

        private static IServiceCollection RegisterEvents(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.RegisterSingleton<IRabbitMqSettings>((p) => configuration.GetSection("RabbitMq").Get<RabbitMqSettings>()!);
            serviceCollection.RegisterSingleton<RabbitMQConnection>();

            serviceCollection.RegisterSingleton<RabbitMQEventService>();
            serviceCollection.RegisterSingleton((s) =>
            {
                var principalAssemblyName = Assembly.GetEntryAssembly()!.GetName().Name;
                var connection = s.GetService<RabbitMQConnection>()!;
                return new RabbitMQWorker(connection, principalAssemblyName!, s, false);
            });
            serviceCollection.AddHostedService((s) =>
            {
                var worker = s.GetService<RabbitMQWorker>()!;
                return new HostedService<RabbitMQWorker>(worker);
            });

            foreach (var item in GetInterfaceImplementations(typeof(IEventHandler<>)))
                serviceCollection.RegisterScoped(item.InterfaceType, item.ClassType);

            return serviceCollection;
        }

        private static IServiceCollection RegisterCommandHandlers(this IServiceCollection serviceCollection)
        {
            // Command Handlers without a result
            foreach (var item in GetInterfaceImplementations(typeof(ICommandHandler<>)))
                serviceCollection.RegisterScoped(item.InterfaceType, item.ClassType);

            // Command Handlers with a result
            foreach (var item in GetInterfaceImplementations(typeof(ICommandHandler<,>)))
                serviceCollection.RegisterScoped(item.InterfaceType, item.ClassType);

            return serviceCollection;
        }

        private static IServiceCollection RegisterQueryHandlers(this IServiceCollection serviceCollection)
        {
            // Query Handlers without a result
            foreach (var item in GetInterfaceImplementations(typeof(IQueryHandler<>)))
                serviceCollection.RegisterScoped(item.InterfaceType, item.ClassType);

            // Query Handlers with a result
            foreach (var item in GetInterfaceImplementations(typeof(IQueryHandler<,>)))
                serviceCollection.RegisterScoped(item.InterfaceType, item.ClassType);

            return serviceCollection;
        }

        private static IServiceCollection RegisterMappersHandlers(this IServiceCollection serviceCollection)
        {
            var mappersToRegister = GetInterfaceImplementations(typeof(IMapperHandler<,>));
            foreach (var item in mappersToRegister)
                serviceCollection.AddScoped(item.InterfaceType, item.ClassType);
            return serviceCollection;
        }

        private static IServiceCollection RegisterPosSyncStrategies(this IServiceCollection serviceColection)
        {
            serviceColection.RegisterScoped<GenericPosSyncService>();
            serviceColection.RegisterScoped<IPosSyncService, GenericPosSyncService>();

            var implementations = GetAllImplementations<IPosSyncStrategy>();
            foreach (var implementation in implementations)
                serviceColection.RegisterScoped(implementation);

            serviceColection.RegisterScoped<IEnumerable<IPosSyncStrategy>>(s =>
            {
                List<IPosSyncStrategy> result = new List<IPosSyncStrategy>();
                foreach (var implementation in implementations)
                    result.Add((IPosSyncStrategy)s.GetService(implementation)!);
                return result;
            });

            return serviceColection;
        }

        private static IServiceCollection RegisterChargeMethods(this IServiceCollection serviceCollection)
        {
            serviceCollection.RegisterSingleton<CashChargeProcessingStrategy>();
            serviceCollection.RegisterScoped<IEnumerable<IChargeProcessingStrategy>>(p => [
                p.GetService<CashChargeProcessingStrategy>()!,
            ]);
            return serviceCollection;
        }

        #region Registrations
        public static void RegisterSingleton<TService>(this IServiceCollection services) where TService : class
        {
            services.AddSingleton<TService>();
        }

        public static void RegisterSingleton<TService>(this IServiceCollection services, Func<IServiceProvider, TService> factory) where TService : class
        {
            services.AddSingleton(factory);
        }

        public static void RegisterSingleton<TService, TImplementation>(this IServiceCollection services) where TService : class
                                                                                                            where TImplementation : class, TService
        {
            services.AddSingleton<TService, TImplementation>();
            services.AddSingleton<TImplementation>();
        }

        public static void RegisterScoped<TService>(this IServiceCollection services) where TService : class
        {
            services.AddScoped<TService>();
        }

        public static void RegisterScoped<TService>(this IServiceCollection services, Func<IServiceProvider, TService> factory) where TService : class
        {
            services.AddScoped(factory);
        }

        public static void RegisterScoped<TService, TImplementation>(this IServiceCollection services) where TService : class
                                                                                                            where TImplementation : class, TService
        {
            services.AddScoped<TService, TImplementation>();
            services.AddScoped<TImplementation>();
        }

        public static void RegisterScoped(this IServiceCollection services, Type serviceType)
        {
            services.AddScoped(serviceType);
        }

        public static void RegisterScoped(this IServiceCollection services, Type serviceType, Type implementationType)
        {
            services.AddScoped(serviceType, implementationType);
            services.AddScoped(implementationType);
        }

        public static void RegisterScoped(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> factory)
        {
            services.AddScoped(serviceType, factory);
        }
        #endregion

        #region Auxiliar Methods
        private static void LoadAllAssemblies()
        {
            // Get the path to the application's output directory (bin/Debug/net8.0 or bin/Release/net8.0)
            var binFolderPath = AppDomain.CurrentDomain.BaseDirectory;

            // Get all the DLLs in the bin folder
            var assemblyFiles = Directory.GetFiles(binFolderPath, "*.dll");

            foreach (var assemblyFile in assemblyFiles)
            {
                try
                {
                    // Load the assembly from the path
                    Assembly.LoadFrom(assemblyFile);
                }
                catch (Exception)
                {
                }
            }
        }

        private static IEnumerable<(Type InterfaceType, Type ClassType)> GetInterfaceImplementations(Type referenceInterfaceType)
        {
            var classesTypes = AppDomain.CurrentDomain
                .GetSafeAssembliesTypes(a => !a.IsDynamic)
                .Where(t => t.IsClass)
                .Where(t => !t.IsAbstract)
                .Where(t => !t.IsGenericType)
                .Where(t => t.FilterTypeInterfaces(referenceInterfaceType).Any());

            foreach (var classType in classesTypes)
            {
                foreach (var interfaceType in classType.FilterTypeInterfaces(referenceInterfaceType))
                {
                    yield return (interfaceType, classType);
                }
            }
        }

        private static IEnumerable<Type> FilterTypeInterfaces(this Type type, Type interfaceType)
        {
            IEnumerable<Type> interfaces = type.GetInterfaces();

            interfaces = interfaceType.IsGenericType
                ? interfaces.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType)
                : interfaces.Where(i => i == interfaceType);

            return interfaces;
        }

        private static IEnumerable<Type> GetAllImplementations<T>()
        {
            var allTypes = AppDomain.CurrentDomain
                .GetSafeAssembliesTypes(a => !a.IsDynamic)
                .Where(t => t.IsClass)
                .Where(t => t.IsPublic)
                .Where(t => !t.IsAbstract)
                .Where(t => !t.IsInterface)
                .Where(t => t.GetInterface(typeof(T).Name) != null)
                .ToList();
            return allTypes;
        }
        #endregion
    }
}