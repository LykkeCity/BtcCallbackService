using System;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureRepositories;
using AzureStorage.Tables;
using Common.Log;
using Core.Settings;
using Lykke.Bitcoin.CallbackService.Middleware;
using Lykke.Bitcoin.CallbackService.Modules;
using Lykke.Logs;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.Swagger.Model;

namespace Lykke.Bitcoin.CallbackService
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }
        public ILog Log { get; private set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(env.ContentRootPath)
               .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var appSettings = Configuration.LoadSettings<CallbackServiceSettings>();

            var builder = new ContainerBuilder();

            Log = CreateLogWithSlack(appSettings);

            builder.RegisterModule(new JobModule(appSettings.Nested(x => x.CallbackService.Db), Log));

            services.AddMvc();

            services.AddSwaggerGen(options =>
            {
                options.SingleApiVersion(new Info
                {
                    Version = "v1",
                    Title = "Bitcoin Callback Service"
                });
                options.DescribeAllEnumsAsStrings();

                //Determine base path for the application.
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;

                //Set the comments path for the swagger json and ui.
                var xmlPath = Path.Combine(basePath, "Lykke.Bitcoin.CallbackService.xml");
                options.IncludeXmlComments(xmlPath);
            });
            
            builder.Populate(services);

            return new AutofacServiceProvider(builder.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.Use(next => context =>
            {
                context.Request.EnableRewind();
                return next(context);
            });
            app.UseMiddleware<GlobalLogRequestsMiddleware>();
            app.UseMiddleware<GlobalErrorHandlerMiddleware>();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUi("swagger/ui/index");
        }

        private static ILog CreateLogWithSlack(IReloadingManager<CallbackServiceSettings> settings)
        {
            var consoleLogger = new LogToConsole();
            var aggregateLogger = new AggregateLogger();

            aggregateLogger.AddLog(consoleLogger);

            var dbLogConnectionStringManager = settings.Nested(x => x.CallbackService.Db.LogsConnString);
            var dbLogConnectionString = dbLogConnectionStringManager.CurrentValue;

            // Creating azure storage logger, which logs own messages to concole log
            if (!string.IsNullOrEmpty(dbLogConnectionString) && !(dbLogConnectionString.StartsWith("${") && dbLogConnectionString.EndsWith("}")))
            {
                var persistenceManager = new LykkeLogToAzureStoragePersistenceManager(
                    AzureTableStorage<LogEntity>.Create(dbLogConnectionStringManager, "PrebroadcastHandlerLog", consoleLogger),
                    consoleLogger);
                
                var azureStorageLogger = new LykkeLogToAzureStorage(
                    persistenceManager,
                    null,
                    consoleLogger);

                azureStorageLogger.Start();

                aggregateLogger.AddLog(azureStorageLogger);
            }

            return aggregateLogger;
        }
    }
}