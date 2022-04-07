// © Microsoft Corporation. All rights reserved.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Identity.Web;

namespace Chat
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddContosoServices(this IServiceCollection serviceCollection, IConfigurationSection chatConfigurationSection)
        {
            _ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));

            serviceCollection.Configure<ContosoSettings>(options =>
            {
                options.ChatGatewayUrl = ExtractApiChatGatewayUrl(chatConfigurationSection["ResourceConnectionString"]);
                options.ResourceConnectionString = chatConfigurationSection["ResourceConnectionString"];
            });
            serviceCollection.AddSingleton<IUserTokenManager, UserTokenManager>();

            // This is purely for the handshake server
            serviceCollection.AddSingleton<IChatAdminThreadStore, InMemoryChatAdminThreadStore>();
            return serviceCollection;
        }

        private static string ExtractApiChatGatewayUrl(string resourceConnectionString)
        {
            var uri = new Uri(resourceConnectionString.Replace("endpoint=", string.Empty, StringComparison.OrdinalIgnoreCase));
            return $"{uri.Scheme}://{uri.Host}";
        }

        /// <summary>
        /// Add cross-origin resource sharing services to the specified Microsoft.Extensions.DependencyInjection.IServiceCollection.
        /// </summary>
        /// <param name="services">A collection of service descriptors.</param>
        public static void AddCustomCors(this IServiceCollection services)
        {
            // Refer to this article if you require more information on CORS
            // https://docs.microsoft.com/aspnet/core/security/cors
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });
        }

        /// <summary>
        /// Add all setting models realted to the appsettings to the service container with Configure and bound to configuration.
        /// </summary>
        /// <param name="services">A collection of service descriptors.</param>
        /// <param name="configuration">Represents a set of key/value application configuration properties.</param>
        public static void AddAppSettings(
             this IServiceCollection services, IConfiguration configuration)
        {
            // Add CommunicationServicesSettingsModel to the service container with Configure and bound to configuration
            services.Configure<CommunicationServicesSettingsModel>(
                configuration.GetSection(CommunicationServicesSettingsModel.CommunicationServicesSettingsName));
            // Add GraphSettingsModel to the service container with Configure and bound to configuration
            services.Configure<GraphSettingsModel>(
                configuration.GetSection(GraphSettingsModel.GraphSettingsName));
        }

        /// <summary>
        /// Add core services (e.g. ACS service | Graph service) to the service container.
        /// </summary>
        /// <param name="services">A collection of service descriptors.</param>
        public static void AddCoreServices(this IServiceCollection services)
        {
            // Add ACS service
            services.AddSingleton<IACSService, ACSService>();
            // Add Graph service
            services.AddScoped<IGraphService, GraphService>();
        }

        /// <summary>
        /// Add all downstream apis to the service container.
        /// </summary>
        /// <param name="services">A collection of service descriptors.</param>
        /// <param name="configuration">Represents a set of key/value application configuration properties.</param>
        public static void AddDownstreamApis(this IServiceCollection services, IConfiguration configuration)
        {
            // Add the Microsoft Graph api as one of downstream apis
            // For more information, see https://docs.microsoft.com/azure/active-directory/develop/scenario-web-api-call-api-app-configuration?tabs=aspnetcore#option-1-call-microsoft-graph
            //services.AddMicrosoftIdentityWebAppAuthentication(configuration, AzureActiveDirectorySettingsModel.AzureActiveDirectorySettingsName)
            services.AddMicrosoftIdentityWebApiAuthentication(configuration, AzureActiveDirectorySettingsModel.AzureActiveDirectorySettingsName)
                    .EnableTokenAcquisitionToCallDownstreamApi()
                    .AddMicrosoftGraph(configuration.GetSection(GraphSettingsModel.GraphSettingsName))
                    .AddInMemoryTokenCaches();
        }

    }
}