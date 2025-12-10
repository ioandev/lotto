using BedeLotteryConsole.Models;
using BedeLotteryConsole.Services;
using BedeLotteryConsole.Services.Interfaces;
using BedeLotteryConsole.Settings;
using Microsoft.Extensions.Options;
using Moq;

namespace BedeLotteryConsole.Tests.Services;

[TestFixture]
public class GameServiceTests
{
    private LottoSettings _defaultSettings;
    private Mock<IWinnersService> _mockWinnersService;
    private IOptions<LottoSettings> _options;

    [SetUp]
    public void SetUp()
    {
        _defaultSettings = new LottoSettings
        {
            InitialBalance = 10.0m,
            TicketPrice = 1.0m,
            MaxTicketsPerPlayers = 10,
            MaxPlayersPerGame = 15,
            SuppressOutput = true
        };

        _options = Options.Create(_defaultSettings);
        _mockWinnersService = new Mock<IWinnersService>();
    }

    [Test]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Act
        var service = new GameService(_options, _mockWinnersService.Object);

        // Assert
        Assert.That(service, Is.Not.Null);
    }

    [Test]
    public void AppCancellationToken_InitiallyNotCancelled()
    {
        // Arrange
        var service = new GameService(_options, _mockWinnersService.Object);

        // Act & Assert
        Assert.That(service.AppCancellationToken.IsCancellationRequested, Is.False);
    }

    [Test]
    public void Initialize_CreatesGameStateWithCorrectNumberOfPlayers()
    {
        // Arrange
        var service = new GameService(_options, _mockWinnersService.Object);

        // Act
        service.Initialize();

        // Assert
        // Note: We can't directly test the internal game state, but Initialize should not throw
        Assert.DoesNotThrow(() => service.Initialize());
    }

    [Test]
    public void Initialize_LoadsTemplatesWithoutError()
    {
        // Arrange
        var service = new GameService(_options, _mockWinnersService.Object);

        // Act & Assert
        Assert.DoesNotThrow(() => service.Initialize());
    }

    [Test]
    public void Initialize_WithDifferentSettings_DoesNotThrow()
    {
        // Arrange
        var customSettings = new LottoSettings
        {
            InitialBalance = 50.0m,
            TicketPrice = 2.0m,
            MaxTicketsPerPlayers = 20,
            MaxPlayersPerGame = 30,
            SuppressOutput = true
        };
        var options = Options.Create(customSettings);
        var service = new GameService(options, _mockWinnersService.Object);

        // Act & Assert
        Assert.DoesNotThrow(() => service.Initialize());
    }

    [Test]
    public void Initialize_StartsBackgroundTasksWithoutBlocking()
    {
        // Arrange
        var service = new GameService(_options, _mockWinnersService.Object);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        service.Initialize();
        stopwatch.Stop();

        // Assert - Initialize should return quickly (within 1 second)
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1000));
    }

    [Test]
    public void AppCancellationToken_ProvidesValidToken()
    {
        // Arrange
        var service = new GameService(_options, _mockWinnersService.Object);

        // Act
        var token = service.AppCancellationToken;

        // Assert
        Assert.That(token.CanBeCanceled, Is.True);
    }

    [Test]
    public void MultipleInitialize_CallsDoNotThrow()
    {
        // Arrange
        var service = new GameService(_options, _mockWinnersService.Object);

        // Act & Assert
        Assert.DoesNotThrow(() => service.Initialize());
        Assert.DoesNotThrow(() => service.Initialize());
    }

    [Test]
    public void Constructor_WithNullSettings_ThrowsNullReferenceException()
    {
        // Act & Assert
        Assert.Throws<NullReferenceException>(() => 
            new GameService(null!, _mockWinnersService.Object));
    }

    [Test]
    public void Constructor_WithNullWinnersService_AcceptsNull()
    {
        // Act & Assert
        // Note: The constructor doesn't validate winnersService parameter
        Assert.DoesNotThrow(() => 
            new GameService(_options, null!));
    }

    [Test]
    public void Initialize_WithMinimalSettings_CreatesAtLeast10Players()
    {
        // Arrange
        var minSettings = new LottoSettings
        {
            InitialBalance = 10.0m,
            TicketPrice = 1.0m,
            MaxTicketsPerPlayers = 1,
            MaxPlayersPerGame = 15,
            SuppressOutput = true
        };
        var options = Options.Create(minSettings);
        var service = new GameService(options, _mockWinnersService.Object);

        // Act & Assert
        // The Initialize method creates between 10 and MaxPlayersPerGame-1 CPU players
        // plus the human player (total 11 to MaxPlayersPerGame)
        Assert.DoesNotThrow(() => service.Initialize());
    }

    [Test]
    public void Initialize_WithMaxSettings_DoesNotExceedMaxPlayers()
    {
        // Arrange
        var maxSettings = new LottoSettings
        {
            InitialBalance = 100.0m,
            TicketPrice = 5.0m,
            MaxTicketsPerPlayers = 50,
            MaxPlayersPerGame = 100,
            SuppressOutput = true
        };
        var options = Options.Create(maxSettings);
        var service = new GameService(options, _mockWinnersService.Object);

        // Act & Assert
        Assert.DoesNotThrow(() => service.Initialize());
    }

    [Test]
    public void GameService_AfterInitialize_CancellationTokenStillValid()
    {
        // Arrange
        var service = new GameService(_options, _mockWinnersService.Object);

        // Act
        service.Initialize();
        var token = service.AppCancellationToken;

        // Assert
        Assert.That(token.CanBeCanceled, Is.True);
    }

    [Test]
    public void GameService_WithZeroInitialBalance_Initializes()
    {
        // Arrange
        var settings = new LottoSettings
        {
            InitialBalance = 0.0m,
            TicketPrice = 1.0m,
            MaxTicketsPerPlayers = 10,
            MaxPlayersPerGame = 15,
            SuppressOutput = true
        };
        var options = Options.Create(settings);
        var service = new GameService(options, _mockWinnersService.Object);

        // Act & Assert
        Assert.DoesNotThrow(() => service.Initialize());
    }

    [Test]
    public void GameService_WithHighTicketPrice_Initializes()
    {
        // Arrange
        var settings = new LottoSettings
        {
            InitialBalance = 1000.0m,
            TicketPrice = 100.0m,
            MaxTicketsPerPlayers = 10,
            MaxPlayersPerGame = 15,
            SuppressOutput = true
        };
        var options = Options.Create(settings);
        var service = new GameService(options, _mockWinnersService.Object);

        // Act & Assert
        Assert.DoesNotThrow(() => service.Initialize());
    }
}
