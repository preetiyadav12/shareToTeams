// <copyright file="EntityState.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.EmbeddedChat.Models
{
    /// <summary>
    /// Class for EntityState.
    /// </summary>
    public class EntityState : BaseModel
    {
        public string EntityId { get; set; }
        public Person Owner { get; set; }
        public ChatInfo ChatInfo { get; set; }
        public Person[] Participants { get; set; }
        public ACSInfo AcsInfo { get; set; }

        public EntityState Copy(EntityState source) { return source.Copy(this); }
    }
}
