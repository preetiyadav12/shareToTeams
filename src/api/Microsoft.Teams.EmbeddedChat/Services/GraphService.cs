using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Teams.EmbeddedChat.Models;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Teams.EmbeddedChat.Services
{
    public class GraphService : IGraphService
    {
        // The client credentials flow requires that you request the
        // /.default scope, and preconfigure your permissions on the
        // app registration in Azure. An administrator must grant consent
        // to those permissions beforehand.
        private string[] scopes = new[] { "https://graph.microsoft.com/.default" };
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string tenantId;
        private readonly IConfidentialClientApplication cca;

        private GraphServiceClient _graphServiceClient;
        private ILogger _logger;


        /// <summary>
        /// Constructor: create graph service client 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// /// <param name="tenantId"></param>
        public GraphService()
        {
            clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            if (String.IsNullOrEmpty(clientId))
                throw new ArgumentException("Missing AZURE_CLIENT_ID environment variable");
            if (String.IsNullOrEmpty(clientSecret))
                throw new ArgumentException("Missing AZURE_CLIENT_SECRET environment variable");
            if (String.IsNullOrEmpty(tenantId))
                throw new ArgumentException("Missing AZURE_TENANT_ID environment variable");


            cca = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithClientSecret(clientSecret)
                .WithTenantId(tenantId)
                .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
                .Build();
        }

        /// <summary>
        /// create GraphServiceClient
        /// </summary>
        /// <returns>GraphServiceClient</returns>
        public void GetGraphServiceClient(string userToken, ILogger logger)
        {
            this._logger = logger;
            try
            {
                // Multi-tenant apps can use "common",
                // single-tenant apps must use the tenant ID from the Azure portal
                // using Azure.Identity;
                var options = new TokenCredentialOptions
                {
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
                };

                // DelegateAuthenticationProvider is a simple auth provider implementation
                // that allows you to define an async function to retrieve a token
                // Alternatively, you can create a class that implements IAuthenticationProvider
                // for more complex scenarios
                var authProvider = new DelegateAuthenticationProvider(async (request) => {
                    // Use Microsoft.Identity.Client to retrieve token
                    var assertion = new UserAssertion(userToken);
                    var result = await cca.AcquireTokenOnBehalfOf(scopes, assertion).ExecuteAsync();

                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.AccessToken);
                });

                _graphServiceClient = new GraphServiceClient(authProvider);
            }
            catch (MsalUiRequiredException e)
            {
                string failureReason = "Failed to retrieve a Graph token for the user";
                if (e.Classification == UiRequiredExceptionClassification.ConsentRequired)
                {
                    failureReason = "The user or admin has not provided sufficient consent for the application";
                }
                _logger.LogError(failureReason);
                throw new ApplicationException(failureReason);
            }
        }


        /// <summary>
        /// Create a new Online Meeting
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        public async Task<OnlineMeeting> CreateOnlineMeetingAsync(ChatInfoRequest requestData)
        {
            // Assign the chat topic to the subject if it exists, otherwise, use the Entity Id as the subject
            var subject = !string.IsNullOrEmpty(requestData.Topic) ? requestData.Topic : requestData.EntityId;

            try
            {
                // Compile the participants list
                var attendees = new List<MeetingParticipantInfo>();

                foreach (var participant in requestData.Participants)
                {
                    var member = new MeetingParticipantInfo
                    {
                        Upn = participant.UserPrincipalName,
                        Role = OnlineMeetingRole.Attendee,
                    };
                    attendees.Add(member);
                }

                var participants = new MeetingParticipants
                {
                    Attendees = attendees
                };

                var onlineMeeting = new OnlineMeeting
                {
                    Subject = subject,
                    Participants = participants
                };

                // Create a new Online Meeting
                var meetingResponse = await _graphServiceClient.Users[requestData.Owner.Id].OnlineMeetings
                   .Request().AddAsync(onlineMeeting);

                 return meetingResponse;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    _logger.LogError(ex.InnerException.Message);
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<Models.Person[]> GetParticipantsListAsync(MeetingRequest meetingRequest)
        {
            var participants = new List<Models.Person>();

            try
            {
                // Get the list of participants in the chat
                var members = await _graphServiceClient.Users[meetingRequest.MeetingOwnerId]
                    .Chats[meetingRequest.ChatInfo.ThreadId]
                    .Members
                    .Request()
                    .GetAsync();

                // Iterate thru the list of members excluding the chat owner
                foreach (AadUserConversationMember member in members)
                {
                    if (member.UserId == meetingRequest.MeetingOwnerId)
                        continue;
                    participants.Add(new Models.Person
                    {
                        Id = member.UserId,
                        UserPrincipalName = member.Email,
                        DisplayName = member.DisplayName,
                    });
                }

                return participants.ToArray();
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                    _logger.LogError(e.InnerException.Message);
                _logger.LogError(e.Message);
                throw;
            }
        }
    }
}
