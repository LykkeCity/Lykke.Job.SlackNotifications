using System.Threading.Tasks;

namespace Lykke.Job.SlackNotifications.Core.Domain
{
    public interface IMessagesRepository
    {
        Task<string> SaveMessageAsync(string message);
    }
}