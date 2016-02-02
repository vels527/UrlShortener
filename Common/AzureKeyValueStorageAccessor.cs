using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using System.Reflection;

namespace Common
{
    public class AzureKeyValueStorageAccessor : IKeyValueStorageAccessor
    {

        #region ////////// - Private Members - //////////

        private CloudTable Table;

        #endregion

        #region ////////// - Public Methods - //////////

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureKeyValueStorageAccessor" /> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="tableName">Name of the table.</param>
        public AzureKeyValueStorageAccessor(string connectionString, string tableName)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            this.Table = tableClient.GetTableReference(tableName);
            this.Table.CreateIfNotExists();
        }

        /// <summary>
        /// Inserts the specified key value entity.
        /// </summary>
        /// <param name="keyValueEntity">The key value entity.</param>
        public bool Insert(ITableEntity keyValueEntity)
        {
            TableOperation tableOperation = TableOperation.Insert(keyValueEntity);
            TableResult tableResult = this.Table.Execute(tableOperation);
            return tableResult.HttpStatusCode == 204;
        }

        /// <summary>
        /// Insert a set of objects into the Azure Store
        /// </summary>
        /// <param name="keyValueEntities"></param>
        public void Insert(IEnumerable<ITableEntity> keyValueEntities)
        {
            // Create the batch operation.
            var batchOperation = new TableBatchOperation();
            foreach (var tableEntity in keyValueEntities)
            {
                batchOperation.Insert(tableEntity);
            }
            this.Table.ExecuteBatch(batchOperation);
        }

        /// <summary>
        /// Insert a TableEntity object into Azure Table Storage, replace the object if it already exists
        /// </summary>
        public void InsertOrReplace(ITableEntity keyValueEntity)
        {
            TableOperation tableOperation = TableOperation.InsertOrReplace(keyValueEntity);
            TableResult tableResult = this.Table.Execute(tableOperation);

            //TODO: use tableResult to find the failure cases (if any) and take appropriate action.
        }

        /// <summary>
        /// Replaces the specified key value entity.
        /// </summary>
        /// <param name="keyValueEntity">The key value entity.</param>
        public void Replace(ITableEntity keyValueEntity)
        {
            ITableEntity tableEntity = keyValueEntity;

            TableOperation tableOperation = TableOperation.Replace(tableEntity);
            TableResult tableResult = this.Table.Execute(tableOperation);
        }

        /// <summary>
        ///  Insert a set of objects into the Key Value Store,  replace objects if it already exists
        /// </summary>
        /// <param name="keyValueEntities"></param>
        public void InsertOrReplace(IEnumerable<ITableEntity> keyValueEntities)
        {
            // Create the batch operation.
            var batchOperation = new TableBatchOperation();
            foreach (var tableEntity in keyValueEntities)
            {
                batchOperation.InsertOrReplace(tableEntity);
            }
            this.Table.ExecuteBatch(batchOperation);
        }

        /// <summary>
        /// Inserts or Merges the entity data whichever has changed.
        /// Note that in this case, if you want to reset something to a default value like null,
        /// Azure Table Storage will think that there is no Merge required. InsertOrReplace is better in those cases.
        /// </summary>
        /// <param name="keyValueEntity">The key value entity.</param>
        public void InsertOrMerge(ITableEntity keyValueEntity)
        {
            TableOperation tableOperation = TableOperation.InsertOrMerge(keyValueEntity);
            this.Table.Execute(tableOperation);
        }

        /// <summary>
        /// Merges the specified key value entity.
        /// </summary>
        /// <param name="keyValueEntity">The key value entity.</param>
        public void Merge(ITableEntity keyValueEntity)
        {
            TableOperation tableOperation = TableOperation.Merge(keyValueEntity);
            this.Table.Execute(tableOperation);
        }

        /// <summary>
        /// Deletes the specified key value entity.
        /// </summary>
        /// <param name="keyValueEntity">The key value entity.</param>
        public void Delete(ITableEntity keyValueEntity)
        {
            TableOperation tableOperation = TableOperation.Delete(keyValueEntity);
            this.Table.Execute(tableOperation);
        }

        /// <summary>
        /// Deletes the specified key value entity.
        /// </summary>
        /// <param name="keyValueEntity">The key value entity.</param>
        public Task DeleteAsync(ITableEntity keyValueEntity)
        {
            TableOperation tableOperation = TableOperation.Delete(keyValueEntity);
            return this.Table.ExecuteAsync(tableOperation);
        }

