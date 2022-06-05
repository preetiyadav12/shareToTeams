namespace Microsoft.Teams.EmbeddedChat.Models
{
	public class BaseModel
	{
        public string Id { get; set; }
        public bool? IsSuccess { get; set; }
        public string AccessToken { get; set; }
        public string CorrelationId { get; set; }
    }
}

