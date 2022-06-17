using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Teams.EmbeddedChat.Activities;
using Microsoft.Teams.EmbeddedChat.Models;
using Microsoft.Teams.EmbeddedChat.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Teams.EmbeddedChat
{
    public class Processing : IProcessing
    {
        private readonly IGraphService _graphService;
        private readonly ILogger<Processing> _log;
        private readonly AppSettings _appConfiguration;

        public Processing(IGraphService graphService, ILoggerFactory loggerFactory, AppSettings configuration)
        {
            _graphService = graphService;
            _log = loggerFactory.CreateLogger<Processing>();
            _appConfiguration = configuration;
        }


        /// <summary>
        /// Get Entity State
        /// </summary>
        /// <param name="requestData"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<HttpResponseData> GetEntity(
            ChatInfoRequest requestData,
            HttpRequestData request)
        {
            _log.LogInformation($"Started function {nameof(GetEntity)} for the Entity ID: '{requestData.EntityId}'.");

            try
            {
                // Check if the mapping exists for this entity Id by invoking a separate Activity Function.
                var entities = EntityMappingActivity.GetEntityState(requestData, _log, _appConfiguration);

                if (entities.Any())
                {
                    // for each entity get the participant list and update state
                    // 1. foreach entity begin
                    // 2.  call getParticipantsActivity passing entity's chatInfo
                    // 3. getParticipantsActivity returns the updated list of participants
                    // 4. assign the new participants list to the entity's participants property
                    // 5. call UpdateEntityStateActivity passing the current entity (with updated participants list)
                    // 6. end foreach loop
                    foreach (var entity in entities)
                    {
                        var meetingRequest = new MeetingRequest
                        {
                            ChatInfo = entity.ChatInfo,
                            MeetingOwnerId = requestData.Owner.Id,
                            AccessToken = ExtractJWToken(request.Headers.GetValues("Authorization").First()) // extract access token from the header
                        };

                        //Refresh teams user token
                        await EntityMappingActivity.UpdateEntityTokenStateAsync(entity, _appConfiguration, _log, meetingRequest.AccessToken, _graphService);


                        // Get the updated list of all chat participants
                        var participants = await ParticipantsActivity.GetParticipantsAsync(meetingRequest, _graphService, _log);

                        if (participants.Any(p => p.Id == requestData.Owner.Id) || entity.Owner.Id == requestData.Owner.Id)
                        {
                            if (!ParticipantsActivity.ParticipantsEqual(entity.Participants, participants))
                            {
                                // the new list of participants is different from the source
                                // then, we'll update the entity state
                                entity.Participants = participants;

                                // If the ACS Token has expired, we'll refresh it and then update the state
                                if (DateTime.Parse(entity.AcsInfo.TokenExpiresOn).CompareTo(DateTime.UtcNow) < 0)
                                {
                                    _log.LogWarning("ACS Token has expired. Refreshing the token and returning the updated state...");
                                    // invalidate the current expired token
                                    entity.AcsInfo.AcsToken = null;
                                }

                                var (updateStatus, updatedState) =
                                    await EntityMappingActivity.UpdateEntityStateAsync(entity, _appConfiguration, _log);
                                if (updateStatus == false)
                                {
                                    var msg = $"Failed to update the entity with Id: {requestData.EntityId}";
                                    _log.LogError(msg);
                                    return HttpResponses.CreateFailedResponse(request, msg);
                                }

                                // Mark the updated state as successful
                                updatedState.IsSuccess = true;

                                return await HttpResponses.CreateOkResponseAsync(request, updatedState);
                            }

                            return await HttpResponses.CreateOkResponseAsync(request, entity);
                        }
                    }

                    // None of the entities contain this user as the participant
                    // We're going to denied this user's accessing this entity
                    var ownerNames = entities.Select(e => e.Owner.UserPrincipalName).ToArray();
                    var owners = new Person
                    {
                        UserPrincipalName = String.Join(",", ownerNames)
                    };

                    return await HttpResponses.CreateOkResponseAsync(request, new EntityState
                    {
                        EntityId = requestData.EntityId,
                        IsSuccess = false,
                        Owner = owners,
                        CorrelationId = requestData.CorrelationId
                    });
                }

                return HttpResponses.CreateNotFoundResponse(
                    request, $"No Entity mapping found for the requested Entity Id: {requestData.EntityId}");
            }
            catch (Exception ex)
            {
                var errMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return HttpResponses.CreateFailedResponse(request, errMsg);
            }
        }



        /// <summary>
        /// Create a new entity state
        /// </summary>
        /// <param name="requestData"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<HttpResponseData> CreateEntity(
            ChatInfoRequest requestData,
            HttpRequestData request)
        {
            _log.LogInformation($"Started function {nameof(CreateEntity)} for the Entity ID: '{requestData.EntityId}'.");

            try
            {
                if (!request.Headers.Contains("Authorization"))
                {
                    var msg = "Request header is missing Authorization header.";
                    _log.LogError(msg);
                    return HttpResponses.CreateFailedResponse(request, msg);
                }

                // Update the user's token of the request data
                requestData.AccessToken = ExtractJWToken(request.Headers.GetValues("Authorization").First()); // extract access token from the header

                // 1. Create a new Online Meeting and get the Thread Id
                var chatInfo = await ChatInfoActivity.CreateOnlineMeetingAsync(requestData, _graphService, _log);

                // 2. create a new ACS Client and fill the info to the entity state
                var acsInfo = await AcsClientActivity.CreateACSClientAsync(requestData, _appConfiguration, _graphService, _log);

                // 3. Create a new Entity State
                var newState = new EntityState
                {
                    EntityId = requestData.EntityId,
                    Owner = requestData.Owner,
                    ChatInfo = chatInfo,
                    AcsInfo = acsInfo,
                    Participants = requestData.Participants,
                    CorrelationId = requestData.CorrelationId,
                    IsSuccess = true
                };

                // Save the entity state to the durable storage
                var entityRecord = await EntityMappingActivity.CreateEntityStateAsync(newState, _log, _appConfiguration);
                if (entityRecord == null)
                {
                    var msg = $"{nameof(EntityMappingActivity.CreateEntityStateAsync)} Activity failed to create a new Entity State for the entity id {requestData.EntityId}.";
                    _log.LogError(msg);
                    return HttpResponses.CreateFailedResponse(request, msg);
                }

                return await HttpResponses.CreateOkResponseAsync(request, newState);
            }
            catch (Exception ex)
            {
                var errMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return HttpResponses.CreateFailedResponse(request, errMsg);
            }
        }



        /// <summary>
        /// Extracts JWToken from the Bearer Authentication token
        /// </summary>
        /// <param name="authorization"></param>
        /// <returns></returns>
        private static string ExtractJWToken(string authorization)
        {
            return authorization.StartsWith("Bearer") ? authorization.Split(" ").Last() :
                authorization;
        }
    }
}
