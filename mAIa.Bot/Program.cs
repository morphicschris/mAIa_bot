namespace DiscordTest
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using mAIa.Bot;
    using mAIa.Bot.Services;
    using mAIa.Bot.Chat;

    public static class Program
    {
        public async static Task Main(string[] args)
        {
            ConsoleWriter.LogLevel = LogLevel.Debug;
            var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddJsonFile(@"c:\Workspace\keys.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddScoped(typeof(IChatService), typeof(OpenAIChatService));
                    services.AddHostedService<mAIaBot>();
                });
    }
}