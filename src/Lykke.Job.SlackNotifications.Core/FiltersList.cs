using System.Collections.Generic;

namespace Lykke.Job.SlackNotifications.Core
{
    public class FiltersList
    {
        public Dictionary<string, string> Senders { get; set; }
        public Dictionary<string, string> MessagePrefixes { get; set; }
        public Dictionary<string, string> MessageRegExps { get; set; }
    }
}
