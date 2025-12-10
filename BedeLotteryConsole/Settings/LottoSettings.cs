
namespace BedeLotteryConsole.Settings;

public class LottoSettings
{
    public decimal InitialBalance { get; set; } = 10.0m;
    public decimal TicketPrice { get; set; } = 1.0m;
    public int MaxTicketsPerPlayers { get; set; } = 10;
    public int MaxPlayersPerGame { get; set; } = 15;
}