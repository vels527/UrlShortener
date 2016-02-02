using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Utilities
    {
        public static bool ValidateFullUrl(string fullUrl, out string cleanedFullUrl)
        {
            cleanedFullUrl = fullUrl.Trim(new char[] { '/', ' ', '\\', '?' });

            if (!(cleanedFullUrl.StartsWith("http") || cleanedFullUrl.StartsWith("https")))
            {
                cleanedFullUrl = "http://" + cleanedFullUrl;
            }

            Uri uriResult;
            bool result = Uri.TryCreate(cleanedFullUrl, UriKind.Absolute, out uriResult)
                          && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            return result;
        }

        public static bool ValidateShortUrl(string shortUrl, string shortUrlPrefix, out string cleanedShortUrl)
        {
            cleanedShortUrl = shortUrl.Trim(new char[] { '/', ' ', '\\' });

            if (cleanedShortUrl.StartsWith("https"))
            {
                return false;
            }

            if (!cleanedShortUrl.StartsWith("http"))
            {
                cleanedShortUrl = "http://" + shortUrl;
            }

            if (!cleanedShortUrl.StartsWith(shortUrlPrefix))
            {
                return false;
            }

            Uri uriResult;
            bool result = Uri.TryCreate(cleanedShortUrl, UriKind.Absolute, out uriResult)
                          && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (result)
            {
                cleanedShortUrl = cleanedShortUrl.Substring(shortUrlPrefix.Length);
                if (cleanedShortUrl.Length > 6 || !cleanedShortUrl.All(Char.IsLetterOrDigit))
                {
                    result = false;
                }
            }

            return result;
        }
    }
}
