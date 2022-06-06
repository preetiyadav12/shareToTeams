using Dynamitey;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Teams.EmbeddedChat.Authorization;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microsoft.Teams.EmbeddedChat.Middleware
{
    public class AuthenticationMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly JwtSecurityTokenHandler _tokenValidator;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
        private readonly string _functionKey;

        public AuthenticationMiddleware(IConfiguration configuration)
        {
            var authority = $"{configuration["AuthenticationAuthority"]}/{configuration["AZURE_TENANT_ID"]}/v2.0";
            var audience = configuration["AZURE_CLIENT_ID"];
            _tokenValidator = new JwtSecurityTokenHandler();
            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidAudience = audience
            };
            _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{authority}/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever());

            _functionKey = configuration["FUNCTION_KEY"];
        }

        public async Task Invoke(
            FunctionContext context,
            FunctionExecutionDelegate next)
        {
            var targetMethod = context.GetTargetFunctionMethod();

            var methodAttributes = AuthorizationMiddleware.GetCustomAttributesOnClassAndMethod<AuthorizeAttribute>(targetMethod);

            if (methodAttributes.Count == 0) // No authorization decorations
            {
                // Set principal + token in Features collection
                // They can be accessed from here later in the call chain
                context.Features.Set(new JwtPrincipalFeature(new System.Security.Claims.ClaimsPrincipal(), "", true));
                await next(context);
                return;
            }

            if (!TryGetTokenFromHeaders(context, out var token))
            {
                // Unable to get token from headers
                context.SetHttpResponseStatusCode(HttpStatusCode.Unauthorized);
                return;
            }

            // If the function key was provided instead of Authorization, then let it go.
            if (token == this._functionKey)
            {
                // Set principal + token in Features collection
                // They can be accessed from here later in the call chain
                context.Features.Set(new JwtPrincipalFeature(new System.Security.Claims.ClaimsPrincipal(), "", true));
                await next(context);
                return;
            }

            if (!_tokenValidator.CanReadToken(token))
            {
                // Token is malformed
                context.SetHttpResponseStatusCode(HttpStatusCode.Unauthorized);
                return;
            }

            // Get OpenID Connect metadata
            var validationParameters = _tokenValidationParameters.Clone();
            var openIdConfig = await _configurationManager.GetConfigurationAsync(default);
            validationParameters.ValidIssuer = openIdConfig.Issuer;
            validationParameters.IssuerSigningKeys = openIdConfig.SigningKeys;

            try
            {
                // Validate token
                var principal = _tokenValidator.ValidateToken(
                        token, validationParameters, out _);

                // Set principal + token in Features collection
                // They can be accessed from here later in the call chain
                context.Features.Set(new JwtPrincipalFeature(principal, token, principal.Identity.IsAuthenticated));

                await next(context);
            }
            catch (SecurityTokenException)
            {
                // Token is not valid (expired etc.)
                context.SetHttpResponseStatusCode(HttpStatusCode.Unauthorized);
                return;
            }
        }

        private bool TryGetTokenFromHeaders(FunctionContext context, out string token)
        {
            token = null;
            // HTTP headers are in the binding context as a JSON object
            // The first checks ensure that we have the JSON string
            if (!context.BindingContext.BindingData.TryGetValue("Headers", out var headersObj))
            {
                return false;
            }

            if (headersObj is not string headersStr)
            {
                return false;
            }

            // Deserialize headers from JSON
            var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(headersStr);
            var normalizedKeyHeaders = headers.ToDictionary(h => h.Key.ToLowerInvariant(), h => h.Value);
            if (!normalizedKeyHeaders.TryGetValue("authorization", out var authHeaderValue))
            {
                // No Authorization header present
                // Will now check if X-Functions-Key was present
                if (normalizedKeyHeaders.TryGetValue("x-functions-key", out var funcHeaderValue))
                {
                    token = this._functionKey;
                    return true;
                }

                return false;
            }

            if (!authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                // Scheme is not Bearer
                return false;
            }

            token = authHeaderValue.Substring("Bearer ".Length).Trim();
            return true;
        }
    }
}
