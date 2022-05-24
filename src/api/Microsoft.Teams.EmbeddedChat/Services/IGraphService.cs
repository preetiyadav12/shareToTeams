using Microsoft.Graph;
using Microsoft.Teams.EmbeddedChat.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Microsoft.Teams.EmbeddedChat.Services
{
    public interface IGraphService
    {
        void GetGraphServiceClient(string userToken, ILogger logger);
        Task<OnlineMeeting> CreateOnlineMeetingAsync(ChatInfoRequest requestData);
        Task<Models.Person[]> GetParticipantsListAsync(MeetingRequest meetingRequest);
    }
}
