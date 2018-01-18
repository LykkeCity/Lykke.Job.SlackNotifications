using System;

namespace Lykke.Job.SlackNotifications.Core.Domain
{
    public class MuteItem
    {
        public string Value { get; set; }
        public TimeSpan TimeToMute { get; set; }
        public string Type { get; set; }
        public DateTime ExpireAt { get; set; }
        public long MutedMessagesCount { get; set; }
        
        public static MuteItem Create(string value, TimeSpan timeToMute, string type)
        {
            return new MuteItem
            {
                Value = value,
                TimeToMute = timeToMute,
                Type = type,
                ExpireAt = DateTime.UtcNow.Add(timeToMute)
            };
        }
    }
}
