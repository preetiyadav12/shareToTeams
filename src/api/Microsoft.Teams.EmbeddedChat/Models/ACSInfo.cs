using System;
using System.Text.Json.Serialization;

namespace Microsoft.Teams.EmbeddedChat.Models
{
	public class ACSInfo
	{
        [JsonPropertyName("acsUserId")]
        public string AcsUserId { get; set; }

        [JsonPropertyName("acsToken")]
        public string AcsToken { get; set; }

        [JsonPropertyName("tokenExpiresOn")]
        public string TokenExpiresOn { get; set; }

        [JsonPropertyName("commIdentityToken")]
        public string CommIdentityToken { get; set; }
    }
}

