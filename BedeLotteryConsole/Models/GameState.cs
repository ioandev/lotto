
namespace BedeLotteryConsole.Models;

public class GameState
{
    public Dictionary<int, decimal> PlayerBalances { get; init; } = new();
    public RoundResults? LastRoundResults { get; init; } = null;
    public int? CPUPlayersTicketsAmount { get; init; }
}