using System;
namespace Microsoft.Teams.EmbeddedChat.Models
{
	public class ChatInfoRequest
	{
        public string Id { get; set; }
        public string EntityId { get; set; }
        public Person Owner { get; set; }
        public string accessToken { get; set; }
        public string Topic { get; set; }
        public string ThreadId { get; set; }
        public Person[] Participants { get; set; }
        public string CorrelationId { get; set; }
    }
}

