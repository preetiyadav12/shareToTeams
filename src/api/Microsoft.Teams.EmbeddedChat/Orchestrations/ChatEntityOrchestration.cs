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
                        // for each entity get the participant list and update state
                        // 1. foreach entity begin
                        // 2.  call getParticipantsActivity passing entity's chatInfo
                        // 3. getParticipantsActivity returns the updated list of participants
                        // 4. assign the new participants list to the entity's participants property
                        // 5. call UpdateEntityStateActivity passing the current entity (with updated participants list)
                        // 6. end foreach loop
                        foreach (var entity in entities)    
                        {
                            entity.Participants = await context.CallActivityAsync<(Participant[])>(Constants.GetParticipantsActivity, requestData);

                            var (updateStatus, updatedState) = await context.CallActivityAsync<(bool, EntityState)>(Constants.UpdateEntityStateActivity, requestData);
                            if (updateStatus == false)
                            {
                                log.LogError($"Failed to update the entity with Id: {requestData.EntityId}");
                                return null;
                            }
                        }
                       

                        var updatedEntities = entities;

                        // at least one entity mapping found
                        // now we'll check if this user was the owner of one of the entities in the list
                        var entityState = updatedEntities.FirstOrDefault(
                            e => e.Participants.Any(p => p.Id == requestData.Owner.Id));

                        if (entityState != null) // This user is one of the participants in this entity chat!
                        {
                            // If the ACS Token has expired, we'll refresh it and then update the state
                            if (DateTime.Parse(entityState.AcsInfo.TokenExpiresOn).CompareTo(context.CurrentUtcDateTime) < 0)
                            {
                                log.LogWarning("ACS Token has expired. Refreshing the token and returning the updated state...");
                                // invalidate the current expired token
                                entityState.AcsInfo.AcsToken = null;

                                // update the entity state while refreshing the token
                                var (updateStatus, updatedState) = await context.CallActivityAsync<(bool, EntityState)>(Constants.UpdateEntityStateActivity, entityState);
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

                    // 1. Create a new entity state object and initialize it 
                    // 2. Create a new Online Meeting and get the Thread Id
                    var chatInfo = await context.CallActivityAsync<ChatInfo>(Constants.CreateOnlineMeeting, requestData);
                    // 3. create a new ACS Client and fill the info to the entity state
                    var acsInfo = await context.CallActivityAsync<ACSInfo>(Constants.CreateACSClientActivity, null);
                    // Create a new Entity State
                    var newState = new EntityState
                    {
                        PartitionKey = requestData.EntityId,
                        RowKey = requestData.Owner.Id,
                        EntityId = requestData.EntityId,
                        Owner = requestData.Owner,
                        ChatInfo = chatInfo,
                        AcsInfo = acsInfo,
                        CorrelationId = requestData.CorrelationId,
                        IsSuccess = false
                    };

                    if (!context.IsReplaying)
                    {
                        log.LogWarning($"{Constants.CreateACSClientActivity} Activity completed for the entity id {requestData.EntityId}.");
                    }

                    // Save the entity state to the durable storage
                    if (!await context.CallActivityAsync<bool>(Constants.CreateEntityStateActivity, newState))
                    {
                        log.LogWarning($"{Constants.CreateEntityStateActivity} Activity failed to create a new Entity State for the entity id {requestData.EntityId}.");
                        return null;
                    }

                    if (!context.IsReplaying)
                    {
                        log.LogWarning($"{Constants.CreateEntityStateActivity} Activity completed for the entity id {requestData.EntityId}.");
                    }

                    return newState;

                default:
                    log.LogError("No Orchestration Function Activities found to execute.");
                    break;
            }

            return null;
        }
    }
}