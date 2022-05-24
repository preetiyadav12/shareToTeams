namespace Microsoft.Teams.EmbeddedChat.Models
{
	public class MeetingRequest
	{
        public string MeetingOwnerId { get; set; }
        public string AccessToken { get; set; }
        public ChatInfo ChatInfo { get; set; }
    }
}

