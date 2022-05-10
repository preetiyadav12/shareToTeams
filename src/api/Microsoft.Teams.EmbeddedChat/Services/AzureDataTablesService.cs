using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Sas;
using Azure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Teams.EmbeddedChat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Teams.EmbeddedChat.Utils
{
    public class AzureDataTablesService<T> where T : ITableEntity, new()
    {
        private readonly TableClient _tableClient;

        /// <summary>
        /// Constructor: create TableClient with Connection string 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableName"></param>
        public AzureDataTablesService(string connectionString, string tableName)
        {
            // Construct a new TableClient using a Connection string
            _tableClient = new TableClient(connectionString, tableName);

            // Create the table if it doesn't already exist to verify we've successfully authenticated.
            _tableClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Constructor: Shared Key Credential
        /// </summary>
        /// <param name="storageUri"></param>
        /// <param name="tableName"></param>
        /// <param name="accountName"></param>
        /// <param name="accountKey"></param>
        public AzureDataTablesService(Uri storageUri, string tableName, string accountName, string accountKey)
        {
            // Construct a new TableClient using Shared Key Credential
            _tableClient = new TableClient(
                storageUri, 
                tableName,
                new TableSharedKeyCredential(accountName, accountKey));

            // Create the table if it doesn't already exist to verify we've successfully authenticated.
            _tableClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Constructor: Shared Access Signature (SAS)
        /// </summary>
        /// <param name="storageUri"></param>
        /// <param name="tableName"></param>
        /// <param name="accountName"></param>
        /// <param name="accountKey"></param>
        /// <param name="expiresOn"></param>
        public AzureDataTablesService(Uri storageUri, string tableName, string accountName, string accountKey, DateTime expiresOn)
        {
            // Construct a new <see cref="TableServiceClient" /> using a <see cref="TableSharedKeyCredential" />.
            var credential = new TableSharedKeyCredential(accountName, accountKey);

            // Get TableService client
            var serviceClient = new TableServiceClient(
                storageUri,
                credential);

            // Build a shared access signature with the Write and Delete permissions and access to all service resource types.
            var sasUri = serviceClient.GenerateSasUri(
                TableAccountSasPermissions.Write | TableAccountSasPermissions.Delete,
                TableAccountSasResourceTypes.All,
                expiresOn);

            // Create the TableServiceClients using the SAS URI.
            var serviceClientWithSas = new TableServiceClient(sasUri);

            // A TableClient is needed to perform table-level operations like inserting and deleting entities within the table,
            // so it is ideal for dealing with only a specific table. 
            _tableClient = serviceClientWithSas.GetTableClient(tableName);

            // Create the table if it doesn't already exist to verify we've successfully authenticated.
            _tableClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Constructor: TokenCredential with integration with Azure AD 
        /// To access a table resource with a TokenCredential, the authenticated identity should have 
        /// either the "Storage Table Data Contributor" or "Storage Table Data Reader" role.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableName"></param>
        public AzureDataTablesService(Uri storageUri, string tableName)
        {
            // Construct a new TableClient using a TokenCredential.
            _tableClient = new TableClient(
                storageUri,
                tableName,
                new DefaultAzureCredential());

            // Create the table if it doesn't already exist to verify we've successfully authenticated.
            _tableClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();
        }


        /// <summary>
        /// Delete table asynchronously
        /// </summary>
        /// <returns></returns>
        public async Task DeleteTable()
        {
            await _tableClient.DeleteAsync();
        }

        /// <summary>
        /// Get all entities by their partition key
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        public IEnumerable<IBaseTableEntity> GetPartition(string partitionKey)
        {
            var queryResultsLINQ = _tableClient.Query<EntityState>(ent => ent.PartitionKey == partitionKey);

            return queryResultsLINQ.ToList();
        }

        /// <summary>
        /// Get Entity by its partition key
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        public IBaseTableEntity GetEntity(string partitionKey)
        {
            var queryResultsLINQ = _tableClient.Query<EntityState>(ent => ent.PartitionKey == partitionKey);

            return queryResultsLINQ.FirstOrDefault();
        }

        /// <summary>
        /// Add entity to the table
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task AddEntityAsync(IBaseTableEntity entity)
        {
            // Add the newly created entity.
            await _tableClient.AddEntityAsync(entity);
        }

        /// <summary>
        /// Add entity to the table
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task UpdateEntityAsync(IBaseTableEntity entity)
        {
            // Add the newly created entity.
            await _tableClient.UpdateEntityAsync(entity, ETag.All);
        }

        /// <summary>
        /// Delete entity by its partition and row keys
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="rowKey"></param>
        /// <returns></returns>
        public async Task DeleteEntityAsync(string partitionKey, string rowKey)
        {
            // Add the newly created entity.
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

    }
}
