using Microsoft.Extensions.Logging;
using Microsoft.Teams.EmbeddedChat.Models;
using Microsoft.Teams.EmbeddedChat.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Teams.EmbeddedChat.Activities
{
    public static class ParticipantsActivity
    {
        /// <summary>
        /// The Activity to update participants list
        /// </summary>
        /// <param name="context"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static async Task<Models.Person[]> GetParticipantsAsync(
            MeetingRequest meetingRequest, IGraphService graphService, ILogger logger)
        {
            // Initialize the graph client
            graphService.GetGraphServiceClient(meetingRequest.AccessToken, logger);


            // Create a new online meeting
            var participants = await graphService.GetParticipantsListAsync(meetingRequest);


            logger.LogInformation($"Successfully retrieved the latest list of participants for the meeting with Chat Thread Id: {meetingRequest.ChatInfo.ThreadId}");
 
            // Return the custom Chat Info entity
            return participants;
        }


        public static bool ParticipantsEqual(Person[] source, Person[] target)
        {
            foreach (var person in source)
            {
                if (target.Count(p => p.Id == person.Id) == 0)
                    return false;
            }

            return target.Count() == source.Count();
        }

    }
}
