using System;
using Microsoft.Extensions.Configuration;

namespace RedisOidcDemoHost.Redis
{
    public static class JdtConfiguration
    {
        public static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder();

            builder.AddEnvironmentVariables();
            builder.AddJsonFile("appsettings.json", true, true);

            var configuration = builder.Build();

            return configuration;
        }

        private static bool GetConfiguration(this IServiceProvider serviceProvider, out IConfiguration configuration)
        {
            configuration = null;

            var config = serviceProvider?.GetService(typeof(IConfiguration));

            if (config is not IConfiguration config1) return false;

            configuration = config1;
            return true;
        }

        public static bool HasConfigSetting(this IServiceProvider serviceProvider, string settingName,
            out string returnValue)
        {
            returnValue = string.Empty;

            if (serviceProvider == null || string.IsNullOrEmpty(settingName) ||
                !serviceProvider.GetConfiguration(out var config)) return false;

            var hasConfigSetting = config.HasConfigSetting(settingName, out var configSettingValue);

            if (hasConfigSetting)
            {
                returnValue = configSettingValue;
            }

            return hasConfigSetting;
        }

        public static bool HasConfigSetting(this IConfiguration config, string settingName, out string returnValue)
        {
            returnValue = string.Empty;

            if (config == null || string.IsNullOrEmpty(settingName)) return false;

            if (string.IsNullOrEmpty(config[settingName]) ||
                config[settingName].Equals("null", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            returnValue = config[settingName];
            return true;
        }
    }
}