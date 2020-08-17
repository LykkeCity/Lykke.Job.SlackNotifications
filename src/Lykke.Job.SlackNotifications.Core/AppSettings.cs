using System;
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

        public string FullMessagesConnString { get; set; }

        public string OpsgenieHost { get; set; }

        public string OpsgenieApiKey { get; set; }

        [Optional]
        public string ForwardMonitorMessagesQueueConnString { get; set; }

        [Optional]
        public Dictionary<string, MuteSettings> MutedSenders { get; set; }
        [Optional]
        public Dictionary<string, MuteSettings> MutedMessagePrefixes { get; set; }
        [Optional]
        public Dictionary<string, MuteSettings> MutedRegexMessage { get; set; }
    }

    public class MuteSettings
    {
        public TimeSpan TimeToMute { get; set; }

        [Optional]
        public string Type { get; set; }
    }

    public class SlackNotificationsSettings
    {
        public AzureQueueSettings AzureQueue { get; set; }
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

            [Optional]
            public bool Opsgenie { get; set; }
        }

        public string Env { get; set; }
        public Channel[] Channels { get; set; }
    }
}
