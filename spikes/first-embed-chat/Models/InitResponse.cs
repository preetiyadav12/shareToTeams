// <copyright file="InitResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.EmbeddedChat.Models
{
    /// <summary>
    /// Class for InitResponse.
    /// </summary>
    public class InitResponse
    {
        public Mapping Mapping { get; set; }
        public string AcsToken { get; set; }
    }
}
