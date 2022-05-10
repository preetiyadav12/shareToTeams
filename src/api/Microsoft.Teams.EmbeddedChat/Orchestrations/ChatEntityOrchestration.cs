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
            // retrieves the entity Id from the ChatEntityOrchestrator_HttpStart function
            var entityStateRequest = context.GetInput<EntityState>();
            entityStateRequest.RowKey = context.InstanceId;

            if (!context.IsReplaying)
            {
                log.LogWarning($"Started Entity State Orchestration with Instance Id: {entityStateRequest.RowKey} for the entity id {entityStateRequest.EntityId}.");
            }

            // Check if the mapping exists for this entity Id by invoking a separate Activity Function.
            var entityState = await context.CallActivityAsync<EntityState>(Constants.GetEntityStateActivity, entityStateRequest);

            if (!context.IsReplaying)
            {
                log.LogWarning($"{Constants.GetEntityStateActivity} Activity completed for the entity id {entityStateRequest.EntityId}.");
            }

            if (entityState == null)
            {
                log.LogWarning($"Entity State for the entity id {entityStateRequest.EntityId} has not been found. Creating a new one...");

                // create a new entity state and save it in the durable storage
                entityState = await context.CallActivityAsync<EntityState>(Constants.CreateEntityStateActivity, entityStateRequest);
            }

            if (!context.IsReplaying)
            {
                log.LogWarning($"{Constants.CreateEntityStateActivity} Activity completed for the entity id {entityStateRequest.EntityId}.");
            }

            if (entityState == null)
            {
                log.LogError("Failed to get/create Entity State. Something went wrong.");
            }

            return entityState;
        }
    }
}