using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Teams.EmbeddedChat.Models;
using Microsoft.Teams.EmbeddedChat.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Teams.EmbeddedChat.Activities
{
    public class UpdateParticipantsActivity
    {
        private readonly AppSettings _appConfiguration;

        public UpdateParticipantsActivity(IOptions<AppSettings> configuration)
        {
            _appConfiguration = configuration.Value;
        }

        /// <summary>
        /// The Activity to update participants list
        /// </summary>
        /// <param name="context"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(Constants.GetParticipantsActivity)]

        public async Task<(bool participantUpdateStatus, Models.Participant[] particpants)> UpdateParticipantsAsync(
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
            var mtgId = "MSo5MmIyOTUzYy02ZDFiLTQxZWMtOGJkZi1kODQ4MWFkMzM1MWUqMCoqMTk6bWVldGluZ19ZV0UwWldNeFkyUXRZVEppT1MwME5EWTRMV0ZoTWpFdFkyVmpaak00TnpoaU1HUXhAdGhyZWFkLnYy";

            //var mtgId = requestData.Id;
            //Call Graph to get the participant data
            var onlineMeeting = await graphClient.Me.OnlineMeetings[mtgId]
                    .Request()
                    .GetAsync();

            // retrieves the participants list from the graph call

            List<object> attendees = JsonConvert.DeserializeObject<List<object>>(onlineMeeting.Participants.Attendees.ToString());
            String[] result;

            for (int i = 0; i < attendees.Count; i++)
            {
                result[i] = attendees[i]["identity"];

            }


            log.LogInformation($"Activity {Constants.GetParticipantsActivity} has started.");

            return (true, result); //for debug reasons
        }
    }
}
