// <copyright file="AcsSettings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.EmbeddedChat.Models
{
    /// <summary>
    /// Class AcsSettings.
    /// </summary>
    public class AcsSettings
    {
        /// <summary>
        /// Gets or sets the ACS Connection string.
        /// </summary>
        /// <value>The endpoint URL.</value>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the name of the ACS endpoint.
        /// </summary>
        /// <value>The name of the database.</value>
        public string Endpoint { get; set; }
    }
}