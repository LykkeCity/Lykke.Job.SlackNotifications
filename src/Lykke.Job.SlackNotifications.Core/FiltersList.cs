namespace Lykke.Job.SlackNotifications.Core
{
    public class FiltersList
    {
        public string[] Senders { get; set; }
        public string[] MessagePrefixes { get; set; }
        public string[] MessageRegExps { get; set; }
    }
}
