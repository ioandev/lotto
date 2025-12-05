
namespace BedeLotteryConsole.Models;

public record RoundResults
{
    public List<PrizeResult> PrizeResults { get; init; } = new();
    public decimal HouseProfit { get; init; }
}
