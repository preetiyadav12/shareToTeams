// <copyright file="OrchestrationRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.EmbeddedChat.Models
{
    /// <summary>
    /// Class for OrchestrationRequest.
    /// </summary>
    public class OrchestrationRequest
    {
        public ApiOperation Operation { get; set; }
        public EntityState Request { get; set; }
    }
}
