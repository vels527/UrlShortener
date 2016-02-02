using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// KeyValueStorageAccessor interface, for storing and retrieving key value pairs
    /// </summary>
    public interface IKeyValueStorageAccessor
    {
        /// <summary>
        /// Insert an object into the Key Value Store
        /// </summary>
        bool Insert(ITableEntity keyValueEntity);

        /// <summary>
        /// Deletes the specified key value entity.
        /// </summary>
        /// <param name="keyValueEntity">The key value entity.</param>
        void Delete(ITableEntity keyValueEntity);

        ///// <summary>
        ///// Retrieve a particular object from the Key Value Store
        ///// </summary>
        T Retrieve<T>(string partitionKey, string rowKey) where T : class, ITableEntity, new();
    }
}
