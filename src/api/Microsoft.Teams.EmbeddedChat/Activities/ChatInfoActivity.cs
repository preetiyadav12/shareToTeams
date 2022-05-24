using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Teams.EmbeddedChat.Models;
using Microsoft.Teams.EmbeddedChat.Services;
using System.Threading.Tasks;

namespace Microsoft.Teams.EmbeddedChat.Activities
{
	public class ChatInfoActivity
	{
        private readonly IGraphService _graphClient;
        private readonly ILogger<ChatInfoActivity> _log;

        public ChatInfoActivity(IGraphService graphService, ILogger<ChatInfoActivity> log)
        {
            _log = log;
            _graphClient = graphService;
        }

        /// <summary>
        /// The Activity to create online meeting
        /// </summary>
        /// <param name="context"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(Constants.CreateOnlineMeeting)]
        public async Task<Models.ChatInfo> CreateOnlineMeetingAsync(
            [ActivityTrigger] IDurableActivityContext context)
        {
            _log.LogInformation($"Activity {Constants.CreateOnlineMeeting} has started.");

            // retrieves the entity state from the orchestration
            var orchestrationData = context.GetInput<OrchestrationRequest>();
            var requestData = orchestrationData.Request;

            // Initialize the graph client
            _graphClient.GetGraphServiceClient(orchestrationData.AccessToken, _log);

            // Create a new online meeting
            var onlineMeeting = await _graphClient.CreateOnlineMeetingAsync(requestData);

            _log.LogInformation($"Successfully created a new online meeting with meeting Id: {onlineMeeting.Id}");
            _log.LogInformation($"..and with thread Id: {onlineMeeting.ChatInfo.ThreadId}");

            // Return the custom Chat Info entity
            return new Models.ChatInfo
            {
                MeetingId = onlineMeeting.Id,
                ThreadId = onlineMeeting.ChatInfo.ThreadId,
                JoinUrl = onlineMeeting.JoinWebUrl
            };
        }
    }
}

