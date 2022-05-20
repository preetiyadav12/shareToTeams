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
    public class ParticipantsActivity
    {
        private readonly AppSettings _appConfiguration;

        public ParticipantsActivity(IOptions<AppSettings> configuration)
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
        public async Task<Models.Person[]> GetParticipantsAsync(
            [ActivityTrigger] IDurableActivityContext context, ILogger log)
        {

            // retrieves the entity state from the orchestration
            var requestData = context.GetInput<ChatInfoRequest>();


            // Create a Graph Service client
            var graphClient = new GraphService(requestData.accessToken, log);

            // Initialize the graph client
            graphClient.GetGraphServiceClient();



            // var mtgId = "MSo5MmIyOTUzYy02ZDFiLTQxZWMtOGJkZi1kODQ4MWFkMzM1MWUqMCoqMTk6bWVldGluZ19ZV0UwWldNeFkyUXRZVEppT1MwME5EWTRMV0ZoTWpFdFkyVmpaak00TnpoaU1HUXhAdGhyZWFkLnYy";

            //Call Graph to get the participant data
            //var onlineMeeting = await graphClient.

       

            log.LogInformation($"Activity {Constants.GetParticipantsActivity} has started.");

            return (true, result); //for debug reasons
        }

        [FunctionName(Constants.UpdateParticipantsActivity)]
        public async Task<(bool updateStatus, EntityState updatedState)> UpdateParticipantsActivity([ActivityTrigger] IDurableActivityContext context, ILogger log)
        {

        }
    }
}
