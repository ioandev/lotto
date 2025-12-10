
using System.Diagnostics.CodeAnalysis;
using BedeLotteryConsole.Services;
using BedeLotteryConsole.Services.Interfaces;
using BedeLotteryConsole.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BedeLotteryConsole;

[ExcludeFromCodeCoverage]
public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        await RunAsync(host.Services);
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                // Configure LottoSettings from appsettings.json
                services.Configure<LottoSettings>(context.Configuration.GetSection("LottoSettings"));

                // Register services
                services.AddSingleton<IGameService, GameService>();
            });

    public static async Task RunAsync(IServiceProvider services)
    {
        var gameService = services.GetRequiredService<IGameService>();

        gameService.Initialize();

        try
        {
            while (!gameService.AppCancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1_000, gameService.AppCancellationToken);
            }
        }
        catch(TaskCanceledException)
        {
            // Expected when the application is shutting down
        }
    }
}
