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
            var meetingRequest = context.GetInput<MeetingRequest>();


            // Create a Graph Service client
            var graphClient = new GraphService(meetingRequest.Token, log);

            // Initialize the graph client
            graphClient.GetGraphServiceClient();


            // Create a new online meeting
            var participants = await graphClient.GetParticipantsListAsync(meetingRequest);


            log.LogInformation($"Successfully retrieved the latest list of participants for the meeting with meeting Id: {meetingRequest.MeetingId}");
 
            // Return the custom Chat Info entity
            return participants;
        }

    }
}
