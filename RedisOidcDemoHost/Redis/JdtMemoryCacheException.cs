using System;

namespace RedisOidcDemoHost.Redis
{
    public class JdtMemoryCacheException : Exception
    {
        public JdtMemoryCacheException(string message)
            : base(message)
        {
        }
    }
}