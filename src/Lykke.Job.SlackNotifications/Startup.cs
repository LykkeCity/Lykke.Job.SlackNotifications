﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using AzureStorage.Tables;
using Lykke.Logs;
using Lykke.SlackNotification.AzureQueue;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.SettingsReader;
using Lykke.JobTriggers.Extenstions;
using Lykke.JobTriggers.Triggers;
using Lykke.Job.SlackNotifications.Core;
using Lykke.Job.SlackNotifications.Core.Services;
using Lykke.Job.SlackNotifications.Services;

namespace Lykke.Job.SlackNotifications
{
    public class Startup
    {
        private TriggerHost _triggerHost;

        public IContainer ApplicationContainer { get; private set; }
        public IConfigurationRoot Configuration { get; }
        public ILog Log { get; private set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                });

            services.AddSwaggerGen(options =>
            {
                options.DefaultLykkeConfiguration("v1", "SlackNotifications Job API");
            });

            var builder = new ContainerBuilder();
            var settingsManager = Configuration.LoadSettings<AppSettings>();

            Log = CreateLogWithSlack(services, settingsManager);
            builder.RegisterInstance(Log).As<ILog>().SingleInstance();

            var settings = settingsManager.CurrentValue;
            builder.RegisterInstance(settings.SlackNotificationsJobSettings).SingleInstance();
            builder.RegisterType<SlackNotifcationsConsumer>();
            builder.RegisterType<NotificationFilter>().As<INotificationFilter>().SingleInstance();
            builder
                .RegisterType<SrvSlackNotifications>()
                .WithParameter(TypedParameter.From(settings.SlackNotificationsJobSettings.Slack))
                .As<ISlackNotificationSender>();

            builder.AddTriggers(pool =>
            {
                pool.AddDefaultConnection(settingsManager.CurrentValue.SlackNotificationsJobSettings.SharedStorageConnString);
            });

            builder.Populate(services);

            ApplicationContainer = builder.Build();

            var serviceProvider = new AutofacServiceProvider(ApplicationContainer);

            _triggerHost = new TriggerHost(serviceProvider);
            _triggerHost.ProvideAssembly(typeof(SlackNotifcationsConsumer).Assembly);

            return serviceProvider;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseLykkeMiddleware(nameof(SlackNotifications), ex => new { Message = "Technical problem" });

            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUi();
            app.UseStaticFiles();

            appLifetime.ApplicationStarted.Register(StartApplication);
            appLifetime.ApplicationStopping.Register(StopApplication);
            appLifetime.ApplicationStopped.Register(CleanUp);
        }

        private void StartApplication()
        {
            try
            {
                Task.Run(() => _triggerHost?.Start().Wait());
            }
            catch (Exception ex)
            {
                Log.WriteFatalErrorAsync(nameof(Startup), nameof(StartApplication), "", ex);
            }
        }

        private void StopApplication()
        {
            try
            {
                _triggerHost?.Cancel();
            }
            catch (Exception ex)
            {
                Log.WriteFatalErrorAsync(nameof(Startup), nameof(StopApplication), "", ex);
            }
        }

        private void CleanUp()
        {
            try
            {
                ApplicationContainer.Dispose();
            }
            catch (Exception ex)
            {
                Log.WriteFatalErrorAsync(nameof(Startup), nameof(CleanUp), "", ex);
            }
        }

        private static ILog CreateLogWithSlack(IServiceCollection services, IReloadingManager<AppSettings> settings)
        {
            var consoleLogger = new LogToConsole();
            var aggregateLogger = new AggregateLogger();

            aggregateLogger.AddLog(consoleLogger);

            // Creating slack notification service, which logs own azure queue processing messages to aggregate log
            var slackService = services.UseSlackNotificationsSenderViaAzureQueue(new AzureQueueIntegration.AzureQueueSettings
            {
                ConnectionString = settings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString,
                QueueName = settings.CurrentValue.SlackNotifications.AzureQueue.QueueName
            }, aggregateLogger);

            var dbLogConnectionStringManager = settings.Nested(x => x.SlackNotificationsJobSettings.LogsConnectionString);
            var dbLogConnectionString = dbLogConnectionStringManager.CurrentValue;

            // Creating azure storage logger, which logs own messages to concole log
            if (!string.IsNullOrEmpty(dbLogConnectionString) && !(dbLogConnectionString.StartsWith("${") && dbLogConnectionString.EndsWith("}")))
            {
                const string appName = "Lykke.Job.SlackNotifications";

                var persistenceManager = new LykkeLogToAzureStoragePersistenceManager(
                    appName,
                    AzureTableStorage<LogEntity>.Create(dbLogConnectionStringManager, "SlackNotificationsJobLogs", consoleLogger),
                    consoleLogger);

                var slackNotificationsManager = new LykkeLogToAzureSlackNotificationsManager(appName, slackService, consoleLogger);

                var azureStorageLogger = new LykkeLogToAzureStorage(
                    appName,
                    persistenceManager,
                    slackNotificationsManager,
                    consoleLogger);

                azureStorageLogger.Start();

                aggregateLogger.AddLog(azureStorageLogger);
            }

            return aggregateLogger;
        }
    }
}
