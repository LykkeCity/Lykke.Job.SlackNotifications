﻿namespace Lykke.Job.SlackNotifications.Models
{
    public class IsAliveResponse
    {
        public string Version { get; set; }
        public string Env { get; set; }
        public string Name { get; set; }
        public bool IsDebug { get; set; }
    }
}
