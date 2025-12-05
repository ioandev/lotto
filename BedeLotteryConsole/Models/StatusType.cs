
namespace BedeLotteryConsole.Models;

public enum StatusType
{
    Invalid = 0,
    WaitingForBet = 1,
    ShowingResults = 2,
    CannotStartDrawNotEnoughTickets = 3,
    GameOver = 4,
}