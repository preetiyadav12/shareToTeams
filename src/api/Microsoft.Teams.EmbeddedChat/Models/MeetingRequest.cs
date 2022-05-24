using System;
namespace Microsoft.Teams.EmbeddedChat.Models
{
	public class MeetingRequest
	{
        public string MeetingId { get; set; }
        public string MeetingOwnerId { get; set; }
        public string AccessToken { get; set; }
    }
}

