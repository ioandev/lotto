using BedeLotteryConsole.IO;
using BedeLotteryConsole.Models;
using Microsoft.Extensions.Options;
using BedeLotteryConsole.Settings;

namespace BedeLotteryConsole.Tests.IO;

[TestFixture]
public class OutputProcessorTests
{

    [Test]
    public async Task Process_InitialState_WaitingForBet_DefaultSettings()
    {
        // Arrange
        var settings = new LottoSettings
        {
            InitialBalance = 10.0m,
            TicketPrice = 1.0m,
            Currency = "$",
            MaxTicketsPerPlayers = 10,
            MaxPlayersPerGame = 15
        };
        var options = Options.Create(settings);
        var processor = new OutputProcessor(options);
        processor.Initialize();

        var gameState = new GameState
        {
            PlayerBalances = new Dictionary<int, decimal> { { 1, 10.0m } },
            LastRoundResults = null,
            StatusType = StatusType.WaitingForBet,
            Player1TicketsAmount = 0
        };

        // Act
        var result = await processor.Process(gameState);

        // Assert
        await Verify(result)
            .UseDirectory("Snapshots")
            .UseMethodName("InitialState_WaitingForBet_DefaultSettings");
    }

    [Test]
    public async Task Process_InitialState_WaitingForBet_EuroSettings()
    {
        // Arrange
        var settings = new LottoSettings
        {
            InitialBalance = 50.0m,
            TicketPrice = 2.5m,
            Currency = "€",
            MaxTicketsPerPlayers = 5,
            MaxPlayersPerGame = 10
        };
        var options = Options.Create(settings);
        var processor = new OutputProcessor(options);
        processor.Initialize();

        var gameState = new GameState
        {
            PlayerBalances = new Dictionary<int, decimal> { { 1, 50.0m } },
            LastRoundResults = null,
            StatusType = StatusType.WaitingForBet,
            Player1TicketsAmount = 0
        };

        // Act
        var result = await processor.Process(gameState);

        // Assert
        await Verify(result)
            .UseDirectory("Snapshots")
            .UseMethodName("InitialState_WaitingForBet_EuroSettings");
    }

    [Test]
    public async Task Process_AfterRound_Player1WinsGrandPrize()
    {
        // Arrange
        var settings = new LottoSettings
        {
            InitialBalance = 10.0m,
            TicketPrice = 1.0m,
            Currency = "$",
            MaxTicketsPerPlayers = 10,
            MaxPlayersPerGame = 15
        };
        var options = Options.Create(settings);
        var processor = new OutputProcessor(options);
        processor.Initialize();

        var gameState = new GameState
        {
            PlayerBalances = new Dictionary<int, decimal> { {1, 65.0m}, {2, 5.0m}, {3, 5.0m}, {4, 5.0m}, {5, 5.0m}, {6, 5.0m} },
            LastRoundResults = new RoundResults
            {
                PrizeResults = new List<PrizeResult>
                {
                    new() { PlayerId = 1, PrizeType = PrizeType.Grand, PrizeAmount = 60.0m, Tickets = 3 }
                },
                HouseProfit = 0.0m
            },
            StatusType = StatusType.ShowingResults,
            Player1TicketsAmount = 3
        };

        // Act
        var result = await processor.Process(gameState);

        // Assert
        await Verify(result)
            .UseDirectory("Snapshots")
            .UseMethodName("AfterRound_Player1WinsGrandPrize");
    }

    [Test]
    public async Task Process_AfterRound_Player1WinsSecondTier()
    {
        // Arrange
        var settings = new LottoSettings
        {
            InitialBalance = 10.0m,
            TicketPrice = 1.0m,
            Currency = "$",
            MaxTicketsPerPlayers = 10,
            MaxPlayersPerGame = 15
        };
        var options = Options.Create(settings);
        var processor = new OutputProcessor(options);
        processor.Initialize();

        var gameState = new GameState
        {
            PlayerBalances = new Dictionary<int, decimal> { {1, 19.0m}, {2, 5.0m}, {3, 5.0m}, {4, 5.0m}, {5, 5.0m}, {6, 5.0m}, {7, 5.0m}, {8, 5.0m}, {9, 5.0m} },
            LastRoundResults = new RoundResults
            {
                PrizeResults = new List<PrizeResult>
                {
                    new() { PlayerId = 5, PrizeType = PrizeType.Grand, PrizeAmount = 45.0m, Tickets = 2 },
                    new() { PlayerId = 1, PrizeType = PrizeType.SecondTier, PrizeAmount = 5.0m, Tickets = 2 },
                    new() { PlayerId = 3, PrizeType = PrizeType.SecondTier, PrizeAmount = 5.0m, Tickets = 1 }
                },
                HouseProfit = 15.0m
            },
            StatusType = StatusType.ShowingResults,
            Player1TicketsAmount = 2
        };

        // Act
        var result = await processor.Process(gameState);

        // Assert
        await Verify(result)
            .UseDirectory("Snapshots")
            .UseMethodName("AfterRound_Player1WinsSecondTier");
    }

