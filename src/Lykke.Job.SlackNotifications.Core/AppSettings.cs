using System.Collections.Generic;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.SlackNotifications.Core
{
    public class AppSettings
    {
        public SlackNotificationsSettings SlackNotifications { get; set; }

        public SlackSettings SlackIntegration { get; set; }

        public SlackNotificationsJobSettings SlackNotificationsJobSettings { get; set; }
    }

    public class SlackNotificationsJobSettings
    {
        public string LogsConnectionString { get; set; }

        public string SharedStorageConnString { get; set; }

        [Optional]
        public List<string> MutedSenders { get; set; }

        public List<string> MutedMessagePrefixes { get; set; }
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
            [Optional]
            public int MaxShortMessageLength { get; set; }
        }

        public string Env { get; set; }
        public Channel[] Channels { get; set; }
    }
}
