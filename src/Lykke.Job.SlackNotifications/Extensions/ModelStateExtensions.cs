using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Lykke.Job.SlackNotifications.Extensions
{
    public static class ModelStateExtensions
    {
        public static string GetError(this ModelStateDictionary modelState)
        {
            foreach (var state in modelState)
            {
                var message = state.Value.Errors
                    .Where(e => !string.IsNullOrWhiteSpace(e.ErrorMessage))
                    .Select(e => e.ErrorMessage)
                    .Concat(state.Value.Errors
                        .Where(e => string.IsNullOrWhiteSpace(e.ErrorMessage))
                        .Select(e => e.Exception.Message))
                    .ToList().FirstOrDefault();

                if (string.IsNullOrEmpty(message))
                    continue;

                return message;
            }

            return string.Empty;
        }
    }
}
