using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// Specifies the Success/Failure status for operations
    /// </summary>
    public enum UrlServicesStatus
    {
        Success = 0,
        InvalidFullUrl = 1,
        InvalidShortUrl = 2,
        ErrorShortUrlCreation = 3,
        ErrorFullUrlNotFound = 4,
        UnknownError=5        
    }

}
