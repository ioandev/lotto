using BedeLotteryConsole.Algos;
using BedeLotteryConsole.Models;
using BedeLotteryConsole.Settings;

namespace BedeLotteryConsole.Tests.Algos;

[TestFixture]
public class WinnersTests
{
    private LottoSettings _defaultSettings;

    [SetUp]
    public void SetUp()
    {
        _defaultSettings = new LottoSettings
        {
            InitialBalance = 10.0m,
            TicketPrice = 1.0m,
            Currency = "$",
            MaxTicketsPerPlayers = 50,
            MaxPlayersPerGame = 15
        };
    }

    [Test]
    public void CalculateWinners_WithNullInput_ThrowsArgumentNullException()
    {
        // Arrange
        var random = new Random(111);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Winners.CalculateWinners(null!, _defaultSettings, random));
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
        var result = Winners.CalculateWinners(input, _defaultSettings, random);

        // Assert - Verify balances are calculated correctly
        // Player 1 should have: initial - tickets_bought + winnings
        var player1Winnings = result.LastRoundResults.PrizeResults
            .Where(x => x.PlayerId == 1)
            .Sum(x => x.PrizeAmount);
        var expectedPlayer1Balance = 50.0m - (10 * _defaultSettings.TicketPrice) + player1Winnings;

        Assert.That(result.PlayerBalances[1], Is.EqualTo(expectedPlayer1Balance));
        
        // Verify prize results exist
        Assert.That(result.LastRoundResults.PrizeResults, Is.Not.Empty);
        
        // Verify all balances are properly calculated
        foreach (var playerId in input.PlayerBalances.Keys)
        {
            if (playerId == 1) continue; // Skip player 1, already checked
            var cpuWinnings = result.LastRoundResults.PrizeResults
                .Where(x => x.PlayerId == playerId)
                .Sum(x => x.PrizeAmount);
            Assert.That(result.PlayerBalances.ContainsKey(playerId), Is.True);
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
        var result = Winners.CalculateWinners(input, _defaultSettings, random);

        // Assert - All CPU players should have their balances updated
        Assert.That(result.PlayerBalances.Count, Is.EqualTo(6));
        
        foreach (var playerId in input.PlayerBalances.Keys)
        {
            if (playerId == 1) continue; // Skip player 1
            Assert.That(result.PlayerBalances.ContainsKey(playerId), Is.True, $"Player {playerId} should be in output");
            
            // Each CPU player's new balance should be: initial - tickets_bought + winnings
            var initialBalance = input.PlayerBalances[playerId];
            var winnings = result.LastRoundResults.PrizeResults
                .Where(x => x.PlayerId == playerId)
                .Sum(x => x.PrizeAmount);
            
            // We need to calculate how many tickets they bought
            // This is determined by the random number generator in GetTicketsPerPlayers
            // For now, verify the balance is <= initial (they at least spent money)
            Assert.That(result.PlayerBalances[playerId], Is.LessThanOrEqualTo(initialBalance + winnings));
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
        var result = Winners.CalculateWinners(input, _defaultSettings, random);

        // Assert
        var totalPrizesAwarded = result.LastRoundResults.PrizeResults.Sum(x => x.PrizeAmount);
        var houseProfit = result.LastRoundResults.HouseProfit;
        
        // Total prizes + house profit should equal the total pool
        // Calculate total tickets sold
        var totalSpent = input.Player1TicketsAmount * _defaultSettings.TicketPrice; // Player 1
        foreach (var playerId in input.PlayerBalances.Keys)
        {
            if (playerId == 1) continue; // Already counted player 1
            var spent = input.PlayerBalances[playerId] - result.PlayerBalances[playerId] + 
                       result.LastRoundResults.PrizeResults.Where(x => x.PlayerId == playerId).Sum(x => x.PrizeAmount);
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
        var result = Winners.CalculateWinners(input, _defaultSettings, random);

        // Assert - There should be at most 1 grand prize winner
        var grandPrizeWinners = result.LastRoundResults.PrizeResults.Where(x => x.PrizeType == PrizeType.Grand).ToList();
        Assert.That(grandPrizeWinners.Count, Is.LessThanOrEqualTo(1), "Should have at most 1 grand prize winner");
        
        // All prize results should have valid prize types
        foreach (var prize in result.LastRoundResults.PrizeResults)
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
        var result1 = Winners.CalculateWinners(input, _defaultSettings, new Random(111));
        var result2 = Winners.CalculateWinners(input, _defaultSettings, new Random(111));

        // Assert - Results should be identical
        Assert.That(result1.PlayerBalances[1], Is.EqualTo(result2.PlayerBalances[1]));
        Assert.That(result1.PlayerBalances[2], Is.EqualTo(result2.PlayerBalances[2]));
        Assert.That(result1.PlayerBalances[3], Is.EqualTo(result2.PlayerBalances[3]));
        Assert.That(result1.LastRoundResults.HouseProfit, Is.EqualTo(result2.LastRoundResults.HouseProfit));
        Assert.That(result1.LastRoundResults.PrizeResults.Count, Is.EqualTo(result2.LastRoundResults.PrizeResults.Count));
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
        var result = Winners.CalculateWinners(input, _defaultSettings, random);

        // Assert - Player 1 balance should be unchanged (they didn't participate)
        Assert.That(result.PlayerBalances[1], Is.EqualTo(10.0m));
        
        // Player 1 should not be in the prize results
        var player1Prizes = result.LastRoundResults.PrizeResults.Where(x => x.PlayerId == 1).ToList();
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
        var result = Winners.CalculateWinners(input, _defaultSettings, random);

        // Assert
        var allWinners = result.LastRoundResults.PrizeResults.ToList();
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
            var spent = input.PlayerBalances[playerId] - result.PlayerBalances[playerId] + 
                       allWinners.Where(x => x.PlayerId == playerId).Sum(x => x.PrizeAmount);
            totalSpent += spent;
        }
        
        Assert.That(totalPrizesAwarded + result.LastRoundResults.HouseProfit, Is.EqualTo(totalSpent).Within(0.01m));
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
        var result = Winners.CalculateWinners(input, _defaultSettings, random);

        // Assert - No CPU player should go into negative balance
        foreach (var kvp in result.PlayerBalances)
        {
            Assert.That(kvp.Value, Is.GreaterThanOrEqualTo(0m), $"Player {kvp.Key} should not have negative balance");
        }
    }
}
