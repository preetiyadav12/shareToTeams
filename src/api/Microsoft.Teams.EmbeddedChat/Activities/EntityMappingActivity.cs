﻿using Azure.Communication.Identity;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Teams.EmbeddedChat.ACS;
using Microsoft.Teams.EmbeddedChat.Models;
using Microsoft.Teams.EmbeddedChat.Utils;
using Newtonsoft.Json;
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
        /// The Activity to get the list of Entity States
        /// </summary>
        /// <param name="context"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(Constants.GetEntityStateActivity)]
        public IEnumerable<EntityState> GetEntityStateAsync([ActivityTrigger] IDurableActivityContext context, ILogger log)
        {
            List<EntityState> entityStates = new List<EntityState>();

            // retrieves the entity state from the orchestration
            var requestData = context.GetInput<ChatInfoRequest>();

            log.LogInformation($"Activity {Constants.GetEntityStateActivity} has started.");

            try
            {
                // Construct a new "TableServiceClient using a connection string.
                var tableService = new AzureDataTablesService<EntityStateRecord>(_appConfiguration.StorageConnectionString, _appConfiguration.AzureTableName);

                // return all the entities for the particular Entity Id
                var entityRecords = (IEnumerable<EntityStateRecord>) tableService.GetEntities(requestData.EntityId);
                foreach (var record in entityRecords)
                {
                    entityStates.Add(new EntityState
                    {
                        Id = record.Id,
                        EntityId = record.EntityId,
                        IsSuccess = record.IsSuccess,
                        Owner = JsonConvert.DeserializeObject<Person>(record.Owner),
                        Participants = JsonConvert.DeserializeObject<Person[]>(record.Participants) ?? new List<Person>().ToArray(),
                        ChatInfo = JsonConvert.DeserializeObject<ChatInfo>(record.ChatInfo),
                        AcsInfo = JsonConvert.DeserializeObject<ACSInfo>(record.AcsInfo),
                        CorrelationId = record.CorrelationId
                    });
                }

                return entityStates;
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
                // Convert (serialize) the complex types into their JSON representation before writting into the storage
                var entityRecord = new EntityStateRecord
                {
                    PartitionKey = entityState.EntityId,
                    RowKey = entityState.Owner.Id,
                    EntityId = entityState.EntityId,
                    IsSuccess = entityState.IsSuccess,
                    Owner = JsonConvert.SerializeObject(entityState.Owner),
                    ChatInfo = JsonConvert.SerializeObject(entityState.ChatInfo),
                    AcsInfo = JsonConvert.SerializeObject(entityState.AcsInfo),
                    Participants = JsonConvert.SerializeObject(entityState.Participants),
                    CorrelationId = entityState.CorrelationId
                };

                // Construct a new "TableServiceClient using a connection string.
                var tableService = new AzureDataTablesService<EntityStateRecord>(
                    _appConfiguration.StorageConnectionString, _appConfiguration.AzureTableName);

                // add a new Entity into the state
                await tableService.AddEntityAsync(entityRecord);

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
                var tableService = new AzureDataTablesService<EntityStateRecord>(
                    _appConfiguration.StorageConnectionString, _appConfiguration.AzureTableName);

                if (entityState.AcsInfo.AcsToken == null)
                {
                    // create ACS Communication Identity Client Service
                    var comClient = new CommServices(_appConfiguration.AcsConnectionString,
                        new[] { CommunicationTokenScope.Chat });

                    // Refresh ACS Token and update the state
                    var accessToken = await comClient.RefreshAccessToken(entityState.AcsInfo.AcsUserId);
                    entityState.AcsInfo.AcsToken = accessToken.Token;
                    entityState.AcsInfo.TokenExpiresOn = accessToken.ExpiresOn.ToString("F");
                }

                // Convert (serialize) the complex types into their JSON representation before writting into the storage
                var entityRecord = new EntityStateRecord
                {
                    PartitionKey = entityState.EntityId,
                    RowKey = entityState.Owner.Id,
                    EntityId = entityState.EntityId,
                    IsSuccess = entityState.IsSuccess,
                    Owner = JsonConvert.SerializeObject(entityState.Owner),
                    ChatInfo = JsonConvert.SerializeObject(entityState.ChatInfo),
                    AcsInfo = JsonConvert.SerializeObject(entityState.AcsInfo),
                    Participants = JsonConvert.SerializeObject(entityState.Participants),
                    CorrelationId = entityState.CorrelationId
                };

                // update the existing entity state with the updated data
                await tableService.UpdateEntityAsync(entityRecord);

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
