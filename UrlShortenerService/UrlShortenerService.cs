using System;
using System.Data.HashFunction;
using System.Linq;
using Common.Exceptions;
using UrlShortenerService.Extensions;

namespace UrlServices
{
    /// <summary>
    /// UrlShortenerService is used by ASP.Net web FD to get fullUrl -> shortUrl and vice-versa
    /// </summary>
    public class UrlShortenerService
    {
        private readonly UrlDataStore _urlDataStore;
        private const int MaxTriesShortenUrl = 6;
        private const int MaxHashSize = 56;

        public UrlShortenerService(string connString, string cacheConnString, string cacheMode)
        {
            _urlDataStore = new UrlDataStore(connString, cacheConnString, cacheMode);
        }

        /// <summary>
        /// Shortens the URL.
        /// Algo:
        /// 1. Do Hash on the full Url
        /// 2. Try to see if Hash slot is available for the Base62 encoded chars from length 3-5
        /// 3. If our best effort to find empty hash slot is not possible, then try to add a random salt and check again
        /// 4. If after max tries, we have not been able to find empty hash slot then exit function
        /// </summary>
        /// <param name="fullUrl">The full URL.</param>
        /// <returns></returns>
        public string ShortenUrl(string fullUrl)
        {
            var murmurHash = new MurmurHash3();
            byte[] hash = murmurHash.ComputeHash(fullUrl);
            int count = 0;
            int length = 2;
            string urlHash = null;

            while (count < MaxTriesShortenUrl)
            {
                //First 3 tries, keep increasing Length from 3 to 5
                if (count < 3)
                {   //Try increasing length of hash to reduce collisions
                    length++;
                }
                else
                {
                    //Random salt to get different hash once we reach max length of 5
                    string salt = DateTime.Now.ToLongDateString();
                    hash = murmurHash.ComputeHash(fullUrl+salt, MaxHashSize);
                }

                var minHash = hash.Take(length).ToArray();
                urlHash = minHash.ToBase62();
                string existingFullUrl = _urlDataStore.Get(urlHash);

                if ((existingFullUrl == null && _urlDataStore.Set(urlHash, fullUrl))||(existingFullUrl != null && existingFullUrl.Equals(fullUrl)))
                {
                    break;
                }

                count++;
            }

            if (string.IsNullOrEmpty(urlHash))
                throw new ShortUrlHashSlotNotAvailableException("Full Url: " + fullUrl);

            return urlHash;
        }

        /// <summary>
        /// Expands the URL. Given a short Url, full url is retrieved from the Database/Cache
        /// </summary>
        /// <param name="shortUrl">The short URL.</param>
        /// <returns></returns>
        public string ExpandUrl(string shortUrl)
        {
            return _urlDataStore.Get(shortUrl);
        }

    }
}
