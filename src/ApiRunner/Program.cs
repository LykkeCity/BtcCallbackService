using System;
using System.IO;
using System.Linq;
using Core.Settings;
using Lykke.Bitcoin.CallbackService;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace ApiRunner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var arguments = args.Select(t => t.Split('=')).ToDictionary(spl => spl[0].Trim('-'), spl => spl[1]);

            Console.Clear();
            Console.Title = "Prebroadcast handler self-hosted API - Ver. " + Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion;

            var builder = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>();

            if (arguments.ContainsKey("port"))
                builder.UseUrls($"http://*:{arguments["port"]}");

            Console.WriteLine($"Web Server is running");
            Console.WriteLine("Utc time: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

            var host = builder.Build();

            host.Run();
        }
    }
}