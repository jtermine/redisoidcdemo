using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace RedisOidcDemoHost.Redis
{
    public class JdtOpenIdRedisConfigurationRetriever : IConfigurationRetriever<OpenIdConnectConfiguration>
    {
        private readonly JdtRedisCacheProvider _jdtRedisCacheProvider;
        private readonly ILogger _logger;

        public JdtOpenIdRedisConfigurationRetriever(JdtRedisCacheProvider jdtRedisCacheProvider, ILogger logger)
        {
            _jdtRedisCacheProvider = jdtRedisCacheProvider;
            _logger = logger;
        }
        
        Task<OpenIdConnectConfiguration> IConfigurationRetriever<OpenIdConnectConfiguration>.GetConfigurationAsync(
            string address,
            IDocumentRetriever retriever,
            CancellationToken cancel)
        {
            return GetAsync(address, retriever, cancel);
        }

        // ReSharper disable once UnusedType.Local
        private class OpenIdRedisConfigurationRetrieverException : Exception
        {
            // ReSharper disable once UnusedMember.Local
            public OpenIdRedisConfigurationRetrieverException()
            {
            }

            public OpenIdRedisConfigurationRetrieverException(string message)
                : base(message)
            {
            }

            // ReSharper disable once UnusedMember.Local
            public OpenIdRedisConfigurationRetrieverException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        private async Task<OpenIdConnectConfiguration> GetAsync(string address, IDocumentRetriever retriever,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw LogHelper.LogArgumentNullException(nameof(address));

            if (retriever == null)
                throw LogHelper.LogArgumentNullException(nameof(retriever));

            var cacheObject = await _jdtRedisCacheProvider.TryGetStringValue(address, CancellationToken.None);

            var str1 = cacheObject.IsValid ? cacheObject.Response : string.Empty;

            if (string.IsNullOrEmpty(str1))
            {
                str1 = await retriever.GetDocumentAsync(address, cancellationToken).ConfigureAwait(false);

                await _jdtRedisCacheProvider.SetString(address, str1,
                    cancellationToken);
            }

            var contractResolver = new DefaultContractResolver {NamingStrategy = new SnakeCaseNamingStrategy()};
            var settings = new JsonSerializerSettings {ContractResolver = contractResolver};

            var openIdConnectConfiguration =
                JsonConvert.DeserializeObject<OpenIdConnectConfiguration>(str1, settings);

            if (openIdConnectConfiguration == null)
            {
                throw new InvalidOperationException("Unable to deserialize the OpenIdConnectConfiguration");
            }
            
            if (string.IsNullOrEmpty(openIdConnectConfiguration.JwksUri)) return openIdConnectConfiguration;

            _logger.Verbose("IDX21812: Retrieving json web keys from: '{JkwsUri}'",
                (object) openIdConnectConfiguration.JwksUri);

            var jsonCacheObject = await _jdtRedisCacheProvider.TryGetStringValue(
                openIdConnectConfiguration.JwksUri, cancellationToken);

            var str2 = jsonCacheObject.IsValid ? jsonCacheObject.Response : string.Empty;

            if (string.IsNullOrEmpty(str2))
            {
                str2 = await retriever.GetDocumentAsync(openIdConnectConfiguration.JwksUri, cancellationToken)
                    .ConfigureAwait(false);

                await _jdtRedisCacheProvider.SetString(
                    openIdConnectConfiguration.JwksUri, str2, cancellationToken);
            }

            _logger.Verbose("IDX21813: Deserializing json web keys: '{JwksUri}'",
                (object) openIdConnectConfiguration.JwksUri);

            openIdConnectConfiguration.JsonWebKeySet = JsonConvert.DeserializeObject<JsonWebKeySet>(str2);

            foreach (var signingKey in openIdConnectConfiguration.JsonWebKeySet
                .GetSigningKeys())
                openIdConnectConfiguration.SigningKeys.Add(signingKey);

            return openIdConnectConfiguration;
        }
    }
}