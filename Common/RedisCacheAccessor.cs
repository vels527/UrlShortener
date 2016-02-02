using System;
using Microsoft.WindowsAzure.Storage.Table;
using StackExchange.Redis;

namespace Common
{
    public class RedisCacheAccessor : ICacheAccessor
    {
        private IDatabase _cache;

        public RedisCacheAccessor(string connString)
        {
            var redisConnection = ConnectionMultiplexer.Connect(connString);
            _cache = redisConnection.GetDatabase();
    
        }

        public void Set(string key, string value)
        {
            _cache.StringSet(key, value);
        }

        public string Get(string key)
        {
            return _cache.StringGet(key);
        }

        public void Clear(string key)
        {
            _cache.KeyDelete(key);
        }
    }
}