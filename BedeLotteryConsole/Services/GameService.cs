using System.Threading.Channels;
using BedeLotteryConsole.Models;
using BedeLotteryConsole.Commands.Interfaces;
using BedeLotteryConsole.Commands;
using BedeLotteryConsole.Services.Interfaces;
using BedeLotteryConsole.Algos;
using Microsoft.Extensions.Options;
using BedeLotteryConsole.Settings;
using BedeLotteryConsole.Exceptions;

namespace BedeLotteryConsole.Services;

public class GameService : IGameService
{
    private readonly Channel<GameState> _stateChannel = Channel.CreateUnbounded<GameState>();
    private readonly Channel<ICommand> _userInputChannel = Channel.CreateUnbounded<ICommand>();
    private readonly Channel<Exception> _exceptionChannel = Channel.CreateUnbounded<Exception>();
    private readonly LottoSettings _lottoSettings;
    private readonly Random _random = new Random();
    private const int MinimumPlayersInTheGameToContinue = 4;

    private GameState? _gameState;

    public ChannelReader<GameState> StateUpdates => _stateChannel.Reader;
    public ChannelWriter<ICommand> UserInput => _userInputChannel.Writer;
    public ChannelReader<Exception> Exception => _exceptionChannel.Reader;

    public GameService(IOptions<LottoSettings> lottoSettings)
    {
        _lottoSettings = lottoSettings.Value;
    }

    private CancellationTokenSource _cancellationTokenSource { get; set; } = new CancellationTokenSource();
    public CancellationToken AppCancellationToken => _cancellationTokenSource.Token;

    public async Task InitializeAsync()
    {
        _ = StartProcessingUserInputAsync();

        var totalCPUPlayers = _random.Next(10, _lottoSettings.MaxPlayersPerGame); // max not included (so max 14 CPU players if 15 is max in the config)
        var playerBalances = Enumerable.Range(1, totalCPUPlayers + 1)
            .ToDictionary(x => x, x => _lottoSettings.InitialBalance);

        _gameState = new GameState
        {
            PlayerBalances = playerBalances,
            StatusType = StatusType.WaitingForBet
        };

        await UpdateStateAsync(_gameState);
    }

    private async Task UpdateStateAsync(GameState newState)
    {
        await _stateChannel.Writer.WriteAsync(newState);
    }

    public async Task StartProcessingUserInputAsync()
    {
        await foreach (var input in _userInputChannel.Reader.ReadAllAsync(AppCancellationToken))
        {
            await ProcessUserInputAsync(input);
        }
    }

    private async Task ProcessUserInputAsync(ICommand input)
    {
        if (_gameState is null)
        {
            throw new InvalidOperationException("Game state is null.");
        }

        try
        {
            if (input is BetCommand betCommand)
            {
                var maxBettableTicketsAccordingToCurrentBalance = (int)(_gameState.PlayerBalances[Constants.HumanPlayerId] / _lottoSettings.TicketPrice);

                if (betCommand.NumberOfTickets > _lottoSettings.MaxTicketsPerPlayers ||
                    betCommand.NumberOfTickets > maxBettableTicketsAccordingToCurrentBalance)
                {
                    throw new InvalidOperationException("Invalid number of tickets bet.");
                }

                _gameState.Player1TicketsAmount = betCommand.NumberOfTickets;   

                try
                {
                    var winnersOutput = Winners.CalculateWinners(new WinnersInput
                    {
                        Player1TicketsAmount = _gameState.Player1TicketsAmount,
                        PlayerBalances = _gameState.PlayerBalances,
                    }, _lottoSettings, _random);

                    var isGameOverBecauseBalanceIsTooLow = winnersOutput.PlayerBalances[Constants.HumanPlayerId] < _lottoSettings.TicketPrice;
                    var isGameOverBecauseOfNotEnoughPlayers = winnersOutput.CPUPlayersTicketsCount < MinimumPlayersInTheGameToContinue;
                    
                    _gameState = new GameState
                    {
                        PlayerBalances = winnersOutput.PlayerBalances,
                        LastRoundResults = winnersOutput.LastRoundResults,
                        StatusType = isGameOverBecauseBalanceIsTooLow || isGameOverBecauseOfNotEnoughPlayers ? StatusType.GameOver : StatusType.ShowingResults,
                        Player1TicketsAmount = 0,
                        CPUPlayersTicketsAmount = winnersOutput.CPUPlayersTicketsCount
                    };

                    await UpdateStateAsync(_gameState);
                }
                catch (NotEnoughTicketsAvailableException)
                {
                    _gameState = new GameState
                    {
                        PlayerBalances = _gameState.PlayerBalances,
                        LastRoundResults = _gameState.LastRoundResults,
                        StatusType = StatusType.CannotStartDrawNotEnoughTickets,
                        Player1TicketsAmount = 0
                    };

                    await UpdateStateAsync(_gameState);
                }
            }
            else if (input is NextRoundCommand)
            {
                var newGameState = new GameState
                {
                    PlayerBalances = _gameState.PlayerBalances,
                    StatusType = StatusType.WaitingForBet,
                    Player1TicketsAmount = 0
                };

                await UpdateStateAsync(newGameState);
            }
            else if (input is ExitCommand)
            {
                _cancellationTokenSource.Cancel();
            }
        }
        catch (Exception ex)
        {
            await _exceptionChannel.Writer.WriteAsync(ex);
        }
    }
}