    [Test]
    public async Task Process_AfterRound_Player1WinsThirdTier()
    {
        // Arrange
        var settings = new LottoSettings
        {
            InitialBalance = 10.0m,
            TicketPrice = 1.0m,
            Currency = "$",
            MaxTicketsPerPlayers = 10,
            MaxPlayersPerGame = 15
        };
        var options = Options.Create(settings);
        var processor = new OutputProcessor(options);
        processor.Initialize();

        var gameState = new GameState
        {
            PlayerBalances = new Dictionary<int, decimal> { {1, 11.5m}, {2, 5.0m}, {3, 5.0m}, {4, 5.0m}, {5, 5.0m}, {6, 5.0m}, {7, 5.0m}, {8, 5.0m}, {9, 5.0m}, {10, 5.0m}, {11, 5.0m} },
            LastRoundResults = new RoundResults
            {
                PrizeResults = new List<PrizeResult>
                {
                    new() { PlayerId = 7, PrizeType = PrizeType.Grand, PrizeAmount = 50.0m, Tickets = 4 },
                    new() { PlayerId = 4, PrizeType = PrizeType.SecondTier, PrizeAmount = 8.0m, Tickets = 2 },
                    new() { PlayerId = 1, PrizeType = PrizeType.ThirdTier, PrizeAmount = 2.5m, Tickets = 1 },
                    new() { PlayerId = 9, PrizeType = PrizeType.ThirdTier, PrizeAmount = 2.5m, Tickets = 2 }
                },
                HouseProfit = 25.0m
            },
            StatusType = StatusType.ShowingResults,
            Player1TicketsAmount = 1
        };

        // Act
        var result = await processor.Process(gameState);

        // Assert
        await Verify(result)
            .UseDirectory("Snapshots")
            .UseMethodName("AfterRound_Player1WinsThirdTier");
    }

    [Test]
    public async Task Process_AfterRound_Player1Loses_MultipleWinners()
    {
        // Arrange
        var settings = new LottoSettings
        {
            InitialBalance = 10.0m,
            TicketPrice = 1.0m,
            Currency = "$",
            MaxTicketsPerPlayers = 10,
            MaxPlayersPerGame = 15
        };
        var options = Options.Create(settings);
        var processor = new OutputProcessor(options);
        processor.Initialize();

        var gameState = new GameState
        {
            PlayerBalances = new Dictionary<int, decimal> { {1, 5.0m}, {2, 5.0m}, {3, 5.0m}, {4, 5.0m}, {5, 5.0m}, {6, 5.0m}, {7, 5.0m}, {8, 5.0m}, {9, 5.0m}, {10, 5.0m}, {11, 5.0m}, {12, 5.0m}, {13, 5.0m} },
            LastRoundResults = new RoundResults
            {
                PrizeResults = new List<PrizeResult>
                {
                    new() { PlayerId = 8, PrizeType = PrizeType.Grand, PrizeAmount = 72.0m, Tickets = 5 },
                    new() { PlayerId = 3, PrizeType = PrizeType.SecondTier, PrizeAmount = 10.0m, Tickets = 3 },
                    new() { PlayerId = 6, PrizeType = PrizeType.SecondTier, PrizeAmount = 10.0m, Tickets = 2 },
                    new() { PlayerId = 11, PrizeType = PrizeType.SecondTier, PrizeAmount = 10.0m, Tickets = 1 },
                    new() { PlayerId = 2, PrizeType = PrizeType.ThirdTier, PrizeAmount = 3.0m, Tickets = 2 },
                    new() { PlayerId = 4, PrizeType = PrizeType.ThirdTier, PrizeAmount = 3.0m, Tickets = 1 },
                    new() { PlayerId = 9, PrizeType = PrizeType.ThirdTier, PrizeAmount = 3.0m, Tickets = 3 }
                },
                HouseProfit = 45.0m
            },
            StatusType = StatusType.ShowingResults,
            Player1TicketsAmount = 5
        };

        // Act
        var result = await processor.Process(gameState);

        // Assert
        await Verify(result)
            .UseDirectory("Snapshots")
            .UseMethodName("AfterRound_Player1Loses_MultipleWinners");
    }

