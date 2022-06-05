using System;
namespace Microsoft.Teams.EmbeddedChat.Models
{
	public class ChatInfoRequest : BaseModel
	{
        public string EntityId { get; set; }
        public Person Owner { get; set; }
        public string Topic { get; set; }
        public string ThreadId { get; set; }
        public Person[] Participants { get; set; }
    }
}

