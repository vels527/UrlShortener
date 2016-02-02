using System;
using System.Collections.Generic;
using System.Threading;
using Common;
using UrlShortenerService.Data;

namespace UrlServices
{
    class UrlDataStore
    {
        private IKeyValueStorageAccessor azureStore;
        private ICacheAccessor cacheStore;
        private int _retryInterval = 10;
        private int _retryCount = 2;

        private const string TableName = "ShortUrl";

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlDataStore"/> class.
        /// CacheMode specifies if we need in-memory cache or distributed redis cache
        /// </summary>
        /// <param name="dbConnString">The db conn string.</param>
        /// <param name="cacheConnString">The cache conn string.</param>
        /// <param name="cacheMode">The cache mode.</param>
        public UrlDataStore(string dbConnString, string cacheConnString, string cacheMode)
        {
            azureStore = new AzureKeyValueStorageAccessor(dbConnString, TableName);
            if (cacheMode == "local")
                cacheStore = new InMemoryCacheAccessor();
            else
                cacheStore = new RedisCacheAccessor(cacheConnString);    
        }

        /// <summary>
        /// Gets the fullUrl for specified short URL.
        /// </summary>
        /// <param name="shortUrl">The short URL.</param>
        /// <returns></returns>
        public string Get(string shortUrl)
        {
            string fullUrl = cacheStore.Get(shortUrl);
            if (fullUrl == null)
            {
                var item = RetryGet(shortUrl);
                if (item != null)
                {
                    fullUrl = item.FullUrl;
                    cacheStore.Set(shortUrl, fullUrl);
                }
            }

            return fullUrl;
        }

        /// <summary>
        /// Retries the get operation
        /// </summary>
        /// <param name="shortUrl">The short URL.</param>
        /// <returns></returns>
        /// <exception cref="AggregateException"></exception>
        private UrlItemTableEntity RetryGet(string shortUrl)
        {
            var exceptions = new List<Exception>();

            for (int retry = 0; retry < _retryCount; retry++)
            {
                try
                {
                    if (retry > 0)
                        Thread.Sleep(_retryInterval);
                    return azureStore.Retrieve<UrlItemTableEntity>(shortUrl, UrlItemTableEntity.TableRowKey);;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            throw new AggregateException(exceptions);
    
        }

        /// <summary>
        /// Sets the specified short URL as key with fullUrl value
        /// </summary>
        /// <param name="shortUrl">The short URL.</param>
        /// <param name="fullUrl">The full URL.</param>
        /// <returns></returns>
        public bool Set(string shortUrl, string fullUrl)
        {
            var item = new UrlItemTableEntity(shortUrl, fullUrl);
            return RetrySet(item);
        }

        /// <summary>
        /// Retries the set operation.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        /// <exception cref="AggregateException"></exception>
        private bool RetrySet(UrlItemTableEntity item)
        {
            var exceptions = new List<Exception>();

            for (int retry = 0; retry < _retryCount; retry++)
            {
                try
                {
                    if (retry > 0)
                        Thread.Sleep(_retryInterval);
                    return azureStore.Insert(item);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            throw new AggregateException(exceptions);

        }

    }
}
