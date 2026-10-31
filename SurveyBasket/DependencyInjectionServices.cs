using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;

namespace Bookify;

public static class DependencyInjectionServices
{
    public static IHostApplicationBuilder AddDependencyInjectionServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.AddMappingServices();
        builder.AddFluentValidationServices();
        builder.AddPollServices();
        builder.AddOpenApiServices();
        builder.AddDatabaseServices();
        builder.AddAuthConfig();
        builder.AddAuditLogging();
        builder.AddRateLimitingServices();


        return builder;
    }
    /////////////////////////////////////////
    private static IHostApplicationBuilder AddMappingServices(this IHostApplicationBuilder builder)
    {

        var MappingConfigrution = TypeAdapterConfig.GlobalSettings;
        MappingConfigrution.Scan(Assembly.GetExecutingAssembly());
        builder.Services.AddSingleton<IMapper>(new Mapper(MappingConfigrution));
        return builder;
    }
    ////////////////////////////////////////
    private static IHostApplicationBuilder AddFluentValidationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddFluentValidationAutoValidation()
            .AddValidatorsFromAssemblyContaining<Program>();

        return builder;
    }
    /////////////////////////////////////
    private static IHostApplicationBuilder AddPollServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IPollService, PollService>();
        return builder;
    }
    ////////////////////////////////////
    private static IHostApplicationBuilder AddOpenApiServices(this IHostApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddOpenApi();
        }
        return builder;
    }
    /////////////////////////////////
    private static IHostApplicationBuilder AddDatabaseServices(this IHostApplicationBuilder builder)
    {
        var ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException
                        ("Connection String 'DefaultConnection' is not configured");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(ConnectionString));
        return builder;
    }
    //////////////////////////////////
    private static IHostApplicationBuilder AddAuthConfig(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IAuthService, AuthService>();

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var JwtSettings = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

        builder.Services.AddScoped<IJwtService, JwtService>();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(JwtSettings?.Key!)),
                ValidateIssuer = true,
                ValidIssuer = JwtSettings?.Issuer,
                ValidateAudience = true,
                ValidAudience = JwtSettings?.Audience,
                ValidateLifetime = true
            };
        });
        return builder;

    }
    /////////////////////////////////
    private static IHostApplicationBuilder AddAuditLogging(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ISecurityAuditService, SecurityAuditService>();
        builder.AddXForwarded();

        return builder;
    }
    ////////////////////////////////
    private static IHostApplicationBuilder AddXForwarded(this IHostApplicationBuilder builder)
    {
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            if (builder.Environment.IsDevelopment())
            {
                // في بيئة التطوير فقط: نثق في الجميع لتسهيل التجربة
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            }
            else
            {
                // في بيئة الإنتاج (Production):

                // 1. مسح الإعدادات الافتراضية أولاً
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();

                // 2. إضافة الـ IP الحقيقي الخاص بخادم الـ Proxy (مثل Nginx أو HAProxy)
                // إذا كان التطبيق والـ Proxy على نفس السيرفر، غالباً سيكون 127.0.0.1
                options.KnownProxies.Add(IPAddress.Parse("127.0.0.1"));

                // إذا كان الـ Proxy على سيرفر خارجي، نضع الـ IP الخاص به
                options.KnownProxies.Add(IPAddress.Parse("192.168.1.50")); // مثال

                /* 
                // إذا كنت تستخدم شبكة كاملة من الـ Load Balancers، يمكنك إضافة نطاق شبكة (CIDR):
                options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("10.0.0.0"), 24));
                */
            }
        });
        return builder;
    }
    ///////////////////////////////
    private static IHostApplicationBuilder AddRateLimitingServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            // Customize the response when blocked to inform the front end when to retry
            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString();
                }

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    Status = 429,
                    Title = "Too Many Requests",
                    Detail = "You have exceeded the allowed number of requests. Please wait a moment and then try again."
                }, cancellationToken);
            };

            // 1. Fixed Window
            options.AddPolicy("public-fixed", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 60,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            // 2. Sliding Window
            options.AddPolicy("auth-sliding", httpContext =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 6,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 3,
                        QueueLimit = 0
                    }));

            // 3. Token Bucket
            options.AddPolicy("api-bucket", httpContext =>
                RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
                    factory: _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 10,
                        TokensPerPeriod = 2,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                        AutoReplenishment = true,
                        QueueLimit = 0
                    }));

            // 4. Concurrency:
            options.AddPolicy("heavy-concurrency", httpContext =>
                RateLimitPartition.GetConcurrencyLimiter(
                    partitionKey: httpContext.User.Identity?.Name ?? "anon",
                    factory: _ => new ConcurrencyLimiterOptions
                    {
                        PermitLimit = 2,
                        QueueLimit = 1,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    }));
        });
        return builder;
    }
}
