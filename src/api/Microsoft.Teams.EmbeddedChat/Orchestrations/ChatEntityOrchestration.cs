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
                        // at least one entity mapping found
                        // now we'll check if this user was the owner of one of the entities in the list
                        var entityState = entities.FirstOrDefault(e => e.Owner == requestData.Username);
                        if (entityState != null) // This user is the owner of the entity! Return it!
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
                            return entityState;
                        }

                        // No entity found for the particular user, let's check the participants list
                    }
                    // If the entity state doesn't exist and if the ACS Token has not expired
                    else
                    {
                        log.LogWarning($"Entity State for the entity id {requestData.EntityId} has not been found. Creating a new one...");

                        // create a new entity state and save it in the durable storage
                        var entityState = await context.CallActivityAsync<EntityState>(Constants.CreateEntityStateActivity, requestData);

                        if (!context.IsReplaying)
                        {
                            log.LogWarning($"{Constants.CreateEntityStateActivity} Activity completed for the entity id {requestData.EntityId}.");
                        }
                    }
                    break;

                case ApiOperation.UpdateEntityState:
                    // update the entity state
                    var (updateActivityStatus, updatedEntityState) = await context.CallActivityAsync<(bool, EntityState)>(Constants.UpdateEntityStateActivity, orchestrationRequest.Request);
                    if (updateActivityStatus == false)
                    {
                        log.LogError($"Failed to update the entity with Id: {orchestrationRequest.Request.EntityId}");
                        return null;
                    }

                    if (!context.IsReplaying)
                    {
                        log.LogWarning($"{Constants.UpdateEntityStateActivity} Activity completed for the entity id {orchestrationRequest.Request.EntityId}.");
                    }

                    // update the entity state returning to the caller
                    return updatedEntityState;

                default:
                    log.LogError("No Orchestration Function Activities found to execute.");
                    break;
            }

            return null;
        }
    }
}