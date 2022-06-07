
using System.Text.Json.Serialization;

namespace Microsoft.Teams.EmbeddedChat.Models
{
	public class Person
	{
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("userPrincipalName")]
        public string UserPrincipalName { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("photo")]
        public string Photo { get; set; }
    }
}

