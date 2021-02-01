using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using RedisOidcDemoHost.Redis;
using Serilog;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Newtonsoft;

namespace RedisOidcDemoHost
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var redisConfiguration = _configuration.GetSection(ConfigConstants.REDIS).Get<RedisConfiguration>();

            if (redisConfiguration == null)
                throw new InvalidOperationException(
                    $"Unable to locate the [{ConfigConstants.REDIS}] configuration block.");

            var redisCacheConnectionPoolManager = new RedisCacheConnectionPoolManager(redisConfiguration);
            var serializer = new NewtonsoftSerializer();
            var redisCacheClient = new RedisCacheClient(redisCacheConnectionPoolManager, serializer,
                redisConfiguration);
            var redisCacheProvider = new JdtRedisCacheProvider(Log.Logger, _configuration, redisCacheClient);

            services.AddSingleton(redisConfiguration);
            services.AddSingleton<ISerializer>(serializer);
            services.AddSingleton<IRedisCacheClient, RedisCacheClient>();
            services.AddSingleton<IRedisCacheConnectionPoolManager>(redisCacheConnectionPoolManager);
            services.AddSingleton<IRedisCacheClient>(redisCacheClient);

            Log.Logger.Information("A Redis connection has been initialized for this service");

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    options.ClientId = _configuration["oidc:clientId"];
                    options.ClientSecret = _configuration["oidc:clientSecret"];
                    options.Authority = _configuration["oidc:authority"];
                    options.ResponseType = _configuration["oidc:responseType"];
                    options.GetClaimsFromUserInfoEndpoint = bool.TryParse(_configuration["oidc:getClaimsFromUserInfoEndpoint"], out var option) && option;
                    options.RequireHttpsMetadata = true;
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.SaveTokens = true;
                    
                    var stsDiscoveryEndpoint = _configuration[ConfigConstants.STS_DISCOVERY_ENDPOINT];

                    options.ConfigurationManager =
                        new ConfigurationManager<OpenIdConnectConfiguration>(stsDiscoveryEndpoint,
                            new JdtOpenIdRedisConfigurationRetriever(redisCacheProvider, Log.Logger));
                    
                    options.Validate();
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    var stsDiscoveryEndpoint = _configuration[ConfigConstants.STS_DISCOVERY_ENDPOINT];

                    options.ConfigurationManager =
                        new ConfigurationManager<OpenIdConnectConfiguration>(stsDiscoveryEndpoint,
                            new JdtOpenIdRedisConfigurationRetriever(redisCacheProvider, Log.Logger));

                    options.Audience = _configuration["oidc:audience"];
                    options.Authority = _configuration["oidc:authority"];
                });

            services.AddAuthorization(options =>
            {
                var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                    JwtBearerDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                defaultAuthorizationPolicyBuilder =
                    defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();

                var defaultAuthorizationPolicy = defaultAuthorizationPolicyBuilder.Build();

                options.DefaultPolicy = defaultAuthorizationPolicy;
                
                
                // oidc
                var oidcAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(OpenIdConnectDefaults.AuthenticationScheme);

                oidcAuthorizationPolicyBuilder =
                    oidcAuthorizationPolicyBuilder.RequireAuthenticatedUser();

                var oidcAuthorizationPolicy = oidcAuthorizationPolicyBuilder.Build();
                
                options.AddPolicy(OpenIdConnectDefaults.AuthenticationScheme, oidcAuthorizationPolicy);
                
                
                // bearer
                
                var bearerAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme);

                bearerAuthorizationPolicyBuilder =
                    bearerAuthorizationPolicyBuilder.RequireAuthenticatedUser();

                var bearerAuthorizationPolicy = bearerAuthorizationPolicyBuilder.Build();
                
                options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, bearerAuthorizationPolicy);
            });

            services.AddSingleton(redisCacheClient);
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IServiceProvider serviceProvider, IConfiguration configuration,
            IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseSerilogRequestLogging();

            app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}