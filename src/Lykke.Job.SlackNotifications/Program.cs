using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.PlatformAbstractions;

namespace Lykke.Job.SlackNotifications
{
    public class Program
    {
        public static string EnvInfo => Environment.GetEnvironmentVariable("ENV_INFO");
        
        public static void Main(string[] args)
        {
            Console.WriteLine($"Lykke.Job.SlackNotifications version {PlatformServices.Default.Application.ApplicationVersion}");
#if DEBUG
            Console.WriteLine("Is DEBUG");
#else
            Console.WriteLine("Is RELEASE");
#endif           
            Console.WriteLine($"ENV_INFO: {EnvInfo}");

            try
            {
                BuildWebHost(args).Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal error:");
                Console.WriteLine(ex);

                // Lets devops to see startup error in console between restarts in the Kubernetes
                var delay = TimeSpan.FromMinutes(1);

                Console.WriteLine();
                Console.WriteLine($"Process will be terminated in {delay}. Press any key to terminate immediately.");

                Task.WhenAny(
                        Task.Delay(delay),
                        Task.Run(() =>
                        {
                            Console.ReadKey(true);
                        }))
                    .Wait();
            }

            Console.WriteLine("Terminated");
        }

        private static IWebHost BuildWebHost(string[] args) =>
            new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseUrls("http://*:5000")
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();
    }
}
