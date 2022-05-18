using Azure.Communication.Identity;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Teams.EmbeddedChat.ACS;
using Microsoft.Teams.EmbeddedChat.Models;
using Microsoft.Teams.EmbeddedChat.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Teams.EmbeddedChat.Activities
{
    public class EntityMappingActivity
    {
        private readonly AppSettings _appConfiguration;

        public EntityMappingActivity(IOptions<AppSettings> configuration)
        {
            _appConfiguration = configuration.Value;
        }

        /// <summary>
        /// The Activity to get the Entity State
        /// </summary>
        /// <param name="context"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(Constants.GetEntityStateActivity)]
        public IEnumerable<EntityState> GetEntityStateAsync([ActivityTrigger] IDurableActivityContext context, ILogger log)
        {
            // retrieves the entity state from the orchestration
            var requestData = context.GetInput<ChatInfoRequest>();

            log.LogInformation($"Activity {Constants.GetEntityStateActivity} has started.");

            var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            var tenant = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");

            log.LogInformation($"Client Id: {clientId}");
            log.LogInformation($"Tenant Id: {tenant}");

            if (String.IsNullOrEmpty(clientId))
            {
                log.LogError("Missing AZURE_CLIENT_ID environment variable");
                throw new ArgumentException("Missing AZURE_CLIENT_ID environment variable");
            }
            if (String.IsNullOrEmpty(clientSecret))
            {
                log.LogError("Missing AZURE_CLIENT_SECRET environment variable");
                throw new ArgumentException("Missing AZURE_CLIENT_SECRET environment variable");
            }
            if (String.IsNullOrEmpty(tenant))
            {
                log.LogError("Missing AZURE_TENANT_ID environment variable");
                throw new ArgumentException("Missing AZURE_TENANT_ID environment variable");
            }

            try
            {
                // Construct a new "TableServiceClient using a connection string.
                var tableService = new AzureDataTablesService<EntityState>(_appConfiguration.StorageConnectionString, _appConfiguration.AzureTableName);

                // return all the entities for the particular Entity Id
                return (IEnumerable<EntityState>) tableService.GetEntities(requestData.EntityId);
            }
            catch (System.Exception e)
            {
                log.LogError(e.Message);
                throw;
            }
        }

        /// <summary>
        /// The Activity to create a new entity state
        /// </summary>
        /// <param name="context"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(Constants.CreateEntityStateActivity)]
        public async Task<EntityState> CreateEntityStateActivity([ActivityTrigger] IDurableActivityContext context, ILogger log)
        {
            // retrieves the entity state from the orchestration
            var requestData = context.GetInput<EntityState>();

            if (requestData == null)
            {
                log.LogWarning("Request data cannot be null.");
                return null;
            }

            log.LogInformation($"Activity {Constants.CreateEntityStateActivity} has started.");

            try
            {
                // create ACS Communication Identity Client Service
                var comClient = new CommServices(new Uri(_appConfiguration.AcsEndpoint), 
                    new [] { CommunicationTokenScope.Chat });

                // Create user Id and ACS token
                var (userId, accessToken, expiresOn) = await comClient.CreateIdentityAndGetTokenAsync();

                // populate Entity State with the ACS User info
                var entityState = new EntityState()
                {
                    PartitionKey = requestData.EntityId,
                    RowKey = requestData.Owner,
                    EntityId = requestData.EntityId,
                    Owner = requestData.Owner,
                    ThreadId = requestData.ThreadId,
                    AcsUserId = userId,
                    AcsToken = accessToken,
                    TokenExpiresOn = expiresOn.ToString("F"),
                    Participants = requestData.Participants,
                };

                if (string.IsNullOrEmpty(entityState.PartitionKey) || string.IsNullOrEmpty(entityState.RowKey))
                    throw new System.Exception("One of the entity state primary keys is emtpy!");

                // Construct a new "TableServiceClient using a connection string.
                var tableService = new AzureDataTablesService<EntityState>(_appConfiguration.StorageConnectionString, _appConfiguration.AzureTableName);

                // add a new Entity into the state
                await tableService.AddEntityAsync(entityState);

                return entityState;
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
            }

            return null;
        }

        /// <summary>
        /// Updates existing entity
        /// </summary>
        /// <param name="context"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(Constants.UpdateEntityStateActivity)]
        public async Task<(bool updateStatus, EntityState updatedState)> UpdateEntityStateAsync([ActivityTrigger] IDurableActivityContext context, ILogger log)
        {
            // retrieves the entity from the orchestration
            var requestData = context.GetInput<EntityState>();

            log.LogInformation($"Activity {Constants.UpdateEntityStateActivity} has started.");

            try
            {
                // Construct a new "TableServiceClient using a connection string.
                var tableService = new AzureDataTablesService<EntityState>(_appConfiguration.StorageConnectionString, _appConfiguration.AzureTableName);

                // Retrieve the existing state data from the data store
                var entityState = (EntityState)tableService.GetEntityMapping(requestData.EntityId, requestData.Owner);

                // create ACS Communication Identity Client Service
                var comClient = new CommServices(new Uri(_appConfiguration.AcsEndpoint),
                    new[] { CommunicationTokenScope.Chat });

                // Refresh ACS Token and update the state
                var accessToken = await comClient.RefreshAccessToken(entityState.AcsUserId);
                entityState.AcsToken = accessToken.Token;
                entityState.TokenExpiresOn = accessToken.ExpiresOn.ToString("F");

                // update the existing entity state with the updated data
                await tableService.UpdateEntityAsync(entityState);

                return (true, entityState);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                return (false, requestData);
            }
        }
    }
}
