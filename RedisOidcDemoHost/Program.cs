using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RedisOidcDemoHost.Redis;
using Serilog;

namespace RedisOidcDemoHost
{
    public static class Program
    {
        private static IConfiguration _config;

        public static void Main(string[] args)
        {
            _config = JdtConfiguration.LoadConfiguration();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_config)
                .CreateLogger();

            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices((_, collection) =>
                {
                    collection.AddSingleton(Log.Logger);
                })
                .ConfigureHostConfiguration(builder => { builder.AddConfiguration(_config); })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        }
    }
}