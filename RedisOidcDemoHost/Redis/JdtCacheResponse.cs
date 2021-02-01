namespace RedisOidcDemoHost.Redis
{
    public class JdtCacheResponse
    {
        public bool IsValid { get; set; }
        public string Response { get; set; }
    }

    public class JdtCacheResponse<TT> where TT : new()
    {
        public bool IsValid { get; set; }
        public TT Response { get; set; }
    }
}