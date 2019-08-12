#region Copyright
//=======================================================================================
// Microsoft 
//
// This sample is supplemental to the technical guidance published on my personal
// blog at https://github.com/paolosalvatori. 
// 
// Author: Paolo Salvatori
//=======================================================================================
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// LICENSED UNDER THE APACHE LICENSE, VERSION 2.0 (THE "LICENSE"); YOU MAY NOT USE THESE 
// FILES EXCEPT IN COMPLIANCE WITH THE LICENSE. YOU MAY OBTAIN A COPY OF THE LICENSE AT 
// http://www.apache.org/licenses/LICENSE-2.0
// UNLESS REQUIRED BY APPLICABLE LAW OR AGREED TO IN WRITING, SOFTWARE DISTRIBUTED UNDER THE 
// LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY 
// KIND, EITHER EXPRESS OR IMPLIED. SEE THE LICENSE FOR THE SPECIFIC LANGUAGE GOVERNING 
// PERMISSIONS AND LIMITATIONS UNDER THE LICENSE.
//=======================================================================================
#endregion

#region Using Directives
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using TodoWeb.Models;
using TodoWeb.Services;
#endregion

namespace TodoWeb
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
            services.AddOptions();
            services.Configure<TodoApiServiceOptions>(Configuration.GetSection("TodoApiService"));
            services.Configure<Models.DataProtectionOptions>(Configuration.GetSection("DataProtectionService"));
            services.AddDataProtection().PersistKeysToAzureBlobStorage(GetKeyBlob());
            services.AddApplicationInsightsTelemetry(Configuration);
            services.AddMvc();
            services.AddDbContext<TodoContext>(opt => opt.UseInMemoryDatabase("TodoList"));
            services.AddSingleton<ITodoApiService, TodoApiService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, 
                              IHostingEnvironment env,
                              ILoggerFactory loggerFactory,
                              IServiceProvider serviceProvider)
        {
            // Add Application Insights logger
            loggerFactory.AddApplicationInsights(serviceProvider, Microsoft.Extensions.Logging.LogLevel.Information);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
        }

        /// <summary>
        /// Get the blob reference where to store the data protection key
        /// </summary>
        /// <returns>blob used for the data protection key</returns>
        private CloudBlockBlob GetKeyBlob()
        {
            // Validation
            var connectionString = Configuration["DataProtection:BlobStorage:ConnectionString"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("No connection string is defined in the configuration of the Data Protection service in the appsettings.json.");
            }

            var containerName = Configuration["DataProtection:BlobStorage:ContainerName"];

            if (string.IsNullOrWhiteSpace(containerName))
            {
                throw new ArgumentNullException("No container name is defined in the configuration of the Data Protection service in the appsettings.json.");
            }

            //Parse the connection string and return a reference to the storage account.
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            //Create the blob client object.
            var blobClient = storageAccount.CreateCloudBlobClient();

            //Get a reference to a container to use for the sample code, and create it if it does not exist.
            var container = blobClient.GetContainerReference(containerName);
            container.CreateIfNotExistsAsync();

            //Get a reference to a blob within the container.
            return container.GetBlockBlobReference("todowebkey");
        }
    }
}
