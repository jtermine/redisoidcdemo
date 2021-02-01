using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace RedisOidcDemoHost.Redis
{
    public class JdtRedisCacheProvider
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IRedisCacheClient _redisCacheClient;

        public JdtRedisCacheProvider(ILogger logger, IConfiguration configuration, IRedisCacheClient redisCacheClient)
        {
            _logger = logger;
            _configuration = configuration;
            _redisCacheClient = redisCacheClient;
        }

        public async Task<JdtCacheResponse> TryGetStringValue(string cacheKey, CancellationToken token)
        {
            if (string.IsNullOrEmpty(cacheKey))
            {
                throw new JdtMemoryCacheException(
                    $"Cannot locate the Redis key because one of the required parameters is null > cacheKey: {cacheKey}.");
            }

            var redisKey = cacheKey.ToLowerInvariant();

            if (IsRedisDisabled())
            {
                _logger.Verbose("Redis is disabled, so {RedisKey} will not be retrieved", redisKey);
                return new JdtCacheResponse();
            }

            token.ThrowIfCancellationRequested();

            if (!await _redisCacheClient.Db0.ExistsAsync(redisKey)) return new JdtCacheResponse();

            token.ThrowIfCancellationRequested();

            var response = await _redisCacheClient.Db0.GetAsync<string>(redisKey);

            _logger.Verbose("Retrieved {RedisKey} from Redis", redisKey);

            return new JdtCacheResponse {Response = response, IsValid = true};
        }

        private bool IsRedisDisabled()
        {
            return _configuration.HasConfigSetting(ConfigConstants.DISABLE_REDIS, out var disableRedis) &&
                   disableRedis.CompareString("TRUE");
        }

        public async Task<bool> TryRemoveValue(string cacheKey, CancellationToken token)
        {
            if (string.IsNullOrEmpty(cacheKey))
            {
                throw new JdtMemoryCacheException(
                    $"Cannot locate the Redis key because one of the required parameters is null > cacheKey: {cacheKey}.");
            }

            var redisKey = cacheKey.ToLowerInvariant();

            if (IsRedisDisabled())
            {
                _logger.Verbose("Redis is disabled, so {RedisKey} will not be removed", redisKey);
                return false;
            }

            token.ThrowIfCancellationRequested();

            if (!await _redisCacheClient.Db0.ExistsAsync(redisKey))
            {
                _logger.Verbose(
                    "The key {RedisKey} does not exist in Redis, so this NxgMemoryCacheHelper.TryRemoveValue will return true without any changes to Redis",
                    redisKey);

                return true;
            }

            _logger.Verbose(
                "The key {RedisKey} was removed from Redis", redisKey);

            token.ThrowIfCancellationRequested();

            await _redisCacheClient.Db0.RemoveAsync(redisKey);

            return true;
        }

        public async Task<JdtCacheResponse<TT>> TryGetValue<TT>(string cacheKey, CancellationToken token)
            where TT : class, new()
        {
            if (string.IsNullOrEmpty(cacheKey))
            {
                throw new JdtMemoryCacheException(
                    $"Cannot locate the Redis key because one of the required parameters is null > cacheKey: {cacheKey}.");
            }

            var redisKey = cacheKey.ToLowerInvariant();

            if (IsRedisDisabled())
            {
                _logger.Verbose("Redis is disabled, so {RedisKey} will not be retrieved", redisKey);
                return new JdtCacheResponse<TT>();
            }

            token.ThrowIfCancellationRequested();

            if (!await _redisCacheClient.Db0.ExistsAsync(redisKey)) return new JdtCacheResponse<TT>();

            token.ThrowIfCancellationRequested();

            var response = await _redisCacheClient.Db0.GetAsync<TT>(redisKey);

            _logger.Verbose("Retrieved {RedisKey} from Redis", redisKey);

            return new JdtCacheResponse<TT> {Response = response, IsValid = true};
        }

        public async Task Set<TT>(string cacheKey, TT payload,
            CancellationToken token,
            int theTimeout = 0) where TT : class, new()
        {
            if (string.IsNullOrEmpty(cacheKey))
            {
                throw new JdtMemoryCacheException(
                    $"Cannot locate the Redis key because one of the required parameters is null > cacheKey: {cacheKey}.");
            }

            var redisKey = cacheKey.ToLowerInvariant();

            if (IsRedisDisabled())
            {
                _logger.Verbose("Redis is disabled, so {RedisKey} will not be retrieved", redisKey);
                return;
            }

            if (theTimeout == 0 && _configuration.HasConfigSetting(ConfigConstants.REDIS_CACHE_TIMEOUT_SECONDS,
                out var timeoutSeconds) && int.TryParse(timeoutSeconds, out var timeout))
            {
                theTimeout = timeout;
            }
            else
            {
                _logger.Warning(
                    "There is no valid timeout value in the setting : REDIS_CACHE_TIMEOUT_SECONDS");
            }

            token.ThrowIfCancellationRequested();

            await _redisCacheClient.Db0.AddAsync(redisKey, payload);
            await _redisCacheClient.Db0.UpdateExpiryAsync(redisKey, new TimeSpan(0, 0, theTimeout));

            _logger.Verbose("Set {RedisKey} to Redis", redisKey);
        }

        public async Task SetString(string cacheKey,
            string payload, CancellationToken token,
            int theTimeout = 0)
        {
            if (string.IsNullOrEmpty(cacheKey))
            {
                throw new JdtMemoryCacheException(
                    $"Cannot locate the Redis key because one of the required parameters is null > cacheKey: {cacheKey}.");
            }

            var redisKey = cacheKey.ToLowerInvariant();

            if (IsRedisDisabled())
            {
                _logger.Verbose("Redis is disabled, so {RedisKey} will not be retrieved", redisKey);
                return;
            }

            if (theTimeout == 0 && _configuration.HasConfigSetting(ConfigConstants.REDIS_CACHE_TIMEOUT_SECONDS,
                out var timeoutSeconds) && int.TryParse(timeoutSeconds, out var timeout))
            {
                theTimeout = timeout;
            }
            else
            {
                _logger.Warning(
                    "There is no valid timeout value in the setting : REDIS_CACHE_TIMEOUT_SECONDS");
            }

            token.ThrowIfCancellationRequested();
            await _redisCacheClient.Db0.AddAsync(redisKey, payload);
            await _redisCacheClient.Db0.UpdateExpiryAsync(redisKey, new TimeSpan(0, 0, theTimeout));

            _logger.Verbose("Set {RedisKey} to Redis", redisKey);
        }

        public async Task Clear(CancellationToken token)
        {
            if (IsRedisDisabled())
            {
                _logger.Verbose("Redis is disabled, so this Clear command will be aborted");
                return;
            }

            token.ThrowIfCancellationRequested();

            await _redisCacheClient.Db0.FlushDbAsync();

            _logger.Verbose("Redis cache flushed successfully");
        }
    }
}