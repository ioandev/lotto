
using System.Diagnostics.CodeAnalysis;
using BedeLotteryConsole.Extensions;
using BedeLotteryConsole.IO.Interfaces;
using BedeLotteryConsole.Services.Interfaces;
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
                services.AddApplicationServices(context.Configuration);
            });

    public static async Task RunAsync(IServiceProvider services)
    {
        var gameService = services.GetRequiredService<IGameService>();
        var consoleIO = services.GetRequiredService<IConsoleIO>();

        consoleIO.Initialize();

        await gameService.InitializeAsync();

        while (!gameService.AppCancellationToken.IsCancellationRequested)
        {
            await Task.Delay(100);
        }
    }
}
