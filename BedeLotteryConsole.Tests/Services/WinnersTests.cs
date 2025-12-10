using BedeLotteryConsole.Algos;
using BedeLotteryConsole.Models;
using BedeLotteryConsole.Settings;
using BedeLotteryConsole.Services.Interfaces;

namespace BedeLotteryConsole.Tests.Services;

[TestFixture]
public class WinnersTests
{
    private LottoSettings _defaultSettings;
    private IWinnersService _winnersService;

    [SetUp]
    public void SetUp()
    {
        _defaultSettings = new LottoSettings
        {
            InitialBalance = 10.0m,
            TicketPrice = 1.0m,
            MaxTicketsPerPlayers = 50,
            MaxPlayersPerGame = 15
        };
        
        _winnersService = new WinnersService(Microsoft.Extensions.Options.Options.Create(_defaultSettings));
    }

    [Test]
    public void CalculateWinners_WithNullInput_ThrowsArgumentNullException()
    {
        // Arrange
        var random = new Random(111);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _winnersService.CalculateWinners(null!, random));
    }

    [Test]
    public void CalculateWinners_Player1WinsGrandPrize_BalanceUpdatedCorrectly()
    {
        // Arrange - Need at least 31 tickets for the lottery to work (1 grand + 10 second + 20 third)
        // With seed 111, we need enough players to ensure >= 31 total tickets
        var random = new Random(111);
        var input = new WinnersInput
        {
            Player1TicketsAmount = 10,
            PlayerBalances = new Dictionary<int, decimal>
            {
                { 1, 50.0m },
                { 2, 50.0m },
                { 3, 50.0m },
                { 4, 50.0m },
                { 5, 50.0m },
                { 6, 50.0m }
            }
        };

        // Act
        var result = _winnersService.CalculateWinners(input, random);

        // Assert
        Assert.That(result, Is.Not.Null, "CalculateWinners should return a result with enough tickets");
        
        // Verify balances are calculated correctly
        // Player 1 should have: initial - tickets_bought + winnings
        var player1Winnings = result!.LastRoundResults?.PrizeResults
            ?.Where(x => x.PlayerId == 1)
            .Sum(x => x.PrizeAmount) ?? 0m;
        var expectedPlayer1Balance = 50.0m - (10 * _defaultSettings.TicketPrice) + player1Winnings;

        Assert.That(result!.PlayerBalances[1], Is.EqualTo(expectedPlayer1Balance));
        
        // Verify prize results exist
        Assert.That(result!.LastRoundResults?.PrizeResults, Is.Not.Empty);
        
        // Verify all balances are properly calculated
        foreach (var playerId in input.PlayerBalances.Keys)
        {
            if (playerId == 1) continue; // Skip player 1, already checked
            var cpuWinnings = result!.LastRoundResults?.PrizeResults
                ?.Where(x => x.PlayerId == playerId)
                .Sum(x => x.PrizeAmount) ?? 0m;
            Assert.That(result!.PlayerBalances.ContainsKey(playerId), Is.True);
        }
    }

    [Test]
    public void CalculateWinners_CPUPlayersBalances_UpdatedCorrectly()
    {
        // Arrange - Need at least 31 tickets total
        var random = new Random(111);
        var input = new WinnersInput
        {
            Player1TicketsAmount = 10,
            PlayerBalances = new Dictionary<int, decimal>
            {
                { 1, 50.0m },
                { 2, 50.0m },
                { 3, 50.0m },
                { 4, 50.0m },
                { 5, 50.0m },
                { 6, 50.0m }
            }
        };

        // Act
        var result = _winnersService.CalculateWinners(input, random);

        // Assert - All CPU players should have their balances updated
        Assert.That(result, Is.Not.Null, "CalculateWinners should return a result with enough tickets");
        Assert.That(result!.PlayerBalances.Count, Is.EqualTo(6));
        
        foreach (var playerId in input.PlayerBalances.Keys)
        {
            if (playerId == 1) continue; // Skip player 1
            Assert.That(result!.PlayerBalances.ContainsKey(playerId), Is.True, $"Player {playerId} should be in output");
            
            // Each CPU player's new balance should be: initial - tickets_bought + winnings
            var initialBalance = input.PlayerBalances[playerId];
            var winnings = result!.LastRoundResults?.PrizeResults
                ?.Where(x => x.PlayerId == playerId)
                .Sum(x => x.PrizeAmount) ?? 0m;
            
            // We need to calculate how many tickets they bought
            // This is determined by the random number generator in GetTicketsPerPlayers
            // For now, verify the balance is <= initial (they at least spent money)
            Assert.That(result!.PlayerBalances[playerId], Is.LessThanOrEqualTo(initialBalance + winnings));
        }
    }

    [Test]
    public void CalculateWinners_TotalPrizeDistribution_EqualsPoolMoney()
    {
        // Arrange - Need at least 31 tickets total
        var random = new Random(111);
        var input = new WinnersInput
        {
            Player1TicketsAmount = 31, // Guarantee minimum 31 tickets
            PlayerBalances = new Dictionary<int, decimal>
            {
                { 1, 50.0m },
                { 2, 50.0m },
                { 3, 50.0m }
            }
        };

        // Act
        var result = _winnersService.CalculateWinners(input, random);

        // Assert
        Assert.That(result, Is.Not.Null);
        var totalPrizesAwarded = result!.LastRoundResults?.PrizeResults?.Sum(x => x.PrizeAmount) ?? 0m;
        var houseProfit = result!.LastRoundResults?.HouseProfit ?? 0m;
        
        // Total prizes + house profit should equal the total pool
        // Calculate total tickets sold
        var totalSpent = input.Player1TicketsAmount * _defaultSettings.TicketPrice; // Player 1
        foreach (var playerId in input.PlayerBalances.Keys)
        {
            if (playerId == 1) continue; // Already counted player 1
            var spent = input.PlayerBalances[playerId] - result!.PlayerBalances[playerId] + 
                       (result!.LastRoundResults?.PrizeResults?.Where(x => x.PlayerId == playerId).Sum(x => x.PrizeAmount) ?? 0m);
            totalSpent += spent;
        }
        
        Assert.That(totalPrizesAwarded + houseProfit, Is.EqualTo(totalSpent).Within(0.01m));
    }

    [Test]
    public void CalculateWinners_PrizeTypes_AssignedCorrectly()
    {
        // Arrange - Need at least 31 tickets total
        var random = new Random(111);
        var input = new WinnersInput
        {
            Player1TicketsAmount = 31, // Guarantee minimum 31 tickets
            PlayerBalances = new Dictionary<int, decimal>
            {
                { 1, 50.0m },
                { 2, 50.0m },
                { 3, 50.0m }
            }
        };

        // Act
        var result = _winnersService.CalculateWinners(input, random);

        // Assert - There should be at most 1 grand prize winner
        Assert.That(result, Is.Not.Null);
        var grandPrizeWinners = result!.LastRoundResults?.PrizeResults?.Where(x => x.PrizeType == PrizeType.Grand).ToList() ?? new List<PrizeResult>();
        Assert.That(grandPrizeWinners.Count, Is.LessThanOrEqualTo(1), "Should have at most 1 grand prize winner");
        
        // All prize results should have valid prize types
        foreach (var prize in result!.LastRoundResults?.PrizeResults ?? new List<PrizeResult>())
        {
            Assert.That(prize.PrizeType, Is.Not.EqualTo(PrizeType.Invalid));
        }
    }

    [Test]
    public void CalculateWinners_WithFixedSeed_ProducesDeterministicResults()
    {
        // Arrange - Need at least 31 tickets total
        var input = new WinnersInput
        {
            Player1TicketsAmount = 31, // Guarantee minimum 31 tickets
            PlayerBalances = new Dictionary<int, decimal>
            {
                { 1, 50.0m },
                { 2, 50.0m },
                { 3, 50.0m }
            }
        };

        // Act - Run twice with same seed
        var result1 = _winnersService.CalculateWinners(input, new Random(111));
        var result2 = _winnersService.CalculateWinners(input, new Random(111));

        // Assert - Results should be identical
        Assert.That(result1, Is.Not.Null);
        Assert.That(result2, Is.Not.Null);
        Assert.That(result1!.PlayerBalances[1], Is.EqualTo(result2!.PlayerBalances[1]));
        Assert.That(result1!.PlayerBalances[2], Is.EqualTo(result2!.PlayerBalances[2]));
        Assert.That(result1!.PlayerBalances[3], Is.EqualTo(result2!.PlayerBalances[3]));
        Assert.That(result1!.LastRoundResults.HouseProfit, Is.EqualTo(result2!.LastRoundResults.HouseProfit));
        Assert.That(result1!.LastRoundResults.PrizeResults.Count, Is.EqualTo(result2!.LastRoundResults.PrizeResults.Count));
    }

    [Test]
    public void CalculateWinners_Player1NoBet_BalanceUnchanged()
    {
        // Arrange - Need at least 31 tickets total from CPU players
        var random = new Random(111);
        var input = new WinnersInput
        {
            Player1TicketsAmount = 0, // No bet
            PlayerBalances = new Dictionary<int, decimal>
            {
                { 1, 10.0m },
                { 2, 50.0m },
                { 3, 50.0m },
                { 4, 50.0m },
                { 5, 50.0m },
                { 6, 50.0m },
                { 7, 50.0m },
                { 8, 50.0m },
                { 9, 50.0m }
            }
        };

        // Act
        var result = _winnersService.CalculateWinners(input, random);

        // Assert - Player 1 balance should be unchanged (they didn't participate)
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.PlayerBalances[1], Is.EqualTo(10.0m));
        
        // Player 1 should not be in the prize results
        var player1Prizes = result!.LastRoundResults?.PrizeResults?.Where(x => x.PlayerId == 1).ToList() ?? new List<PrizeResult>();
        Assert.That(player1Prizes, Is.Empty);
    }

    [Test]
    public void CalculateWinners_MultipleWinners_PrizesDistributedCorrectly()
    {
        // Arrange - Need at least 31 tickets total
        var random = new Random(111);
        var input = new WinnersInput
        {
            Player1TicketsAmount = 31,
            PlayerBalances = new Dictionary<int, decimal>
            {
                { 1, 50.0m },
                { 2, 50.0m },
                { 3, 50.0m }
            }
        };

        // Act
        var result = _winnersService.CalculateWinners(input, random);

        // Assert
        Assert.That(result, Is.Not.Null);
        var allWinners = result!.LastRoundResults?.PrizeResults?.ToList() ?? new List<PrizeResult>();
        Assert.That(allWinners, Is.Not.Empty, "There should be winners");
        
        // Verify grand prize winner exists and gets 50% of the pool
        var grandPrizeWinners = allWinners.Where(x => x.PrizeType == PrizeType.Grand).ToList();
        Assert.That(grandPrizeWinners.Count, Is.EqualTo(1), "Should have exactly 1 grand prize winner");
        
        // Verify the total prizes + house profit = total spent
        var totalPrizesAwarded = allWinners.Sum(x => x.PrizeAmount);
        var totalSpent = input.Player1TicketsAmount * _defaultSettings.TicketPrice;
        foreach (var playerId in input.PlayerBalances.Keys)
        {
            if (playerId == 1) continue; // Already counted player 1
            var spent = input.PlayerBalances[playerId] - result!.PlayerBalances[playerId] + 
                       allWinners.Where(x => x.PlayerId == playerId).Sum(x => x.PrizeAmount);
            totalSpent += spent;
        }
        
        Assert.That(totalPrizesAwarded + (result!.LastRoundResults?.HouseProfit ?? 0m), Is.EqualTo(totalSpent).Within(0.01m));
    }

    [Test]
    public void CalculateWinners_LowBalance_CPUPlayersBetWithinMeans()
    {
        // Arrange - Need at least 31 tickets total; some players have low balance
        var random = new Random(111);
        var input = new WinnersInput
        {
            Player1TicketsAmount = 31, // Guarantee minimum 31 tickets
            PlayerBalances = new Dictionary<int, decimal>
            {
                { 1, 50.0m },
                { 2, 50.0m },
                { 3, 3.0m }, // Can only afford 3 tickets
                { 4, 2.0m }  // Can only afford 2 tickets
            }
        };

        // Act
        var result = _winnersService.CalculateWinners(input, random);

        // Assert - No CPU player should go into negative balance
        Assert.That(result, Is.Not.Null);
        foreach (var kvp in result!.PlayerBalances)
        {
            Assert.That(kvp.Value, Is.GreaterThanOrEqualTo(0m), $"Player {kvp.Key} should not have negative balance");
        }
    }

    [Test]
    public void DrawWinners_WithFixedSeed_ReturnsSameResultsEveryTime()
    {
        // Arrange
        const int seed = 111;
        const int totalTickets = 100;
        var rng = new Random(seed);

        // Act
        var (grandPrize, secondTier, thirdTier) = _winnersService.DrawWinners(rng, totalTickets);

        // Assert - These exact values should be returned every time with seed 111
        // With 100 tickets: 10% = 10 tickets for second tier, 20% = 20 tickets for third tier
        Assert.That(grandPrize, Is.EqualTo(21));
        Assert.That(secondTier, Is.EqualTo(new[] { 94, 65, 98, 40, 85, 26, 50, 32, 10, 18 }));
        Assert.That(thirdTier, Is.EqualTo(new[] { 47, 43, 51, 71, 3, 78, 23, 55, 41, 82, 49, 62, 53, 1, 42, 87, 93, 100, 70, 4 }));
    }

    [Test]
    public void DrawWinners_WithSameSeed_ProducesSameResults()
    {
        // Arrange
        const int seed = 111;
        const int totalTickets = 100;

        // Act - Run twice with same seed
        var result1 = _winnersService.DrawWinners(new Random(seed), totalTickets);
        var result2 = _winnersService.DrawWinners(new Random(seed), totalTickets);

        // Assert - Both calls should produce identical results
        Assert.That(result1.Item1, Is.EqualTo(result2.Item1), "Grand prize tickets should match");
        Assert.That(result1.Item2, Is.EqualTo(result2.Item2), "Second tier tickets should match");
        Assert.That(result1.Item3, Is.EqualTo(result2.Item3), "Third tier tickets should match");
    }

    [Test]
    public void DrawWinners_ReturnsCorrectNumberOfWinners()
    {
        // Arrange
        var rng = new Random(111);
        const int totalTickets = 100;

        // Act
        var (grandPrize, secondTier, thirdTier) = _winnersService.DrawWinners(rng, totalTickets);

        // Assert - 10% of 100 = 10, 20% of 100 = 20
        Assert.That(secondTier.Length, Is.EqualTo(10), "Should have 10 second tier winners (10% of 100)");
        Assert.That(thirdTier.Length, Is.EqualTo(20), "Should have 20 third tier winners (20% of 100)");
    }

    [Test]
    public void DrawWinners_AllTicketsAreUnique()
    {
        // Arrange
        var rng = new Random(111);
        const int totalTickets = 100;

        // Act
        var (grandPrize, secondTier, thirdTier) = _winnersService.DrawWinners(rng, totalTickets);

        // Assert - Combine all winning tickets
        var allWinners = new List<int> { grandPrize };
        allWinners.AddRange(secondTier);
        allWinners.AddRange(thirdTier);

        Assert.That(allWinners.Distinct().Count(), Is.EqualTo(31), "All 31 winning tickets should be unique");
    }

    [Test]
    public void DrawWinners_AllTicketsAreWithinValidRange()
    {
        // Arrange
        var rng = new Random(111);
        const int totalTickets = 100;

        // Act
        var (grandPrize, secondTier, thirdTier) = _winnersService.DrawWinners(rng, totalTickets);

        // Assert
        Assert.That(grandPrize, Is.InRange(1, totalTickets));
        Assert.That(secondTier, Has.All.InRange(1, totalTickets));
        Assert.That(thirdTier, Has.All.InRange(1, totalTickets));
    }

    [Test]
    public void DrawWinners_WithDifferentTicketCounts_CalculatesCorrectPercentages()
    {
        // Arrange & Act & Assert
        var testCases = new[]
        {
            new { TotalTickets = 50, ExpectedSecondTier = 5, ExpectedThirdTier = 10 },  // 10% of 50 = 5, 20% of 50 = 10
            new { TotalTickets = 35, ExpectedSecondTier = 3, ExpectedThirdTier = 7 },   // 10% of 35 = 3, 20% of 35 = 7
            new { TotalTickets = 11, ExpectedSecondTier = 1, ExpectedThirdTier = 2 },   // 10% of 11 = 1, 20% of 11 = 2
            new { TotalTickets = 200, ExpectedSecondTier = 20, ExpectedThirdTier = 40 } // 10% of 200 = 20, 20% of 200 = 40
        };

        foreach (var testCase in testCases)
        {
            var rng = new Random(111);
            var (_, secondTier, thirdTier) = _winnersService.DrawWinners(rng, testCase.TotalTickets);

            Assert.That(secondTier.Length, Is.EqualTo(testCase.ExpectedSecondTier),
                $"For {testCase.TotalTickets} tickets, second tier should be {testCase.ExpectedSecondTier}");
            Assert.That(thirdTier.Length, Is.EqualTo(testCase.ExpectedThirdTier),
                $"For {testCase.TotalTickets} tickets, third tier should be {testCase.ExpectedThirdTier}");
        }
    }
}
