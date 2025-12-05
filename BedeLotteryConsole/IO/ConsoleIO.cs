using Scriban;
using System.Reflection;
using System.Threading.Channels;
using BedeLotteryConsole.Models;
using BedeLotteryConsole.Services.Interfaces;
using BedeLotteryConsole.IO.Interfaces;
using Microsoft.Extensions.Options;
using BedeLotteryConsole.Settings;

namespace BedeLotteryConsole.IO;

public class ConsoleIO : IConsoleIO
{
    private GameState? _gameState;
    private readonly IGameService _gameService;
    private readonly IInputProcessor _inputProcessor;
    private readonly LottoSettings _lottoSettings;
    private Template? _template;

    public ConsoleIO(IGameService gameService, IInputProcessor inputProcessor, IOptions<LottoSettings> lottoSettings)
    {
        _gameService = gameService;
        _inputProcessor = inputProcessor;
        _lottoSettings = lottoSettings.Value;
    }

    public void Initialize()
    {
        // Start listening for updates
        _ = ListenForUpdatesAsync(_gameService.StateUpdates);

        // Start listening for user input
        _ = ListenForUserInputAsync();

        // Start listening for exceptions
        _ = ListenForExceptionsAsync(_gameService.Exception);

        InitTemplate();

        void InitTemplate()
        {
            // clean up everything in the console
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "BedeLotteryConsole.Templates.ConsoleTemplate.txt";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream!);
            var templateContent = reader.ReadToEnd();

            _template = Template.Parse(templateContent);
        }
    }

    private async Task ListenForUpdatesAsync(ChannelReader<GameState> reader)
    {
        await foreach (var state in reader.ReadAllAsync(_gameService.AppCancellationToken))
        {
            _gameState = state;
            await Refresh();
        }
    }

    private async Task ListenForExceptionsAsync(ChannelReader<Exception> reader)
    {
        await foreach (var exception in reader.ReadAllAsync(_gameService.AppCancellationToken))
        {
            Console.WriteLine($"An error occurred: {exception.Message}");
        }
    }

    private async Task Refresh()
    {
        if (_gameState is null || _template is null)
            return;

        Console.Clear();

        try
        {
            var result = await _template.RenderAsync(new 
            { 
                state = _gameState,
                currency = _lottoSettings.Currency,
                ticket_price = _lottoSettings.TicketPrice
            });

            Console.WriteLine(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error rendering template: {ex.Message}");
        }
    }

    private async Task ListenForUserInputAsync()
    {
        await Task.Yield(); // Allow other tasks to start

        while (!_gameService.AppCancellationToken.IsCancellationRequested)
        {
            try
            {
                var input = await Task.Run(() => Console.ReadLine(), _gameService.AppCancellationToken);
                if (!string.IsNullOrEmpty(input))
                {
                    await _inputProcessor.Process(_gameState!, input);
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}