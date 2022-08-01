// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
using System;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Teams.EmbeddedChat.Middleware;
using Microsoft.Teams.EmbeddedChat.Services;

namespace Microsoft.Teams.EmbeddedChat;

class Program
{
    public const string SentinelKey = "Settings:Sentinel";

    static async Task Main(string[] args)
    {
        // #if DEBUG
        //          Debugger.Launch();
        // #endif
        //<docsnippet_startup>
        var host = new HostBuilder()
            //<docsnippet_configure_defaults>
            .ConfigureFunctionsWorkerDefaults(builder =>
            {
                builder.UseMiddleware<AuthenticationMiddleware>();
                builder.UseMiddleware<AuthorizationMiddleware>();

                //builder.Services.Configure<IConfiguration>(configuration =>
                //{
                //    builder.Services.AddOptions<AppSettings>().Bind(configuration);
                //});
            })
            //</docsnippet_configure_defaults>
            .ConfigureAppConfiguration(config =>
            {
                // Add Environment Variables
                config.AddEnvironmentVariables();

                config.AddUserSecrets(System.Reflection.Assembly.GetExecutingAssembly(), true);
                var appConfigConnectionString = config.Build()["APP_CONFIG_CONNECTION_STRING"] ?? Environment.GetEnvironmentVariable("APP_CONFIG_CONNECTION_STRING");
                var hostingEnvironment = config.Build()["APPHOSTING_ENVIRONMENT"] ?? Environment.GetEnvironmentVariable("APPHOSTING_ENVIRONMENT");

                if (!string.IsNullOrEmpty(appConfigConnectionString))
                {
                    // Use the connection string if it is available.
                    config.AddAzureAppConfiguration(options =>
                    {
                        options.Connect(appConfigConnectionString)
                        // Load configuration values with no label
                        .Select(KeyFilter.Any, LabelFilter.Null)
                        // Override with any configuration values specific to current hosting env
                        .Select(KeyFilter.Any, hostingEnvironment)
                        // Configure refresh
                        .ConfigureRefresh(refresh =>
                        {
                            // Register for refresh operation.
                            refresh.Register(SentinelKey, refreshAll: true);
                        });
                    });
                }
                else
                {
                    // Use Azure Active Directory authentication.
                    // The identity of this app should be assigned 'App Configuration Data Reader' or 'App Configuration Data Owner' role in App Configuration.
                    // For more information, please visit https://aka.ms/vs/azure-app-configuration/concept-enable-rbac

                    // Get connection string to the Azure App Configuration
                    string azAppConfigEndPoint = config.Build()["APP_CONFIG_ENDPOINT"] ?? Environment.GetEnvironmentVariable("APP_CONFIG_ENDPOINT");
                    if(string.IsNullOrEmpty(azAppConfigEndPoint))
                    {
                        throw new ApplicationException("Missing APP_CONFIG_ENDPOINT in Function App Configuration settings");
                    }

                    config.AddAzureAppConfiguration(options =>
                    {
                        // Use a managed identity
                        options.Connect(new Uri(azAppConfigEndPoint), new ManagedIdentityCredential())
                        // Load configuration values with no label
                        .Select(KeyFilter.Any, LabelFilter.Null)
                        // Override with any configuration values specific to current hosting env
                        .Select(KeyFilter.Any, hostingEnvironment)
                        // Configure refresh
                        .ConfigureRefresh(refresh =>
                        {
                            refresh.SetCacheExpiration(TimeSpan.FromDays(1));
                            // Register for refresh operation.
                            refresh.Register(SentinelKey, refreshAll: true);
                        });
                    });
                }
            })
            //<docsnippet_dependency_injection>
            .ConfigureServices((host, services) =>
            {
                services.AddLogging();

                IConfiguration configuration = host.Configuration;
                AppSettings settings = new AppSettings
                {
                    StorageConnectionString = configuration["AppSettings:StorageConnectionString"],
                    //AcsEndpoint = configuration["AppSettings:AcsEndpoint"],
                    AcsConnectionString = configuration["AppSettings:AcsConnectionString"],
                    AzureTableName = configuration["AppSettings:AzureTableName"],
                    AuthenticationAuthority = configuration["AppSettings:AuthenticationAuthority"],
                    ClientId = configuration["AppSettings:ClientId"],
                    ClientSecret = configuration["AppSettings:ClientSecret"],
                    TenantId = configuration["AppSettings:TenantId"],
                };

                var (isConfigInitialized, errMsg) = AppSettings.IsInitialized(settings);

                if (!isConfigInitialized)
                {
                    throw new Exception(errMsg);
                }

                services.AddSingleton(settings);

                services.AddSingleton<IGraphService, GraphService>();
                services.AddSingleton<IProcessing, Processing>();
            })
            //</docsnippet_dependency_injection>
            .Build();

        //</docsnippet_startup>
        //<docsnippet_host_run>
        await host.RunAsync();
        //</docsnippet_host_run>

    }
}
