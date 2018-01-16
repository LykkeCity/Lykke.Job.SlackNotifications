using System.Threading.Tasks;
using Lykke.Job.SlackNotifications.Core.Domain;

namespace Lykke.Job.SlackNotifications.Core.Services
{
    public interface INotificationFilter
    {
        void MuteSender(MuteItem item);
        void UnmuteSender(string sender);

        void MuteMessagePrefix(MuteItem item);
        void UnmuteMessagePrefix(string prefix);
        void MuteRegexMessage(MuteItem item);
        void UnmuteRegexMessage(string regex);
        FiltersList GetAllFilters();
        void UnmuteExpired();

        Task<MuteItem> GetMutedItem(SlackNotificationRequestMsg message);
    }
}
