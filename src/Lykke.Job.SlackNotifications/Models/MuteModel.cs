using System;
using System.ComponentModel.DataAnnotations;
using Lykke.Job.SlackNotifications.Core.Domain;

namespace Lykke.Job.SlackNotifications.Models
{
    public class MuteModel
    {
        [Required]
        public string Value { get; set; }
        [Required]
        public TimeSpan TimeToMute { get; set; }
        public string Type { get; set; }

        public MuteItem ToDomain() => MuteItem.Create(Value, TimeToMute, Type);
    }
}
