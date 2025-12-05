
namespace BedeLotteryConsole.Models;

public class WinnersInput
{
    public int Player1TicketsAmount { get; init; }
    public Dictionary<int, decimal> PlayerBalances { get; init; } = new();
}