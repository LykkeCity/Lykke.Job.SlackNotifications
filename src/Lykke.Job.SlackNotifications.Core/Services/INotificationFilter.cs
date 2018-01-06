using System.Threading.Tasks;

namespace Lykke.Job.SlackNotifications.Core.Services
{
    public interface INotificationFilter
    {
        void MuteSender(string sender);
        void UnmuteSender(string sender);

        void MuteMessagePrefix(string prefix);
        void UnmuteMessagePrefix(string prefix);
        void MuteRegexMessage(string regex);
        void UnmuteRegexMessage(string regex);
        FiltersList GetAllFilters();

        Task<bool> ShouldMessageBeFilteredOut(SlackNotificationRequestMsg message);
    }
}
