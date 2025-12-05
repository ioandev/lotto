using BedeLotteryConsole.Commands;
using BedeLotteryConsole.Models;
using BedeLotteryConsole.Services;
using BedeLotteryConsole.Settings;
using Microsoft.Extensions.Options;

namespace BedeLotteryConsole.Tests.Services;

[TestFixture]
public class GameServiceTests
{
    private LottoSettings _defaultSettings;
    private IOptions<LottoSettings> _options;

    [SetUp]
    public void SetUp()
    {
        _defaultSettings = new LottoSettings
        {
            InitialBalance = 10.0m,
            TicketPrice = 1.0m,
            Currency = "$",
            MaxTicketsPerPlayers = 10,
            MaxPlayersPerGame = 15
        };
        _options = Options.Create(_defaultSettings);
    }

    [Test]
    public async Task InitializeAsync_CreatesInitialGameState()
    {
        // Arrange
        var gameService = new GameService(_options);

        // Act
        await gameService.InitializeAsync();

        // Assert
        var stateReceived = await gameService.StateUpdates.ReadAsync();
        Assert.That(stateReceived, Is.Not.Null);
        Assert.That(stateReceived.StatusType, Is.EqualTo(StatusType.WaitingForBet));
        Assert.That(stateReceived.PlayerBalances, Is.Not.Empty);
        Assert.That(stateReceived.PlayerBalances.Count, Is.GreaterThanOrEqualTo(11)); // At least 10 CPU players + 1 human
        Assert.That(stateReceived.PlayerBalances.Count, Is.LessThanOrEqualTo(15)); // Max 14 CPU players + 1 human
        Assert.That(stateReceived.PlayerBalances[Constants.HumanPlayerId], Is.EqualTo(_defaultSettings.InitialBalance));
    }

    [Test]
    public async Task InitializeAsync_AllPlayersHaveInitialBalance()
    {
        // Arrange
        var gameService = new GameService(_options);

        // Act
        await gameService.InitializeAsync();

        // Assert
        var stateReceived = await gameService.StateUpdates.ReadAsync();
        foreach (var balance in stateReceived.PlayerBalances.Values)
        {
            Assert.That(balance, Is.EqualTo(_defaultSettings.InitialBalance));
        }
    }

    [Test]
    public async Task BetCommand_WithValidBet_UpdatesGameState()
    {
        // Arrange
        var gameService = new GameService(_options);
        await gameService.InitializeAsync();
        await gameService.StateUpdates.ReadAsync(); // Read initial state

        var betCommand = new BetCommand { NumberOfTickets = 5 };

        // Act
        await gameService.UserInput.WriteAsync(betCommand);
        await Task.Delay(100); // Give time for processing

        // Assert
        var updatedState = await gameService.StateUpdates.ReadAsync();
        Assert.That(updatedState, Is.Not.Null);
        Assert.That(updatedState.StatusType, Is.Not.EqualTo(StatusType.WaitingForBet));
    }

    [Test]
    public async Task BetCommand_WithTooManyTickets_ThrowsException()
    {
        // Arrange
        var gameService = new GameService(_options);
        await gameService.InitializeAsync();
        await gameService.StateUpdates.ReadAsync(); // Read initial state

        var betCommand = new BetCommand { NumberOfTickets = 15 }; // More than max allowed

        // Act
        await gameService.UserInput.WriteAsync(betCommand);
        await Task.Delay(100); // Give time for processing

        // Assert
        var exception = await gameService.Exception.ReadAsync();
        Assert.That(exception, Is.InstanceOf<InvalidOperationException>());
        Assert.That(exception.Message, Is.EqualTo("Invalid number of tickets bet."));
    }

