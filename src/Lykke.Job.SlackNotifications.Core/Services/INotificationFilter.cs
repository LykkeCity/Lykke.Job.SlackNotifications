namespace Lykke.Job.SlackNotifications.Core.Services
{
    public interface INotificationFilter
    {
        void MuteSender(string sender);

        void UnmuteSender(string sender);

        bool ShouldMessageBeFilteredOut(SlackNotificationRequestMsg message);
    }
}
