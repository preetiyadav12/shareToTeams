// <copyright file="EntityState.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.EmbeddedChat.Models
{
	public class EntityStateRecord : IBaseTableEntity
	{
        public string EntityId { get; set; }
        public string Owner { get; set; }
        public string ChatInfo { get; set; }
        public string Participants { get; set; }
        public string AcsInfo { get; set; }
    }
}

