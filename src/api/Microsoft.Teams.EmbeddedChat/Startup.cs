using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Teams.EmbeddedChat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Teams.EmbeddedChat.Activities;
using System;
using Azure.Identity;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Microsoft.Teams.EmbeddedChat
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOptions<AppSettings>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("AppSettings").Bind(settings);
                });

            builder.Services.AddLogging();
            builder.Services.AddScoped<EntityMappingActivity>();
            builder.Services.AddScoped<ChatInfoActivity>();
            builder.Services.AddScoped<CreateAcsClientActivity>();
            builder.Services.AddScoped<ParticipantsActivity>();
            builder.Services.AddScoped<Processing>();
        }

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
    }
}
