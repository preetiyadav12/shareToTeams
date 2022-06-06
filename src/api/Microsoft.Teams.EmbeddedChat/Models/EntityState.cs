// <copyright file="EntityState.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Microsoft.Teams.EmbeddedChat.Models
{
    /// <summary>
    /// Class for EntityState.
    /// </summary>
    public class EntityState : BaseModel
    {
        [JsonPropertyName("entityId")]
        public string EntityId { get; set; }

        [JsonPropertyName("owner")]
        public Person Owner { get; set; }

        [JsonPropertyName("chatInfo")]
        public ChatInfo ChatInfo { get; set; }

        [JsonPropertyName("participants")]
        public Person[] Participants { get; set; }

        [JsonPropertyName("acsInfo")]
        public ACSInfo AcsInfo { get; set; }

        public EntityState Copy(EntityState source) { return source.Copy(this); }
    }
}
