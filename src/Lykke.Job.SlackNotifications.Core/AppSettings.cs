namespace Lykke.Job.SlackNotifications.Core
{
    public class AppSettings
    {
        public SlackNotificationsSettings SlackNotifications { get; set; }

        public SlackNotificationsJobSettings SlackNotificationsJobSettings { get; set; }
    }

    public class SlackNotificationsJobSettings
    {
        public string LogsConnectionString { get; set; }

        public string SharedStorageConnString { get; set; }

        public SlackSettings Slack { get; set; }
    }

    public class SlackNotificationsSettings
    {
        public AzureQueueSettings AzureQueue { get; set; }

        public int ThrottlingLimitSeconds { get; set; }
    }

    public class AzureQueueSettings
    {
        public string ConnectionString { get; set; }

        public string QueueName { get; set; }
    }

    public class SlackSettings
    {
        public class Channel
        {
            public string Type { get; set; }
            public string WebHookUrl { get; set; }
        }

        public string Env { get; set; }
        public Channel[] Channels { get; set; }
    }
}
