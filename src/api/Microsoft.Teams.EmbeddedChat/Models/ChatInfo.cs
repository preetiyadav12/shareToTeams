using System.Text.Json.Serialization;

namespace Microsoft.Teams.EmbeddedChat.Models
{
	public class ChatInfo
	{
        [JsonPropertyName("meetingId")]
        public string MeetingId { get; set; }

        [JsonPropertyName("threadId")]
        public string ThreadId { get; set; }

        [JsonPropertyName("joinUrl")]
        public string JoinUrl { get; set; }
    }
}

