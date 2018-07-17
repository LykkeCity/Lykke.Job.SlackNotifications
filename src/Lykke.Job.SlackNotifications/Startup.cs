using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Queue;
using Common.Log;
using AzureStorage.Tables;
using Common;
using Lykke.Logs;
using Lykke.SlackNotification.AzureQueue;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.SettingsReader;
using Lykke.JobTriggers.Extenstions;
using Lykke.JobTriggers.Triggers;
using Lykke.Job.SlackNotifications.Core;
using Lykke.Job.SlackNotifications.Core.Domain;
using Lykke.Job.SlackNotifications.Core.Services;
using Lykke.Job.SlackNotifications.PeriodicalHandlers;
using Lykke.Job.SlackNotifications.Repositories;
using Lykke.Job.SlackNotifications.Services;

namespace Lykke.Job.SlackNotifications
{
    public class Startup
    {
        private TriggerHost _triggerHost;
        private Task _triggerHostTask;

        public IContainer ApplicationContainer { get; private set; }
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

            if (!string.IsNullOrEmpty(settings.SlackNotificationsJobSettings.ForwardMonitorMessagesQueueConnString))
            {
                builder.RegisterInstance<IMsgForwarder>(new MsgForwarder(AzureQueueExt.Create(
                    settingsManager.ConnectionString(x => x.SlackNotificationsJobSettings.ForwardMonitorMessagesQueueConnString),
                    "slack-notifications-monitor"))).SingleInstance();
            }
            else
            {
                builder.RegisterInstance<IMsgForwarder>(new MsgForwarderStub()).SingleInstance();
            }

            builder.RegisterType<NotificationFilter>().As<INotificationFilter>().SingleInstance();
            builder
                .RegisterType<SrvSlackNotifications>()
                .WithParameter(TypedParameter.From(settings.SlackIntegration))
                .As<ISlackNotificationSender>();
            builder
                .Register(c => MessagesRepository.Create(settingsManager.Nested(s => s.SlackNotificationsJobSettings.FullMessagesConnString)))
                .As<IMessagesRepository>();

            builder.AddTriggers(pool =>
            {
                pool.AddDefaultConnection(settingsManager.ConnectionString(x => x.SlackNotificationsJobSettings.SharedStorageConnString));
            });

            builder.RegisterType<UnmuteHandler>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();

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
            app.UseSwaggerUI(x =>
            {
                x.RoutePrefix = "swagger/ui";
                x.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            });
            app.UseStaticFiles();

            appLifetime.ApplicationStarted.Register(() => StartApplication().Wait());
            appLifetime.ApplicationStopping.Register(() => StopApplication().Wait());
            appLifetime.ApplicationStopped.Register(() => CleanUp().Wait());
        }

        private async Task StartApplication()
        {
            try
            {
                _triggerHostTask = _triggerHost.Start();

                await Log.WriteMonitorAsync("", $"Env: {Program.EnvInfo}", "Started");
            }
            catch (Exception ex)
            {
                await Log.WriteFatalErrorAsync(nameof(Startup), nameof(StartApplication), ex);
            }
        }

        private async Task StopApplication()
        {
            try
            {
                _triggerHost?.Cancel();

                if (_triggerHostTask != null)
                {
                    await _triggerHostTask;
                }
            }
            catch (Exception ex)
            {
                if (Log != null)
                {
                    await Log.WriteFatalErrorAsync(nameof(Startup), nameof(StopApplication), ex);
                }
            }
        }

        private async Task CleanUp()
        {
            try
            {
                if (Log != null)
                {
                    await Log.WriteMonitorAsync("", $"Env: {Program.EnvInfo}", "Terminating");
                }

                ApplicationContainer.Dispose();
            }
            catch (Exception ex)
            {
                if (Log != null)
                {
                    await Log.WriteFatalErrorAsync(nameof(Startup), nameof(CleanUp), ex);
                }
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
                var persistenceManager = new LykkeLogToAzureStoragePersistenceManager(
                    AzureTableStorage<LogEntity>.Create(dbLogConnectionStringManager, "SlackNotificationsJobLogs", consoleLogger),
                    consoleLogger);

                var slackNotificationsManager = new LykkeLogToAzureSlackNotificationsManager(slackService, consoleLogger);

                var azureStorageLogger = new LykkeLogToAzureStorage(persistenceManager, slackNotificationsManager, consoleLogger);

                azureStorageLogger.Start();

                aggregateLogger.AddLog(azureStorageLogger);
            }

            return aggregateLogger;
        }
    }
}
