using Azure.Communication.Identity;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Teams.EmbeddedChat.ACS;
using Microsoft.Teams.EmbeddedChat.Models;
using Microsoft.Teams.EmbeddedChat.Utils;
using System;
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
        public EntityState GetEntityStateAsync([ActivityTrigger] IDurableActivityContext context, ILogger log)
        {
            // retrieves the entity state from the orchestration
            var requestData = context.GetInput<EntityState>();

            log.LogInformation($"Activity {Constants.GetEntityStateActivity} has started.");

            try
            {
                // Construct a new "TableServiceClient using a connection string.
                var tableService = new AzureDataTablesService<EntityState>(_appConfiguration.StorageConnectionString, _appConfiguration.AzureTableName);

                // check if the entity-user mapping already exists
                var entityState = (EntityState) tableService.GetEntityMapping(requestData.EntityId, requestData.UserId);

                return entityState;
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
                    RowKey = requestData.UserId,
                    EntityId = requestData.EntityId,
                    UserId = requestData.UserId,
                    ThreadId = requestData.ThreadId,
                    AcsUserId = userId,
                    AcsToken = accessToken,
                    TokenExpiresOn = expiresOn.ToString("F"),
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
                var entityState = (EntityState)tableService.GetEntityMapping(requestData.EntityId, requestData.UserId);

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
