using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Job.SlackNotifications.Core;
using Lykke.Job.SlackNotifications.Core.Domain;
using Lykke.Job.SlackNotifications.Core.Services;

namespace Lykke.Job.SlackNotifications.Services
{
    public class NotificationFilter : INotificationFilter
    {
        private readonly ILog _log;
        private readonly Dictionary<string, MuteItem> _mutedSenders = new Dictionary<string, MuteItem>();
        private readonly Dictionary<string, MuteItem> _mutedPrefixes = new Dictionary<string, MuteItem>();
        private readonly Dictionary<string, MuteItem> _mutedMessagesRegex = new Dictionary<string, MuteItem>();

        public NotificationFilter(
            SlackNotificationsJobSettings settings,
            ILog log
            )
        {
            _log = log;
            if (settings.MutedSenders != null)
                FillDictionary(settings.MutedSenders, _mutedSenders);
            if (settings.MutedMessagePrefixes != null)
                FillDictionary(settings.MutedMessagePrefixes, _mutedPrefixes);
            if (settings.MutedRegexMessage != null)
                FillDictionary(settings.MutedRegexMessage, _mutedMessagesRegex);
        }

        public void MuteSender(MuteItem item)
        {
            _mutedSenders[item.Value] = item;
        }

        public void UnmuteSender(string sender)
        {
            _mutedSenders.Remove(sender);
        }

        public void MuteMessagePrefix(MuteItem item)
        {
            _mutedPrefixes[item.Value] = item;
        }

        public void UnmuteMessagePrefix(string prefix)
        {
            _mutedPrefixes.Remove(prefix);
        }

        public void MuteRegexMessage(MuteItem item)
        {
            _mutedMessagesRegex[item.Value] = item;
        }

        public void UnmuteRegexMessage(string regex)
        {
            _mutedMessagesRegex.Remove(regex);
        }

        public FiltersList GetAllFilters()
        {
            return new FiltersList
            {
                Senders = GetExpireInfo(_mutedSenders),
                MessagePrefixes = GetExpireInfo(_mutedPrefixes),
                MessageRegExps = GetExpireInfo(_mutedMessagesRegex)
            };
        }

        public void UnmuteExpired()
        {
            var now = DateTime.UtcNow;

            RemoveExpired(_mutedSenders, now);
            RemoveExpired(_mutedPrefixes, now);
            RemoveExpired(_mutedMessagesRegex, now);
        }

        public async Task<MuteItem> GetMutedItem(SlackNotificationRequestMsg message)
        {
            string key = _mutedSenders.Keys.FirstOrDefault(item => message.Sender.IndexOf(item, StringComparison.OrdinalIgnoreCase) >= 0);
            
            if (!string.IsNullOrEmpty(key))
                return _mutedSenders[key];

            key = _mutedSenders.Keys.FirstOrDefault(item => message.Message.StartsWith(item));
            
            if (!string.IsNullOrEmpty(key))
            {
                await _log.WriteInfoAsync(nameof(NotificationFilter), nameof(GetMutedItem), message.ToJson(), "Sender set in the Message property");
                return _mutedSenders[key];
            }

            key = _mutedPrefixes.Keys.FirstOrDefault(item => message.Message.StartsWith(item));

            if (!string.IsNullOrEmpty(key))
                return _mutedPrefixes[key];

            key = _mutedMessagesRegex.Keys.FirstOrDefault(item => Regex.IsMatch(message.Message, item));

            if (!string.IsNullOrEmpty(key))
                return _mutedMessagesRegex[key];

            return null;
        }

        private void FillDictionary(Dictionary<string, MuteSettings> muteItems, Dictionary<string, MuteItem> dict)
        {
            foreach (var pair in muteItems)
            {
                dict.Add(pair.Key, MuteItem.Create(pair.Key, pair.Value.TimeToMute, pair.Value.Type));
            }
        }

        private Dictionary<string, string> GetExpireInfo(Dictionary<string, MuteItem> dict)
        {
            var now = DateTime.UtcNow;
            var result = new Dictionary<string, string>();

            foreach (KeyValuePair<string, MuteItem> pair in dict)
            {
                int minutes = Math.Max((int)Math.Round((pair.Value.ExpireAt - now).TotalMinutes), 1);
                string min = minutes > 1 ? "minutes" : "minute";
                result[pair.Key] = $"Muted messages: {pair.Value.MutedMessagesCount}. Will be unnmuted in {minutes} {min} (expires at {pair.Value.ExpireAt:T})";
            }

            return result;
        }

        private void RemoveExpired(Dictionary<string, MuteItem> dict, DateTime date)
        {
            foreach (var key in dict.Where(item => date >= item.Value.ExpireAt).Select(item => item.Key).ToList())
                dict.Remove(key);
        }
    }
}
