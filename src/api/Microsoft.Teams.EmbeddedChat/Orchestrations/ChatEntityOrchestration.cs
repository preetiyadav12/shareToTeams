using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Teams.EmbeddedChat.Models;

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

            if (!context.IsReplaying)
            {
                log.LogWarning($"Started Entity State Orchestration with Instance Id: {orchestrationRequest.Request.Id} for the entity id {orchestrationRequest.Request.EntityId}.");
            }

            switch (orchestrationRequest.Operation)
            {
                case ApiOperation.GetEntityState:
                    // Check if the mapping exists for this entity Id by invoking a separate Activity Function.
                    var entityState = await context.CallActivityAsync<EntityState>(Constants.GetEntityStateActivity, orchestrationRequest.Request);

                    if (!context.IsReplaying)
                    {
                        log.LogWarning($"{Constants.GetEntityStateActivity} Activity completed for the entity id {orchestrationRequest.Request.EntityId}.");
                    }

                    // If the entity state doesn't exist and if the ACS Token has not expired
                    if (entityState == null)
                    {
                        log.LogWarning($"Entity State for the entity id {orchestrationRequest.Request.EntityId} has not been found. Creating a new one...");

                        // create a new entity state and save it in the durable storage
                        entityState = await context.CallActivityAsync<EntityState>(Constants.CreateEntityStateActivity, orchestrationRequest.Request);

                        if (!context.IsReplaying)
                        {
                            log.LogWarning($"{Constants.CreateEntityStateActivity} Activity completed for the entity id {orchestrationRequest.Request.EntityId}.");
                        }
                    }
                    // If the ACS Token has expired, we'll refresh it and then update the state
                    else if (DateTime.Parse(entityState.TokenExpiresOn).CompareTo(context.CurrentUtcDateTime) < 0)
                    {
                        log.LogWarning("ACS Token has expired. Refreshing the token and returning the updated state...");

                        // update the entity state with the refreshed token
                        var (updateStatus, updatedState) = await context.CallActivityAsync<(bool, EntityState)>(Constants.UpdateEntityStateActivity, orchestrationRequest.Request);
                        if (updateStatus == false)
                        {
                            log.LogError($"Failed to update the entity with Id: {orchestrationRequest.Request.EntityId}");
                            return null;
                        }

                        // update the entity state returning to the caller
                        entityState = updatedState;

                        if (!context.IsReplaying)
                        {
                            log.LogWarning($"{Constants.UpdateEntityStateActivity} Activity completed for the entity id {orchestrationRequest.Request.EntityId}.");
                        }
                    }

                    if (entityState == null)
                    {
                        log.LogError("Failed to get/create Entity State. Something went wrong.");
                    }

                    return entityState;

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