using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Teams.EmbeddedChat.Models;
using Microsoft.Teams.EmbeddedChat.Services;
using System.Threading.Tasks;

namespace Microsoft.Teams.EmbeddedChat.Activities
{
    public class ParticipantsActivity
    {
        private readonly IGraphService _graphClient;
        private readonly ILogger<ParticipantsActivity> _log;

        public ParticipantsActivity(IGraphService graphService, ILogger<ParticipantsActivity> log)
        {
            _log = log;
            _graphClient = graphService;
        }

        /// <summary>
        /// The Activity to update participants list
        /// </summary>
        /// <param name="context"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(Constants.GetParticipantsActivity)]
        public async Task<Models.Person[]> GetParticipantsAsync(
            [ActivityTrigger] IDurableActivityContext context)
        {

            // retrieves the entity state from the orchestration
            var meetingRequest = context.GetInput<MeetingRequest>();

            // Initialize the graph client
            _graphClient.GetGraphServiceClient(meetingRequest.AccessToken, _log);


            // Create a new online meeting
            var participants = await _graphClient.GetParticipantsListAsync(meetingRequest);


            _log.LogInformation($"Successfully retrieved the latest list of participants for the meeting with meeting Id: {meetingRequest.MeetingId}");
 
            // Return the custom Chat Info entity
            return participants;
        }

    }
}
