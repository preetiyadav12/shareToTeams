// <copyright file="EntityState.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.EmbeddedChat.Models
{
    /// <summary>
    /// Class for InitResponse.
    /// </summary>
    public class EntityState : IBaseTableEntity
    {
        public string EntityId { get; set; }
        public string ThreadId { get; set; }
        public string AcsUserId { get; set; }
        public string AcsToken { get; set; }

        public EntityState Copy(EntityState source) { return source.Copy(this); }
    }
}