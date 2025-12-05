using BedeLotteryConsole.Algos;

namespace BedeLotteryConsole.Tests.Algos;

[TestFixture]
public class FisherAtesRandomShufflerTests
{
    [Test]
    public void DrawWinners_WithFixedSeed_ReturnsSameResultsEveryTime()
    {
        // Arrange
        const int seed = 111;
        const int totalTickets = 100;
        var rng = new Random(seed);

        // Act
        var (grandPrize, secondTier, thirdTier) = FisherAtesRandomShuffler.DrawWinners(rng, totalTickets);

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
        var result1 = FisherAtesRandomShuffler.DrawWinners(new Random(seed), totalTickets);
        var result2 = FisherAtesRandomShuffler.DrawWinners(new Random(seed), totalTickets);

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
        var (grandPrize, secondTier, thirdTier) = FisherAtesRandomShuffler.DrawWinners(rng, totalTickets);

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
        var (grandPrize, secondTier, thirdTier) = FisherAtesRandomShuffler.DrawWinners(rng, totalTickets);

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
        var (grandPrize, secondTier, thirdTier) = FisherAtesRandomShuffler.DrawWinners(rng, totalTickets);

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
            var (_, secondTier, thirdTier) = FisherAtesRandomShuffler.DrawWinners(rng, testCase.TotalTickets);

            Assert.That(secondTier.Length, Is.EqualTo(testCase.ExpectedSecondTier),
                $"For {testCase.TotalTickets} tickets, second tier should be {testCase.ExpectedSecondTier}");
            Assert.That(thirdTier.Length, Is.EqualTo(testCase.ExpectedThirdTier),
                $"For {testCase.TotalTickets} tickets, third tier should be {testCase.ExpectedThirdTier}");
        }
    }
}
