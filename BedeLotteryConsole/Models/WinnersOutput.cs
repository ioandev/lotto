namespace BedeLotteryConsole.Models;

public class WinnersOutput
{
    public Dictionary<int, decimal> PlayerBalances { get; set; } = new();
    public RoundResults LastRoundResults { get; set; } = new();
    public int CPUPlayersTicketsCount { get; set; }
}