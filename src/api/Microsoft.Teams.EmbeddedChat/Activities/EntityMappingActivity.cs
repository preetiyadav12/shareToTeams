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
        public async Task<bool> CreateEntityStateActivity([ActivityTrigger] IDurableActivityContext context, ILogger log)
        {
            log.LogInformation($"Activity {Constants.CreateEntityStateActivity} has started.");

            // retrieves the entity state from the orchestration
            var entityState = context.GetInput<EntityState>();

            if (entityState == null)
            {
                log.LogWarning("Entity State cannot be null.");
                return false;
            }

            try
            {
                if (string.IsNullOrEmpty(entityState.PartitionKey) || string.IsNullOrEmpty(entityState.RowKey))
                    throw new System.Exception("One of the entity state primary keys is emtpy!");

                // Construct a new "TableServiceClient using a connection string.
                var tableService = new AzureDataTablesService<EntityState>(_appConfiguration.StorageConnectionString, _appConfiguration.AzureTableName);

                // add a new Entity into the state
                await tableService.AddEntityAsync(entityState);

                return true;
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                throw;
            }
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
            log.LogInformation($"Activity {Constants.UpdateEntityStateActivity} has started.");

            // retrieves the entity from the orchestration
            var entityState = context.GetInput<EntityState>();

            try
            {
                // Construct a new "TableServiceClient using a connection string.
                var tableService = new AzureDataTablesService<EntityState>(_appConfiguration.StorageConnectionString, _appConfiguration.AzureTableName);

                if (entityState.AcsInfo.AcsToken == null)
                {
                    // create ACS Communication Identity Client Service
                    var comClient = new CommServices(new Uri(_appConfiguration.AcsEndpoint),
                        new[] { CommunicationTokenScope.Chat });

                    // Refresh ACS Token and update the state
                    var accessToken = await comClient.RefreshAccessToken(entityState.AcsInfo.AcsUserId);
                    entityState.AcsInfo.AcsToken = accessToken.Token;
                    entityState.AcsInfo.TokenExpiresOn = accessToken.ExpiresOn.ToString("F");
                }

                // update the existing entity state with the updated data
                await tableService.UpdateEntityAsync(entityState);

                return (true, entityState);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                return (false, entityState);
            }
        }
    }
}
