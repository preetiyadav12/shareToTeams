using Microsoft.Extensions.Logging;
using Microsoft.Teams.EmbeddedChat.Models;
using Microsoft.Teams.EmbeddedChat.Services;
using System.Threading.Tasks;

namespace Microsoft.Teams.EmbeddedChat.Activities;

public static class ChatInfoActivity
{
    /// <summary>
    /// The Activity to create online meeting
    /// </summary>
    /// <param name="context"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static async Task<Models.ChatInfo> CreateOnlineMeetingAsync(
        ChatInfoRequest requestData, IGraphService graphService, ILogger logger)
    {
        logger.LogInformation($"Activity {nameof(CreateOnlineMeetingAsync)} has started.");

        // Initialize the graph client
        graphService.GetGraphServiceClient(requestData.AccessToken, logger);

        // Create a new online meeting
        var onlineMeeting = await graphService.CreateOnlineMeetingAsync(requestData);

        logger.LogInformation($"Successfully created a new online meeting with meeting Id: {onlineMeeting.Id}");
        logger.LogInformation($"..and with thread Id: {onlineMeeting.ChatInfo.ThreadId}");

        // Return the custom Chat Info entity
        return new Models.ChatInfo
        {
            MeetingId = onlineMeeting.Id,
            ThreadId = onlineMeeting.ChatInfo.ThreadId,
            JoinUrl = onlineMeeting.JoinWebUrl
        };
    }
}