        /// <summary>
        /// Retrieves All the rows in the Azure Table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> RetrieveAll<T>() where T : class, ITableEntity, new()
        {
            TableQuery query = new TableQuery().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.NotEqual, string.Empty));

            return this.Table.ExecuteQuery(query, (pk, rk, ts, props, etag) => EntityResolver<T>(props, etag));
        }

        /// <summary>
        /// Retrieves the specified partition key.
        /// ETag is needed for performing Replace or Merge operations
        /// </summary>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns></returns>
        //public IEnumerable<T> Retrieve<T>(string partitionKey) where T : class, ITableEntity, new()
        //{
        //    TableQuery query = new TableQuery().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

        //    return this.Table.ExecuteQuery(query).ToList();//, (pk, rk, ts, props, etag) => EntityResolver<T>(props, etag));
        //}

        /// <summary>
        /// Retrieves the specified partition key.
        /// </summary>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="rowKey">The row key.</param>
        /// <returns></returns>
        public T Retrieve<T>(string partitionKey, string rowKey) where T : class, ITableEntity, new()
        {
            var retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            TableResult tableResult = this.Table.Execute(retrieveOperation);
            return tableResult.Result as T;
        }

        #endregion

        #region ////////// - Protected Methods - //////////

        /// <summary>
        /// Map ITableEntity to IKeyValueEntity
        /// Enums are not supported by ITableEntity so we take private members to map integer values of enums
        /// </summary>
        /// <typeparam name="TKeyValueStorageEntity">The type of the key value storage entity.</typeparam>
        /// <param name="storageProps">The storage props.</param>
        /// <param name="eTag">The e tag.</param>
        /// <returns></returns>
        protected static TKeyValueStorageEntity EntityResolver<TKeyValueStorageEntity>(IDictionary<string, EntityProperty> storageProps, string eTag)
        {
            var newKeyValueEntity = (TKeyValueStorageEntity)Activator.CreateInstance(typeof(TKeyValueStorageEntity));

            if (storageProps != null)
            {
                storageProps.Add("ETag", new EntityProperty(eTag));

                var emptyObjectProps = newKeyValueEntity.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                foreach (var prop in storageProps)
                {
                    PropertyInfo match = emptyObjectProps.FirstOrDefault(v => v.Name == prop.Key);
                    if (match != null)
                    {
                        if (match.PropertyType == typeof(Int32) || match.PropertyType == typeof(Int32?))
                        {
                            match.SetValue(newKeyValueEntity, storageProps[match.Name].Int32Value, null);
                        }
                        if (match.PropertyType == typeof(Int64) || match.PropertyType == typeof(Int64?))
                        {
                            match.SetValue(newKeyValueEntity, storageProps[match.Name].Int64Value, null);
                        }
                        if (match.PropertyType == typeof(string))
                        {
                            match.SetValue(newKeyValueEntity, storageProps[match.Name].StringValue, null);
                        }
                        if (match.PropertyType == typeof(bool) || match.PropertyType == typeof(bool?))
                        {
                            match.SetValue(newKeyValueEntity, storageProps[match.Name].BooleanValue, null);
                        }
                        if (match.PropertyType == typeof(DateTime) || match.PropertyType == typeof(DateTime?))
                        {
                            match.SetValue(newKeyValueEntity, storageProps[match.Name].DateTimeOffsetValue.Value.DateTime, null);
                        }

                        //The following types are not used currently by ProductOfferItem or MerchantItem
                        //Adding them for completeness sake and for future support
                        if (match.PropertyType == typeof(byte[]))
                        {
                            match.SetValue(newKeyValueEntity, storageProps[match.Name].BinaryValue, null);
                        }
                        if (match.PropertyType == typeof(double) || match.PropertyType == typeof(double?))
                        {
                            match.SetValue(newKeyValueEntity, storageProps[match.Name].DoubleValue, null);
                        }
                        if (match.PropertyType == typeof(Guid) || match.PropertyType == typeof(Guid?))
                        {
                            match.SetValue(newKeyValueEntity, storageProps[match.Name].GuidValue, null);
                        }
                        if (match.PropertyType == typeof(object))
                        {
                            match.SetValue(newKeyValueEntity, storageProps[match.Name].PropertyAsObject, null);
                        }
                    }
                }
            }

            return newKeyValueEntity;
        }

        #endregion
    }
}
