using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Job.SlackNotifications.Core.Domain;
using AzureStorage;
using AzureStorage.Blob;
using Lykke.SettingsReader;

namespace Lykke.Job.SlackNotifications.Repositories
{
    public class MessagesRepository : IMessagesRepository
    {
        private static long _conter;
        private readonly IBlobStorage _storage;
        
        public static IMessagesRepository Create(IReloadingManager<string> connectionString)
        {
            return new MessagesRepository(AzureBlobStorage.Create(connectionString));
        }

        private MessagesRepository(IBlobStorage storage)
        {
            _storage = storage;
        }

        /// <summary>
        /// Returns absolute URI to the saved message blob
        /// </summary>
        public async Task<string> SaveMessageAsync(string message)
        {
            var counter = Interlocked.Increment(ref _conter);
            var now = DateTime.UtcNow;
            var key = $"{now:HH-mm-ss.fffffff}.{counter % 100:D2}.txt";
            var content = Encoding.UTF8.GetBytes(message);
            var containerName = GetContainerName(now);

            using (var stream = new MemoryStream(content))
            {
                return await _storage.SaveBlobAsync(containerName, key, stream, anonymousAccess: true);
            }
        }

        private static string GetContainerName(DateTime date)
        {
            return $"slack-notifications-full-messages-{date:yyyy-MM-dd}";
        }
    }
}
