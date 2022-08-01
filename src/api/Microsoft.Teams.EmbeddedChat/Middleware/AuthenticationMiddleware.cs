using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Teams.EmbeddedChat.Authorization;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microsoft.Teams.EmbeddedChat.Middleware
{
    public class AuthenticationMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly JwtSecurityTokenHandler _tokenValidator;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
        private readonly ILogger<AuthenticationMiddleware> _logger;
        private readonly IList<string> _validAudiences;


        public AuthenticationMiddleware(AppSettings appSettings, ILogger<AuthenticationMiddleware> logger)
        {
            _logger = logger;
            var (isConfigInitialized, errMsg) = AppSettings.IsInitialized(appSettings);

            if (!isConfigInitialized)
            {
                throw new Exception(errMsg);
            }

            var authority = (appSettings.AuthenticationAuthority.IndexOf("login.microsoftonline.com") == -1) 
                ? $"{appSettings.AuthenticationAuthority}/{appSettings.TenantId}"
                : $"{appSettings.AuthenticationAuthority}/{appSettings.TenantId}/v2.0";
            var audience = (appSettings.AuthenticationAuthority.IndexOf("sts.windows.net") == -1) 
                ? appSettings.ClientId 
                : $"api://{appSettings.ClientId}";
            _validAudiences = new List<string>() { audience };
            _tokenValidator = new JwtSecurityTokenHandler();
            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidAudiences = _validAudiences
            };
            _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{authority}/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever());
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
                _logger.LogError("401 Unauthorized: Unable to get token from the headers");
                context.SetHttpResponseStatusCode(HttpStatusCode.Unauthorized);
                return;
            }

            if (!_tokenValidator.CanReadToken(token))
            {
                // Token is malformed
                _logger.LogError("401 Unauthorized: JWT Token is malformed");
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
            catch (SecurityTokenException e)
            {
                // Token is not valid (expired etc.)
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendJoin(',', _validAudiences.ToArray());
                _logger.LogError($"401 Unauthorized: Token is not valid. Error: {e.Message}. Accepted Audiences: {stringBuilder.ToString()}");
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
