using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Lykke.Job.SlackNotifications.Core;
using Lykke.Job.SlackNotifications.Core.Services;

namespace Lykke.Job.SlackNotifications.Services
{
    public class NotificationFilter : INotificationFilter
    {
        private readonly HashSet<string> _mutedSenders;
        private readonly HashSet<string> _mutedPrefixes;
        private readonly HashSet<string> _mutedMessagesRegex;

        public NotificationFilter(SlackNotificationsJobSettings settings)
        {
            _mutedSenders = settings.MutedSenders == null ? new HashSet<string>() : new HashSet<string>(settings.MutedSenders);
            _mutedPrefixes = new HashSet<string>(settings.MutedMessagePrefixes);
            _mutedMessagesRegex = settings.MutedRegexMessage == null ? new HashSet<string>() : new HashSet<string>(settings.MutedRegexMessage);
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

        public void MuteRegexMessage(string regex)
        {
            _mutedMessagesRegex.Add(regex);
        }

        public void UnmuteRegexMessage(string regex)
        {
            _mutedMessagesRegex.Remove(regex);
        }

        public FiltersList GetAllFilters()
        {
            return new FiltersList
            {
                Senders = _mutedSenders.ToArray(),
                MessagePrefixes = _mutedPrefixes.ToArray(),
                MessageRegExps = _mutedMessagesRegex.ToArray()
            };
        }

        public bool ShouldMessageBeFilteredOut(SlackNotificationRequestMsg message)
        {
            if (_mutedSenders.Contains(message.Sender))
                return true;

            if (_mutedPrefixes.Any(p => message.Message.StartsWith(p)))
                return true;
            
            if (_mutedMessagesRegex.Any(m => Regex.IsMatch(message.Message, m)))
                return true;

            return false;
        }
    }
}
