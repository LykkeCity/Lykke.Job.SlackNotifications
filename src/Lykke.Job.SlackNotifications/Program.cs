using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.PlatformAbstractions;

namespace Lykke.Job.SlackNotifications
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine($"Lykke.Job.SlackNotifications version {PlatformServices.Default.Application.ApplicationVersion}");

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseUrls("http://*:5000")
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();
    }
}
