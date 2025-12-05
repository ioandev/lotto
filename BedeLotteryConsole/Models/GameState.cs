
namespace BedeLotteryConsole.Models;

public class GameState
{
    public Dictionary<int, decimal> PlayerBalances { get; init; } = new();
    public RoundResults? LastRoundResults { get; init; } = null;
    public StatusType StatusType { get; init; } = default;
    public int Player1TicketsAmount { get; set; } // TODO: this would benefit from a Clone method
    public int? CPUPlayersTicketsAmount { get; init; }
    
}