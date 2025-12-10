using BedeLotteryConsole.Models;
using BedeLotteryConsole.Services.Interfaces;
using Microsoft.Extensions.Options;
using BedeLotteryConsole.Settings;
using Scriban;
using System.Reflection;
using Scriban.Runtime;

namespace BedeLotteryConsole.Services;

public class GameService : IGameService
{
    private readonly LottoSettings _lottoSettings;
    private readonly IWinnersService _winnersService;

    private Template? _templateIntroduction;
    private Template? _templateResults;
    private readonly Random _random = new Random();

    private GameState? _gameState;

    public GameService(IOptions<LottoSettings> lottoSettings, IWinnersService winnersService)
    {
        _lottoSettings = lottoSettings.Value;
        _winnersService = winnersService;
        _gameState = null;
    }

    private CancellationTokenSource _cancellationTokenSource { get; set; } = new CancellationTokenSource();
    public CancellationToken AppCancellationToken => _cancellationTokenSource.Token;

    public void Initialize()
    {
        // Start listening for user input
        _ = ListenForUserInputAsync();

        // Initialize game state
        var totalCPUPlayers = _random.Next(10, _lottoSettings.MaxPlayersPerGame); // max not included (so max 14 CPU players if 15 is max in the config)
        var playerBalances = Enumerable.Range(1, totalCPUPlayers + 1)
            .ToDictionary(x => x, x => _lottoSettings.InitialBalance);

        _gameState = new GameState
        {
            PlayerBalances = playerBalances
        };

        _templateIntroduction = loadTemplate("BedeLotteryConsole.Templates.Introduction.tmpl");
        _templateResults = loadTemplate("BedeLotteryConsole.Templates.Results.tmpl");

        // Start initial render
        _ = PrintIntroduction();

        Template loadTemplate(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream!);
            var templateContent = reader.ReadToEnd();
            return Template.Parse(templateContent);
        }
    }

    private void OrderTickets(int numberOfTickets)
    {
        if (_gameState is null)
        {
            throw new InvalidOperationException("Game state is null.");
        }

        var maxBettableTicketsAccordingToCurrentBalance = (int)(_gameState.PlayerBalances[Constants.HumanPlayerId] / _lottoSettings.TicketPrice);

        if (numberOfTickets > _lottoSettings.MaxTicketsPerPlayers ||
            numberOfTickets > maxBettableTicketsAccordingToCurrentBalance)
        {
            throw new InvalidOperationException("Invalid number of tickets bet.");
        }

        var winnersOutput = _winnersService.CalculateWinners(new WinnersInput
        {
            Player1TicketsAmount = numberOfTickets,
            PlayerBalances = _gameState.PlayerBalances,
        }, _random);

        if (winnersOutput is null)
        {
            _gameState = new GameState
            {
                PlayerBalances = _gameState.PlayerBalances,
                LastRoundResults = _gameState.LastRoundResults
            };

            return;
        }

        _gameState = new GameState
        {
            PlayerBalances = winnersOutput.PlayerBalances,
            LastRoundResults = winnersOutput.LastRoundResults,
            CPUPlayersTicketsAmount = winnersOutput.CPUPlayersTicketsCount
        };
    }

    private async Task ListenForUserInputAsync()
    {
        await Task.Yield(); // Allow other tasks to start

        while (!AppCancellationToken.IsCancellationRequested)
        {
            try
            {
                var input = await Task.Run(() => Console.ReadLine(), AppCancellationToken);
                if (!string.IsNullOrEmpty(input))
                {
                    if (int.TryParse(input, out var numberOfTickets))
                    {
                        try
                        {
                            OrderTickets(numberOfTickets);

                            if (!_lottoSettings.SuppressOutput)
                            {
                                Console.WriteLine();
                                Console.WriteLine("You've chosen to order {0} tickets.", numberOfTickets);
                                Console.WriteLine();
                            }

                            await PrintResults();
                            _cancellationTokenSource.Cancel();
                        }
                        catch (Exception ex)
                        {
                            if (!_lottoSettings.SuppressOutput)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }
                    else
                    {
                        if (!_lottoSettings.SuppressOutput)
                        {
                            Console.WriteLine("Please enter a valid number of tickets.");
                        }
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                if (!_lottoSettings.SuppressOutput)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task PrintIntroduction()
    {
        if (_gameState is null || _templateIntroduction is null)
            return;

        try
        {
            var output = await _templateIntroduction.RenderAsync(new 
            { 
                state = _gameState,
                ticket_price = _lottoSettings.TicketPrice
            });

            if (!_lottoSettings.SuppressOutput)
            {
                Console.WriteLine(output);
            }
        }
        catch (Exception ex)
        {
            if (!_lottoSettings.SuppressOutput)
            {
                Console.WriteLine($"Error rendering template: {ex.Message}");
            }
        }
    }

    private async Task PrintResults()
    {
        if (_templateResults is null)
        {
            throw new InvalidOperationException("Template is not initialized.");
        }

        try
        {
            var output = await _templateResults.RenderAsync(new
            {
                state = _gameState,
                ticket_price = _lottoSettings.TicketPrice
            }, member => StandardMemberRenamer.Default(member));
            
            if (!_lottoSettings.SuppressOutput)
            {
                Console.WriteLine(output);
            }
        }
        catch (Exception ex)
        {
            if (!_lottoSettings.SuppressOutput)
            {
                Console.WriteLine($"Error rendering template: {ex.Message}");
            }
        }
    }
}