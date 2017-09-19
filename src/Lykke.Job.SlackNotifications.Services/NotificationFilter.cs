using System.Collections.Generic;
using Lykke.Job.SlackNotifications.Core;
using Lykke.Job.SlackNotifications.Core.Services;

namespace Lykke.Job.SlackNotifications.Services
{
    public class NotificationFilter : INotificationFilter
    {
        private readonly HashSet<string> _mutedSenders = new HashSet<string>();

        public NotificationFilter(SlackNotificationsJobSettings settings)
        {
            if (settings.MutedSenders != null)
                foreach (var mutedSender in settings.MutedSenders)
                {
                    _mutedSenders.Add(mutedSender);
                }
        }

        public void MuteSender(string sender)
        {
            _mutedSenders.Add(sender);
        }

        public void UnmuteSender(string sender)
        {
            _mutedSenders.Remove(sender);
        }

        public bool ShouldMessageBeFilteredOut(SlackNotificationRequestMsg message)
        {
            return _mutedSenders.Contains(message.Sender);
        }
    }
}
