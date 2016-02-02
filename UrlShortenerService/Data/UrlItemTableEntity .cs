using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using System.Globalization;

namespace UrlShortenerService.Data
{
    /// <summary>
    /// Azure Table Storage Entity class
    /// </summary>
    [Serializable]
    public class UrlItemTableEntity : TableEntity
    {
        public const string TableRowKey = "RowKey";

        public string FullUrl { get; set; }

        public DateTime? CreatedDateTime { get; set; }

        public UrlItemTableEntity()
        {
            
        }

        public UrlItemTableEntity(string shortUrl, string fullUrl)
        {
            this.PartitionKey = shortUrl;
            this.RowKey = UrlItemTableEntity.TableRowKey;
            this.FullUrl = fullUrl;
            this.CreatedDateTime = DateTime.UtcNow;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
