using System.Text.Json.Serialization;

namespace Microsoft.Teams.EmbeddedChat.Models
{
	public class BaseModel
	{
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("isSuccess")]
        public bool? IsSuccess { get; set; }

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }

        [JsonPropertyName("correlationId")]
        public string CorrelationId { get; set; }
    }
}