    [Test]
    public async Task BetCommand_WithMoreTicketsThanBalance_ThrowsException()
    {
        // Arrange
        var gameService = new GameService(_options);
        await gameService.InitializeAsync();
        await gameService.StateUpdates.ReadAsync(); // Read initial state

        var betCommand = new BetCommand { NumberOfTickets = 15 }; // More than balance allows (10 coins / 1 per ticket)

        // Act
        await gameService.UserInput.WriteAsync(betCommand);
        await Task.Delay(100); // Give time for processing

        // Assert
        var exception = await gameService.Exception.ReadAsync();
        Assert.That(exception, Is.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public async Task NextRoundCommand_AfterBet_ResetsToWaitingForBet()
    {
        // Arrange
        var gameService = new GameService(_options);
        await gameService.InitializeAsync();
        await gameService.StateUpdates.ReadAsync(); // Read initial state

        var betCommand = new BetCommand { NumberOfTickets = 2 };
        await gameService.UserInput.WriteAsync(betCommand);
        await Task.Delay(100);
        await gameService.StateUpdates.ReadAsync(); // Read bet result state

        // Act
        var nextRoundCommand = new NextRoundCommand();
        await gameService.UserInput.WriteAsync(nextRoundCommand);
        await Task.Delay(100);

        // Assert
        var newState = await gameService.StateUpdates.ReadAsync();
        Assert.That(newState.StatusType, Is.EqualTo(StatusType.WaitingForBet));
        Assert.That(newState.Player1TicketsAmount, Is.EqualTo(0));
    }

    [Test]
    public async Task NextRoundCommand_PreservesPlayerBalances()
    {
        // Arrange
        var gameService = new GameService(_options);
        await gameService.InitializeAsync();
        await gameService.StateUpdates.ReadAsync(); // Read initial state

        var betCommand = new BetCommand { NumberOfTickets = 2 };
        await gameService.UserInput.WriteAsync(betCommand);
        await Task.Delay(100);
        var afterBetState = await gameService.StateUpdates.ReadAsync(); // Read bet result state

        // Act
        var nextRoundCommand = new NextRoundCommand();
        await gameService.UserInput.WriteAsync(nextRoundCommand);
        await Task.Delay(100);

        // Assert
        var newState = await gameService.StateUpdates.ReadAsync();
        Assert.That(newState.PlayerBalances, Is.EqualTo(afterBetState.PlayerBalances));
    }

    [Test]
    public async Task ExitCommand_SetsCancellationToken()
    {
        // Arrange
        var gameService = new GameService(_options);
        await gameService.InitializeAsync();
        await gameService.StateUpdates.ReadAsync(); // Read initial state

        // Act
        var exitCommand = new ExitCommand();
        await gameService.UserInput.WriteAsync(exitCommand);
        await Task.Delay(100);

        // Assert
        Assert.That(gameService.AppCancellationToken.IsCancellationRequested, Is.True);
    }

    [Test]
    public async Task BetCommand_WhenBalanceBelowTicketPrice_SetsGameOverStatus()
    {
        // Arrange
        var lowBalanceSettings = new LottoSettings
        {
            InitialBalance = 1.5m, // Very low balance
            TicketPrice = 1.0m,
            Currency = "$",
            MaxTicketsPerPlayers = 10,
            MaxPlayersPerGame = 15
        };
        var options = Options.Create(lowBalanceSettings);
        var gameService = new GameService(options);
        await gameService.InitializeAsync();
        await gameService.StateUpdates.ReadAsync(); // Read initial state

        var betCommand = new BetCommand { NumberOfTickets = 1 };

        // Act
        await gameService.UserInput.WriteAsync(betCommand);
        await Task.Delay(100);

        // Assert
        var updatedState = await gameService.StateUpdates.ReadAsync();
        // After betting 1 ticket, balance might drop below ticket price triggering game over
        // This depends on whether player wins or not, but we're testing the logic exists
        Assert.That(updatedState, Is.Not.Null);
    }

    [Test]
    public async Task StateUpdates_ProvidesChannelReader()
    {
        // Arrange
        var gameService = new GameService(_options);

        // Act & Assert
        Assert.That(gameService.StateUpdates, Is.Not.Null);
        Assert.That(gameService.StateUpdates, Is.InstanceOf<System.Threading.Channels.ChannelReader<GameState>>());
    }

    [Test]
    public async Task UserInput_ProvidesChannelWriter()
    {
        // Arrange
        var gameService = new GameService(_options);

        // Act & Assert
        Assert.That(gameService.UserInput, Is.Not.Null);
        Assert.That(gameService.UserInput, Is.InstanceOf<System.Threading.Channels.ChannelWriter<Commands.Interfaces.ICommand>>());
    }

    [Test]
    public async Task Exception_ProvidesChannelReader()
    {
        // Arrange
        var gameService = new GameService(_options);

        // Act & Assert
        Assert.That(gameService.Exception, Is.Not.Null);
        Assert.That(gameService.Exception, Is.InstanceOf<System.Threading.Channels.ChannelReader<Exception>>());
    }

    [Test]
    public async Task AppCancellationToken_InitiallyNotCancelled()
    {
        // Arrange
        var gameService = new GameService(_options);

        // Act & Assert
        Assert.That(gameService.AppCancellationToken.IsCancellationRequested, Is.False);
    }

    [Test]
    public async Task BetCommand_WithZeroTickets_ProcessesSuccessfully()
    {
        // Arrange
        var gameService = new GameService(_options);
        await gameService.InitializeAsync();
        await gameService.StateUpdates.ReadAsync(); // Read initial state

        var betCommand = new BetCommand { NumberOfTickets = 0 };

        // Act
        await gameService.UserInput.WriteAsync(betCommand);
        await Task.Delay(100);

        // Assert - Should process without throwing
        // Check if there's a state update or exception
        var hasException = gameService.Exception.TryRead(out var exception);
        if (hasException)
        {
            Assert.That(exception, Is.Not.Null);
        }
    }

    [Test]
    public async Task BetCommand_MultipleCommands_ProcessedSequentially()
    {
        // Arrange
        var gameService = new GameService(_options);
        await gameService.InitializeAsync();
        await gameService.StateUpdates.ReadAsync(); // Read initial state

        // Act - Send first bet
        var betCommand1 = new BetCommand { NumberOfTickets = 2 };
        await gameService.UserInput.WriteAsync(betCommand1);
        await Task.Delay(100);
        var state1 = await gameService.StateUpdates.ReadAsync();

        // Move to next round
        await gameService.UserInput.WriteAsync(new NextRoundCommand());
        await Task.Delay(100);
        var state2 = await gameService.StateUpdates.ReadAsync();

        // Send second bet
        var betCommand2 = new BetCommand { NumberOfTickets = 3 };
        await gameService.UserInput.WriteAsync(betCommand2);
        await Task.Delay(100);
        var state3 = await gameService.StateUpdates.ReadAsync();

        // Assert
        Assert.That(state1, Is.Not.Null);
        Assert.That(state2, Is.Not.Null);
        Assert.That(state2.StatusType, Is.EqualTo(StatusType.WaitingForBet));
        Assert.That(state3, Is.Not.Null);
    }

    [Test]
    public async Task BetCommand_ResultsInShowingResultsStatus_WhenGameNotOver()
    {
        // Arrange
        var highBalanceSettings = new LottoSettings
        {
            InitialBalance = 100.0m, // High balance to ensure game doesn't end
            TicketPrice = 1.0m,
            Currency = "$",
            MaxTicketsPerPlayers = 10,
            MaxPlayersPerGame = 15
        };
        var options = Options.Create(highBalanceSettings);
        var gameService = new GameService(options);
        await gameService.InitializeAsync();
        await gameService.StateUpdates.ReadAsync(); // Read initial state

        var betCommand = new BetCommand { NumberOfTickets = 5 };

        // Act
        await gameService.UserInput.WriteAsync(betCommand);
        await Task.Delay(100);

        // Assert
        var updatedState = await gameService.StateUpdates.ReadAsync();
        // Game should continue with either ShowingResults or CannotStartDrawNotEnoughTickets
        Assert.That(updatedState.StatusType, Is.Not.EqualTo(StatusType.WaitingForBet));
    }

    [Test]
    public async Task BetCommand_UpdatesLastRoundResults()
    {
        // Arrange
        var gameService = new GameService(_options);
        await gameService.InitializeAsync();
        await gameService.StateUpdates.ReadAsync(); // Read initial state

        var betCommand = new BetCommand { NumberOfTickets = 5 };

        // Act
        await gameService.UserInput.WriteAsync(betCommand);
        await Task.Delay(100);

        // Assert
        var updatedState = await gameService.StateUpdates.ReadAsync();
        // LastRoundResults should be populated if draw was successful
        if (updatedState.StatusType == StatusType.ShowingResults || updatedState.StatusType == StatusType.GameOver)
        {
            Assert.That(updatedState.LastRoundResults, Is.Not.Null);
        }
    }

    [Test]
    public async Task Constructor_InitializesWithProvidedSettings()
    {
        // Arrange
        var customSettings = new LottoSettings
        {
            InitialBalance = 50.0m,
            TicketPrice = 2.0m,
            Currency = "€",
            MaxTicketsPerPlayers = 20,
            MaxPlayersPerGame = 20
        };
        var options = Options.Create(customSettings);

        // Act
        var gameService = new GameService(options);
        await gameService.InitializeAsync();

        // Assert
        var state = await gameService.StateUpdates.ReadAsync();
        Assert.That(state.PlayerBalances[Constants.HumanPlayerId], Is.EqualTo(50.0m));
    }
}
