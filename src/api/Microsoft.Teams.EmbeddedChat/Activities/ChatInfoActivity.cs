using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Teams.EmbeddedChat.Models;
using Microsoft.Teams.EmbeddedChat.Services;
using System.Threading.Tasks;

namespace Microsoft.Teams.EmbeddedChat.Activities
{
	public class ChatInfoActivity
	{
        private readonly AppSettings _appConfiguration;

        public ChatInfoActivity(IOptions<AppSettings> configuration)
        {
            _appConfiguration = configuration.Value;
        }

        /// <summary>
        /// The Activity to create online meeting
        /// </summary>
        /// <param name="context"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(Constants.CreateOnlineMeeting)]
        public async Task<Models.ChatInfo> CreateOnlineMeetingAsync(
            [ActivityTrigger] IDurableActivityContext context, ILogger log)
        {
            log.LogInformation($"Activity {Constants.CreateOnlineMeeting} has started.");

            // retrieves the entity state from the orchestration
            var requestData = context.GetInput<ChatInfoRequest>();


            // Create a Graph Service client
            var graphClient = new GraphService(requestData.accessToken, log);

            // Initialize the graph client
            graphClient.GetGraphServiceClient();

            // Create a new online meeting
            var onlineMeeting = await graphClient.CreateOnlineMeetingAsync(requestData);


            log.LogInformation($"Successfully created a new online meeting with meeting Id: {onlineMeeting.Id}");
            log.LogInformation($"..and with thread Id: {onlineMeeting.ChatInfo.ThreadId}");

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

