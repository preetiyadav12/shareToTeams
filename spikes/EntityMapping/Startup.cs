using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using TeamsEmbeddedChat.Models;

namespace Source
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

            services.AddControllers().AddNewtonsoftJson();

            var azureADSection = this.Configuration.GetSection("AzureAD");
            services.Configure<AzureADSettings>(azureADSection);
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

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
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
