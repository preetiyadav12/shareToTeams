﻿using System.Security.Claims;

namespace Microsoft.Teams.EmbeddedChat.Middleware
{
    /// <summary>
    /// Holds the authenticated user principal
    /// for the request along with the
    /// access token they used.
    /// </summary>
    public class JwtPrincipalFeature
    {
        public JwtPrincipalFeature(ClaimsPrincipal principal, string accessToken, bool isAuthenticated)
        {
            Principal = principal;
            AccessToken = accessToken;
            IsAuthenticated = isAuthenticated;
        }

        public ClaimsPrincipal Principal { get; }

        /// <summary>
        /// The access token that was used for this
        /// request. Can be used to acquire further
        /// access tokens with the on-behalf-of flow.
        /// </summary>
        public string AccessToken { get; }

        public bool IsAuthenticated { get; }
    }
}
