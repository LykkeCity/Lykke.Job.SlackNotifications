using System.Linq;
using System.Collections.Generic;
using Lykke.Job.SlackNotifications.Core;
using Lykke.Job.SlackNotifications.Core.Services;

namespace Lykke.Job.SlackNotifications.Services
{
    public class NotificationFilter : INotificationFilter
    {
        private readonly HashSet<string> _mutedSenders = new HashSet<string>();
        private readonly HashSet<string> _mutedPrefixes = new HashSet<string>();

        public NotificationFilter(SlackNotificationsJobSettings settings)
        {
            _mutedSenders = settings.MutedSenders == null ? new HashSet<string>() : new HashSet<string>(settings.MutedSenders);
            _mutedPrefixes = new HashSet<string>(settings.MutedMessagePrefixes);
        }

        public void MuteSender(string sender)
        {
            _mutedSenders.Add(sender);
        }

        public void UnmuteSender(string sender)
        {
            _mutedSenders.Remove(sender);
        }

        public void MuteMessagePrefix(string prefix)
        {
            _mutedPrefixes.Add(prefix);
        }

        public void UnmuteMessagePrefix(string prefix)
        {
            _mutedPrefixes.Remove(prefix);
        }

        public bool ShouldMessageBeFilteredOut(SlackNotificationRequestMsg message)
        {
            if (_mutedSenders.Contains(message.Sender))
                return true;

            if (_mutedPrefixes.Any(p => message.Message.StartsWith(p)))
                return true;

            return false;
        }
    }
}
