// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Teams.EmbeddedChat.Middleware;
using Microsoft.Teams.EmbeddedChat.Services;

namespace Microsoft.Teams.EmbeddedChat;

class Program
{
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
                builder.Services.Configure<IConfiguration>(configuration =>
                {
                    builder.Services.AddOptions<AppSettings>().Bind(configuration);
                });
            })
            //</docsnippet_configure_defaults>
            .ConfigureAppConfiguration(configure =>
            {
                configure.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                configure.AddJsonFile($"appsettings.{Environments.Development}.json", optional: true, reloadOnChange: true);
            })
            //<docsnippet_dependency_injection>
            .ConfigureServices((host, services) =>
            {
                services.AddLogging();

                IConfiguration configuration = host.Configuration;
                AppSettings settings = configuration.GetSection("AppSettings").Get<AppSettings>();
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
