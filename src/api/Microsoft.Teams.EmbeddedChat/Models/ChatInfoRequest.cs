using System;
namespace Microsoft.Teams.EmbeddedChat.Models
{
	public class ChatInfoRequest
	{
        public string Id { get; set; }
        public string EntityId { get; set; }
        public string Username { get; set; }
        public string Topic { get; set; }
        public string ThreadId { get; set; }
        public Participant[] Participants { get; set; }
        public string CorrelationId { get; set; }
    }
}

