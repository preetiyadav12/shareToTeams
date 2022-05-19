using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using System.Threading.Tasks;
using Microsoft.Teams.EmbeddedChat.Models;
using Microsoft.Teams.EmbeddedChat.Services;
using System;
using System.Collections.Generic;


namespace Microsoft.Teams.EmbeddedChat.Activities
{
    public class CreateOnlineMeetingActivity
    {
        
        private readonly AppSettings _appConfiguration;

        public CreateOnlineMeetingActivity(IOptions<AppSettings> configuration, IDurableActivityContext context, ILogger log)
        {
            _appConfiguration = configuration.Value;


        }

        /// <summary>
        /// The Activity to create online meeting
        /// </summary>
        /// <param name="context"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(Constants.CreateOnlineMeetingActivity)]
        public async Task<String> CreateOnlineMeetingAsync(
            [ActivityTrigger] IDurableActivityContext context, ILogger log)
        {

            // retrieves the entity state from the orchestration
            var requestData = context.GetInput<EntityState>();

            var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            var tenant = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");

            log.LogInformation($"Client Id: {clientId}");
            log.LogInformation($"Tenant Id: {tenant}");

            if (String.IsNullOrEmpty(clientId))
            {
                log.LogError("Missing AZURE_CLIENT_ID environment variable");
                throw new ArgumentException("Missing AZURE_CLIENT_ID environment variable");
            }
            if (String.IsNullOrEmpty(clientSecret))
            {
                log.LogError("Missing AZURE_CLIENT_SECRET environment variable");
                throw new ArgumentException("Missing AZURE_CLIENT_SECRET environment variable");
            }
            if (String.IsNullOrEmpty(tenant))
            {
                log.LogError("Missing AZURE_TENANT_ID environment variable");
                throw new ArgumentException("Missing AZURE_TENANT_ID environment variable");
            }

            var graphService = new GraphService(clientId, clientSecret, tenant);

            GraphServiceClient graphClient = graphService.GetGraphServiceClient();

            var subject = requestData.EntityId;
            var externalId = Guid.NewGuid();

            List<MeetingParticipantInfo> AttendeesList = new List<MeetingParticipantInfo>();
            
            for (int i = 0; i < requestData.Participants.Length; i++)
            {
                MeetingParticipantInfo member = new MeetingParticipantInfo
                {
                    Identity = new IdentitySet
                    {
                        User = new Graph.Identity
                        {
                            Id = requestData.Participants[i].Id

                        }

                    }

                };
                AttendeesList.Add(member);

            }

            var participants = new MeetingParticipants
            {
                Attendees = AttendeesList
            };

            var meetingResponse = await graphClient.Me.OnlineMeetings
               .CreateOrGet(externalId.ToString(), null, null, participants, null, subject)
               .Request()
               .PostAsync();

            var chatThreadId = meetingResponse.ChatInfo.ThreadId;
            log.LogInformation($"Activity {Constants.CreateOnlineMeetingActivity} has started.");
            return (chatThreadId);
        }
    }

}
