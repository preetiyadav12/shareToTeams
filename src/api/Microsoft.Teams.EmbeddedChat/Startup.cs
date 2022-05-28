using Azure.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.Teams.EmbeddedChat;
using Microsoft.Teams.EmbeddedChat.Activities;
using Microsoft.Teams.EmbeddedChat.Services;
using System;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Microsoft.Teams.EmbeddedChat
{
    public class Startup : FunctionsStartup
    {
        IConfiguration Configuration { get; set; }

        /// <summary>
        /// Configure application
        /// </summary>
        /// <param name="builder"></param>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Get the azure function application directory. 
            var executionContextOptions = builder.Services.BuildServiceProvider()
                .GetService<IOptions<ExecutionContextOptions>>().Value;

            var currentDirectory = executionContextOptions.AppDirectory;

            // Get the original configuration provider from the Azure Function
            var configuration = builder.Services.BuildServiceProvider().GetService<IConfiguration>();

            // Create a new IConfigurationRoot and add our configuration along with Azure's original configuration 
            Configuration = new ConfigurationBuilder()
                .SetBasePath(currentDirectory)
                .AddConfiguration(configuration) // Add the original function configuration 
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Replace the Azure Function configuration with our new one
            builder.Services.AddSingleton(Configuration);

            // Add custom App Settings
            builder.Services.AddOptions<AppSettings>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    Configuration.GetSection("AppSettings").Bind(settings);
                });

            //builder.Services.AddOptions<AppSettings>()
            //    .Configure<IConfiguration>((settings, configuration) =>
            //    {
            //        configuration.GetSection("AppSettings").Bind(settings);
            //    });

            builder.Services.AddLogging();
            builder.Services.AddScoped<EntityMappingActivity>();
            builder.Services.AddScoped<ChatInfoActivity>();
            builder.Services.AddScoped<CreateAcsClientActivity>();
            builder.Services.AddScoped<ParticipantsActivity>();
            builder.Services.AddScoped<Processing>();

            //Add DI services
            builder.Services.AddSingleton<IGraphService>((s) =>
            {
                return new GraphService();
            });

            ConfigureServices(builder.Services);
        }

        /// <summary>
        /// Configure App Configuration
        /// </summary>
        /// <param name="builder"></param>
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var builtConfig = builder.ConfigurationBuilder.Build();
            var keyVaultEndpoint = builtConfig["AzureKeyVaultEndpoint"];

            if (!string.IsNullOrEmpty(keyVaultEndpoint))
            {
                // might need this depending on local dev env
                //var credential = new DefaultAzureCredential(
                //    new DefaultAzureCredentialOptions { ExcludeSharedTokenCacheCredential = true });

                // using Key Vault, either local dev or deployed
                builder.ConfigurationBuilder
                        .SetBasePath(Environment.CurrentDirectory)
                        .AddAzureKeyVault(new Uri(keyVaultEndpoint), new DefaultAzureCredential())
                        .AddJsonFile("local.settings.json", true)
                        .AddEnvironmentVariables()
                    .Build();
            }
            else
            {
                // local dev no Key Vault
                builder.ConfigurationBuilder
                   .SetBasePath(Environment.CurrentDirectory)
                   .AddJsonFile("local.settings.json", true)
                   .AddEnvironmentVariables()
                   .Build();
            }
        }


        /// <summary>
        /// Configure Authentication Middleware
        /// </summary>
        /// <param name="services"></param>
        private void ConfigureServices(IServiceCollection services)
        {
            // This is required to be instantiated before the OpenIdConnectOptions starts getting configured.
            // By default, the claims mapping will map claim names in the old format to accommodate older SAML applications.
            // 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' instead of 'roles'
            // This flag ensures that the ClaimsIdentity claims collection will be built from the claims in the token
            // JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            // Adds Microsoft Identity platform (AAD v2.0) support to protect this Api
            services.AddMicrosoftIdentityWebApiAuthentication(Configuration);

            //services.AddAuthentication(sharedOptions =>
            //{
            //    sharedOptions.DefaultScheme = Microsoft.Identity.Web.Constants.Bearer;
            //    sharedOptions.DefaultChallengeScheme = Microsoft.Identity.Web.Constants.Bearer;
            //})
            //    .AddMicrosoftIdentityWebApi(Configuration.GetSection("AzureAd"))
            //        .EnableTokenAcquisitionToCallDownstreamApi()
            //        .AddDownstreamWebApi("GraphApi", Configuration.GetSection("GraphApi"))
            //        .AddMicrosoftGraph(Configuration.GetSection("GraphApi"))
            //        .AddInMemoryTokenCaches();
        }

    }
}
