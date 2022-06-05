using Azure.Communication.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Teams.EmbeddedChat.ACS;
using Microsoft.Teams.EmbeddedChat.Helpers;
using Microsoft.Teams.EmbeddedChat.Models;
using Microsoft.Teams.EmbeddedChat.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Teams.EmbeddedChat.Activities;

public static class EntityMappingActivity
{
    /// <summary>
    /// The Activity to get the list of Entity States
    /// </summary>
    /// <param name="context"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static IEnumerable<EntityState> GetEntityState(ChatInfoRequest requestData, ILogger logger, AppSettings appConfiguration)
    {
        var entityStates = new List<EntityState>();

        logger.LogInformation($"Activity {nameof(GetEntityState)} has started.");

        try
        {
            // Construct a new "TableServiceClient using a connection string.
            var tableService = new AzureDataTablesService<EntityStateRecord>(
                appConfiguration.StorageConnectionString, appConfiguration.AzureTableName);

            // return all the entities for the particular Entity Id
            var entityRecords = (IEnumerable<EntityStateRecord>) tableService.GetEntities(requestData.EntityId);
            foreach (var record in entityRecords)
            {
                entityStates.Add(new EntityState
                {
                    Id = record.Id,
                    EntityId = record.EntityId,
                    IsSuccess = record.IsSuccess,
                    Owner = SerializationHelper.Deserialize<Person>(record.Owner),
                    Participants = SerializationHelper.DeserializeList<Person>(record.Participants).ToArray(),
                    ChatInfo = SerializationHelper.Deserialize<ChatInfo>(record.ChatInfo),
                    AcsInfo = SerializationHelper.Deserialize<ACSInfo>(record.AcsInfo),
                    CorrelationId = record.CorrelationId
                });
            }

            return entityStates;
        }
        catch (System.Exception e)
        {
            logger.LogError(e.Message);
            throw;
        }
    }


    /// <summary>
    /// Updates existing entity
    /// </summary>
    /// <param name="context"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static async Task<(bool updateStatus, EntityState updatedState)> UpdateEntityStateAsync(
        EntityState entityState, AppSettings appConfiguration, ILogger logger)
    {
        logger.LogInformation($"Activity {nameof(UpdateEntityStateAsync)} has started.");

        try
        {
            // Construct a new "TableServiceClient using a connection string.
            var tableService = new AzureDataTablesService<EntityStateRecord>(
                appConfiguration.StorageConnectionString, appConfiguration.AzureTableName);

            if (entityState.AcsInfo.AcsToken == null)
            {
                // create ACS Communication Identity Client Service
                var comClient = new CommServices(appConfiguration.AcsConnectionString,
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
                IsSuccess = (bool)entityState.IsSuccess,
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
            logger.LogError(e.Message);
            return (false, entityState);
        }
    }


    /// <summary>
    /// The Activity to create a new Entity State
    /// </summary>
    /// <param name="context"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static async Task<EntityStateRecord> CreateEntityStateAsync(
        EntityState entityState, ILogger logger, AppSettings appConfiguration)
    {
        logger.LogInformation($"Activity {nameof(CreateEntityStateAsync)} has started.");

        if (string.IsNullOrEmpty(entityState.EntityId))
        {
            logger.LogError("Entity Id cannot be null!");
            return null;
        }

        try
        {
            // Convert (serialize) the complex types into their JSON representation before writting into the storage
            var entityRecord = new EntityStateRecord
            {
                PartitionKey = entityState.EntityId,
                RowKey = entityState.Owner.Id,
                EntityId = entityState.EntityId,
                IsSuccess = (bool)entityState.IsSuccess,
                Owner = JsonConvert.SerializeObject(entityState.Owner),
                ChatInfo = JsonConvert.SerializeObject(entityState.ChatInfo),
                AcsInfo = JsonConvert.SerializeObject(entityState.AcsInfo),
                Participants = JsonConvert.SerializeObject(entityState.Participants),
                CorrelationId = entityState.CorrelationId
            };

            // Construct a new "TableServiceClient using a connection string.
            var tableService = new AzureDataTablesService<EntityStateRecord>(
                appConfiguration.StorageConnectionString, appConfiguration.AzureTableName);

            // add a new Entity into the state
            await tableService.AddEntityAsync(entityRecord);

            return entityRecord;
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            throw;
        }
    }
}