    [Test]
    public async Task Process_AfterRound_OnlyGrandPrizeWinner_HigherTicketPrice()
    {
        // Arrange
        var settings = new LottoSettings
        {
            InitialBalance = 100.0m,
            TicketPrice = 5.0m,
            Currency = "£",
            MaxTicketsPerPlayers = 20,
            MaxPlayersPerGame = 20
        };
        var options = Options.Create(settings);
        var processor = new OutputProcessor(options);
        processor.Initialize();

        var gameState = new GameState
        {
            PlayerBalances = new Dictionary<int, decimal> { {1, 75.0m}, {2, 75.0m}, {3, 75.0m}, {4, 75.0m}, {5, 75.0m}, {6, 75.0m}, {7, 75.0m}, {8, 75.0m}, {9, 75.0m}, {10, 75.0m}, {11, 75.0m}, {12, 75.0m}, {13, 75.0m}, {14, 75.0m}, {15, 75.0m}, {16, 75.0m} },
            LastRoundResults = new RoundResults
            {
                PrizeResults = new List<PrizeResult>
                {
                    new() { PlayerId = 12, PrizeType = PrizeType.Grand, PrizeAmount = 200.0m, Tickets = 8 }
                },
                HouseProfit = 50.0m
            },
            StatusType = StatusType.ShowingResults,
            Player1TicketsAmount = 5
        };

        // Act
        var result = await processor.Process(gameState);

        // Assert
        await Verify(result)
            .UseDirectory("Snapshots")
            .UseMethodName("AfterRound_OnlyGrandPrize_HigherTicketPrice");
    }

    [Test]
    public async Task Process_AfterRound_AllTierWinners_WithDecimalAmounts()
    {
        // Arrange
        var settings = new LottoSettings
        {
            InitialBalance = 20.0m,
            TicketPrice = 1.5m,
            Currency = "$",
            MaxTicketsPerPlayers = 8,
            MaxPlayersPerGame = 12
        };
        var options = Options.Create(settings);
        var processor = new OutputProcessor(options);
        processor.Initialize();

        var gameState = new GameState
        {
            PlayerBalances = new Dictionary<int, decimal> { {1, 31.25m}, {2, 15.0m}, {3, 15.0m}, {4, 15.0m}, {5, 15.0m}, {6, 15.0m}, {7, 15.0m}, {8, 15.0m} },
            LastRoundResults = new RoundResults
            {
                PrizeResults = new List<PrizeResult>
                {
                    new() { PlayerId = 1, PrizeType = PrizeType.Grand, PrizeAmount = 18.75m, Tickets = 2 },
                    new() { PlayerId = 5, PrizeType = PrizeType.SecondTier, PrizeAmount = 4.25m, Tickets = 1 },
                    new() { PlayerId = 3, PrizeType = PrizeType.ThirdTier, PrizeAmount = 1.75m, Tickets = 3 }
                },
                HouseProfit = 12.5m
            },
            StatusType = StatusType.ShowingResults,
            Player1TicketsAmount = 2
        };

        // Act
        var result = await processor.Process(gameState);

        // Assert
        await Verify(result)
            .UseDirectory("Snapshots")
            .UseMethodName("AfterRound_AllTierWinners_DecimalAmounts");
    }

    [Test]
    public void Process_ThrowsException_WhenNotInitialized()
    {
        // Arrange
        var settings = new LottoSettings
        {
            InitialBalance = 10.0m,
            TicketPrice = 1.0m,
            Currency = "$"
        };
        var options = Options.Create(settings);
        var processor = new OutputProcessor(options);
        // Note: NOT calling Initialize()

        var gameState = new GameState
        {
            PlayerBalances = new Dictionary<int, decimal> { { 1, 10.0m } },
            LastRoundResults = null,
            StatusType = StatusType.WaitingForBet,
            Player1TicketsAmount = 0
        };

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await processor.Process(gameState);
        });

        Assert.That(ex.Message, Is.EqualTo("Template is not initialized."));
    }

    [Test]
    public async Task Process_AfterRound_LowBalance_WaitingForBet()
    {
        // Arrange
        var settings = new LottoSettings
        {
            InitialBalance = 10.0m,
            TicketPrice = 1.0m,
            Currency = "$",
            MaxTicketsPerPlayers = 10,
            MaxPlayersPerGame = 15
        };
        var options = Options.Create(settings);
        var processor = new OutputProcessor(options);
        processor.Initialize();

        var gameState = new GameState
        {
            PlayerBalances = new Dictionary<int, decimal> { {1, 2.5m}, {2, 2.5m}, {3, 2.5m}, {4, 2.5m}, {5, 2.5m}, {6, 2.5m}, {7, 2.5m}, {8, 2.5m}, {9, 2.5m}, {10, 2.5m}, {11, 2.5m} },
            LastRoundResults = new RoundResults
            {
                PrizeResults = new List<PrizeResult>
                {
                    new() { PlayerId = 7, PrizeType = PrizeType.Grand, PrizeAmount = 80.0m, Tickets = 6 },
                    new() { PlayerId = 2, PrizeType = PrizeType.SecondTier, PrizeAmount = 12.0m, Tickets = 2 }
                },
                HouseProfit = 35.0m
            },
            StatusType = StatusType.WaitingForBet,
            Player1TicketsAmount = 0
        };

        // Act
        var result = await processor.Process(gameState);

        // Assert
        await Verify(result)
            .UseDirectory("Snapshots")
            .UseMethodName("AfterRound_LowBalance_WaitingForBet");
    }
}
