using System.Threading.Tasks;

namespace Lykke.Job.SlackNotifications.Core.Services
{
    public interface ISlackNotificationSender
    {
        Task SendNotificationAsync(string type, string message, string sender = null);
    }
}
