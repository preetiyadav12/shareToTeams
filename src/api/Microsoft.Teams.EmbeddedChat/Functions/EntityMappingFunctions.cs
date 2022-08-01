// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Teams.EmbeddedChat.Authorization;
using System.Threading.Tasks;
using Microsoft.Teams.EmbeddedChat.Models;
using System.Linq;

namespace Microsoft.Teams.EmbeddedChat.Functions;


// If an Authorize attribute is placed at class-level,
// requests to any function within the class
// must pass the authorization checks
//[Authorize(
//    Scopes = new[] { Scopes.FunctionsAccess },
//    UserRoles = new[] { UserRoles.User, UserRoles.Admin },
//    AppRoles = new[] { AppRoles.AccessAllFunctions })]
public class EntityMappingFunctions
{
    private readonly ILogger<EntityMappingFunctions> _logger;
    private readonly IProcessing _process;
    private readonly AppSettings _appSettings;

    public EntityMappingFunctions(IProcessing processing, ILogger<EntityMappingFunctions> logger, AppSettings appSettings)
    {
        _logger = logger;
        _process = processing;
        _appSettings = appSettings;
    }

    /// <summary>
    /// HTTP-triggered function that starts the <see cref="EntityMappingOrchestration"/> orchestration.
    /// </summary>
    /// <param name="req">The HTTP request that was used to trigger this function.</param>
    /// <param name="executionContext">The Azure Functions execution context, which is available to all function types.</param>
    /// <returns>Returns an HTTP response with more information about the started orchestration instance.</returns>
    [Authorize(
        Scopes = new[] { Scopes.FunctionsAccess })]
    [Function(nameof(GetEntityMapping))]
    public async Task<HttpResponseData> GetEntityMapping(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = HttpRoutes.GetEntityStateRoute)] HttpRequestData req,
        FunctionContext executionContext)
    {
        try
        {
            // check if the App Settings were initialized correctly
            var (isInitialized, errMsg) = AppSettings.IsInitialized(_appSettings);
            if (!isInitialized) 
            {
                return HttpResponses.CreateFailedResponse(req,
                    $"The AppSettings have not been initialized! {errMsg}");
            }

            var requestData = await req.ReadFromJsonAsync<ChatInfoRequest>();

            if (requestData == null)
            {
                var msg = $"Must provide the input body of {nameof(ChatInfoRequest)} type";
                _logger.LogError(msg);
                return HttpResponses.CreateFailedResponse(req, msg);
            }

            _logger.LogInformation(
                $"The Function {nameof(GetEntityMapping)} was triggered by {requestData?.Owner?.DisplayName}");

            var response = await _process.GetEntity(requestData, req);

            return response;
        }
        catch (System.Exception ex)
        {
            var errMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return HttpResponses.CreateFailedResponse(req, errMsg);
        }
    }


    /// <summary>
    /// HTTP-triggered function that starts the <see cref="EntityMappingOrchestration"/> orchestration.
    /// </summary>
    /// <param name="req">The HTTP request that was used to trigger this function.</param>
    /// <param name="executionContext">The Azure Functions execution context, which is available to all function types.</param>
    /// <returns>Returns an HTTP response with more information about the started orchestration instance.</returns>
    [Authorize(
        Scopes = new[] { Scopes.FunctionsAccess })]
    [Function(nameof(CreateEntityMapping))]
    public async Task<HttpResponseData> CreateEntityMapping(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = HttpRoutes.CreateEntityStateRoute)] HttpRequestData req,
        FunctionContext executionContext)
    {
        try
        {
            // check if the App Settings were initialized correctly
            var (isInitialized, errMsg) = AppSettings.IsInitialized(_appSettings);
            if (!isInitialized)
            {
                return HttpResponses.CreateFailedResponse(req,
                    $"The AppSettings have not been initialized! {errMsg}");
            }

            var requestData = await req.ReadFromJsonAsync<ChatInfoRequest>();

            if (requestData == null)
            {
                var msg = $"Must provide the input body of {nameof(ChatInfoRequest)} type";
                _logger.LogError(msg);
                return HttpResponses.CreateFailedResponse(req, msg);
            }

            _logger.LogInformation(
                $"The Function {nameof(CreateEntityMapping)} was triggered by {requestData?.Owner?.DisplayName}");

            var response = await _process.CreateEntity(requestData, req);
            return response;
        }
        catch (System.Exception ex)
        {
            var errMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return HttpResponses.CreateFailedResponse(req, errMsg);
        }
    }


    /// <summary>
    /// HTTP-triggered function that returns the health status of this function.
    /// </summary>
    /// <param name="req">The HTTP request that was used to trigger this function.</param>
    /// <param name="executionContext">The Azure Functions execution context, which is available to all function types.</param>
    /// <returns>Returns an HTTP response with more information about the started orchestration instance.</returns>
    [Function(nameof(GetHealthStatus))]
    public HttpResponseData GetHealthStatus(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = HttpRoutes.GetFunctionHealthRoute)] HttpRequestData req,
        FunctionContext executionContext)
    {
        try
        {
            _logger.LogInformation(
                $"The Function {nameof(GetHealthStatus)} was triggered.");

            // check if the App Settings were initialized correctly
            var (isInitialized, errMsg) = AppSettings.IsInitialized(_appSettings);
            if (!isInitialized)
            {
                return HttpResponses.CreateFailedResponse(req,
                    $"The AppSettings have not been initialized! {errMsg}");
            }

            return HttpResponses.CreateOkTextResponse(req, "The Function Health Status is OK");
        }
        catch (System.Exception ex)
        {
            var errMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return HttpResponses.CreateFailedResponse(req, errMsg);
        }
    }


    /// <summary>
    /// HTTP-triggered function that checks the Auth JWToken from the request data
    /// </summary>
    /// <param name="req">The HTTP request that was used to trigger this function.</param>
    /// <param name="executionContext">The Azure Functions execution context, which is available to all function types.</param>
    /// <returns>Returns an HTTP response with more information about the started orchestration instance.</returns>
    [Authorize(
        Scopes = new[] { Scopes.FunctionsAccess })]
    [Function(nameof(CheckJWTokenStatus))]
    public HttpResponseData CheckJWTokenStatus(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = HttpRoutes.GetJWTokenRoute)] HttpRequestData req,
        FunctionContext executionContext)
    {
        try
        {
            _logger.LogInformation(
                $"The Function {nameof(CheckJWTokenStatus)} was triggered.");

            // check if the App Settings were initialized correctly
            var (isInitialized, errMsg) = AppSettings.IsInitialized(_appSettings);
            if (!isInitialized)
            {
                return HttpResponses.CreateFailedResponse(req,
                    $"The AppSettings have not been initialized! {errMsg}");
            }

            if (!req.Headers.Contains("Authorization"))
            {
                var msg = "Request header is missing Authorization header.";
                _logger.LogError(msg);
                return HttpResponses.CreateFailedResponse(req, msg);
            }

            // Update the user's token of the request data
            var accessToken = Processing.ExtractJWToken(req.Headers.GetValues("Authorization").First()); // extract access token from the header

            return HttpResponses.CreateOkTextResponse(req, accessToken);
        }
        catch (System.Exception ex)
        {
            var errMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return HttpResponses.CreateFailedResponse(req, errMsg);
        }
    }


}
