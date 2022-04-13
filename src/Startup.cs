// <copyright file="Startup.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Teams.EmbeddedChat.Models;

namespace Microsoft.Teams.EmbeddedChat
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // resolve config sections
            var cosmosDBSection = this.Configuration.GetSection("AzureCosmos");
            services.Configure<CosmosDBSettings>(cosmosDBSection);
            var acsSection = this.Configuration.GetSection("Acs");
            services.Configure<AcsSettings>(acsSection);
            var azureADSection = this.Configuration.GetSection("AzureAD");
            services.Configure<AzureADSettings>(azureADSection);

            // initialize JSON.NET for controllers
            services.AddControllers().AddNewtonsoftJson();

            // Enable CORS
            services.AddCors();

            // initialize bearer authentication for APIs
            services.AddAuthentication(o =>
            {
                o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                var azureADSettings = azureADSection.Get<AzureADSettings>();
                o.Authority = $"https://login.microsoftonline.com/{azureADSettings.TenantId}/v2.0/";
                o.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    // Both App ID URI and client id are valid audiences in the access token
                    ValidAudiences = new List<string>
                    {
                        azureADSettings.AppId,
                        $"api://{azureADSettings.AppId}",
                        $"api://{azureADSettings.HostDomain}/{azureADSettings.AppId}",
                    },
                };
            });

            // Initialize Cosmos client
            var cosmosDBSettings = cosmosDBSection.Get<CosmosDBSettings>();
            var cosmosClientOptions = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                },
            };
            var cosmosClient = new CosmosClient(cosmosDBSettings.ConnectionString, cosmosClientOptions);
            services.AddSingleton<CosmosClient>(cosmosClient);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // global cors policy
            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials()); // allow credentials

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseCors("DefaultPolicy")
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Home}/{action=Index}/{id?}");
                    endpoints.MapFallbackToFile("index.html");
                });
        }
    }
}
