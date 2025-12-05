using BedeLotteryConsole.IO;
using BedeLotteryConsole.IO.Interfaces;
using BedeLotteryConsole.Services;
using BedeLotteryConsole.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BedeLotteryConsole.Settings;

namespace BedeLotteryConsole.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure LottoSettings from appsettings.json
        services.Configure<LottoSettings>(configuration.GetSection("LottoSettings"));

        // Register services
        services.AddSingleton<IGameService, GameService>();
        services.AddSingleton<IInputProcessor, InputProcessor>();
        services.AddSingleton<IOutputProcessor, OutputProcessor>();
        services.AddSingleton<IConsoleIO, ConsoleIO>();

        return services;
    }
}
