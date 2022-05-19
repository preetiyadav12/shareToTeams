using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Teams.EmbeddedChat.Models;
using System.Collections.Generic;

namespace Microsoft.Teams.EmbeddedChat
{
    public class ChatEntityOrchestration
    {
        /// <summary>
        /// Entity State Orchestration
        /// </summary>
        /// <param name="context"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(Constants.Orchestration)]
        public async Task<EntityState> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            // retrieves the request data
            var orchestrationRequest = context.GetInput<OrchestrationRequest>();
            orchestrationRequest.Request.Id = context.InstanceId;
            var requestData = orchestrationRequest.Request;

            if (!context.IsReplaying)
            {
                log.LogWarning($"Started Entity State Orchestration with Instance Id: {requestData.Id} for the entity id {requestData.EntityId}.");
            }

            switch (orchestrationRequest.Operation)
            {
                case ApiOperation.GetEntityState:
                    // Check if the mapping exists for this entity Id by invoking a separate Activity Function.
                    var entities = await context.CallActivityAsync<IEnumerable<EntityState>>(Constants.GetEntityStateActivity, requestData);

                    if (!context.IsReplaying)
                    {
                        log.LogWarning($"{Constants.GetEntityStateActivity} Activity completed for the entity id {requestData.EntityId}.");
                    }

                    if (entities.Any())
                    {
                        // Get the updated list of participants for each entity
                        // TODO
                        var updatedEntities = entities;

                        // at least one entity mapping found
                        // now we'll check if this user was the owner of one of the entities in the list
                        var entityState = updatedEntities.FirstOrDefault(
                            e => e.Participants.Any(p => p.Username == requestData.Username));

                        if (entityState != null) // This user is one of the participants in this entity chat!
                        {
                            // If the ACS Token has expired, we'll refresh it and then update the state
                            if (DateTime.Parse(entityState.TokenExpiresOn).CompareTo(context.CurrentUtcDateTime) < 0)
                            {
                                log.LogWarning("ACS Token has expired. Refreshing the token and returning the updated state...");

                                // update the entity state with the refreshed token
                                var (updateStatus, updatedState) = await context.CallActivityAsync<(bool, EntityState)>(Constants.UpdateEntityStateActivity, requestData);
                                if (updateStatus == false)
                                {
                                    log.LogError($"Failed to update the entity with Id: {requestData.EntityId}");
                                    return null;
                                }

                                // update the entity state returning to the caller
                                entityState = updatedState;

                                if (!context.IsReplaying)
                                {
                                    log.LogWarning($"{Constants.UpdateEntityStateActivity} Activity completed for the entity id {requestData.EntityId}.");
                                }
                            }

                            // Update the entity response status
                            entityState.IsSuccess = true;

                            return entityState;
                        }

                        // None of the entities contain this user as the participant
                        // We're going to denied this user's accessing this entity
                        var owners = entities.Select(e => e.RowKey).ToArray();

                        return new EntityState
                        {
                            EntityId = requestData.EntityId,
                            IsSuccess = false,
                            RowKey = String.Join(",", owners),
                            CorrelationId = requestData.CorrelationId
                        };
                    }

                    // No entity mapping is found
                    return null;

                // Create Entity
                case ApiOperation.CreateEntityState:

                    // Create a new Online Meeting and get the Thread Id
                    requestData.ThreadId = await context.CallActivityAsync<string>(Constants.CreateOnlineMeetingActivity,
                        requestData);

                    // create a new entity state and save it in the durable storage
                    var newState = await context.CallActivityAsync<EntityState>(Constants.CreateEntityStateActivity,
                        requestData);

                    if (!context.IsReplaying)
                    {
                        log.LogWarning($"{Constants.CreateEntityStateActivity} Activity completed for the entity id {requestData.EntityId}.");
                    }

                    // update the entity state returning to the caller
                    return newState;

                default:
                    log.LogError("No Orchestration Function Activities found to execute.");
                    break;
            }

            return null;
        }
    }
}