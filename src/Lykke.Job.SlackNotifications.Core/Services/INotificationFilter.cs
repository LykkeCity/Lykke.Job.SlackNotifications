namespace Lykke.Job.SlackNotifications.Core.Services
{
    public interface INotificationFilter
    {
        void MuteSender(string sender);
        void UnmuteSender(string sender);

        void MuteMessagePrefix(string prefix);
        void UnmuteMessagePrefix(string prefix);

        bool ShouldMessageBeFilteredOut(SlackNotificationRequestMsg message);
    }
}
