

using BedeLotteryConsole.Commands.Interfaces;

namespace BedeLotteryConsole.Commands;

public class BetCommand : ICommand
{
    public int NumberOfTickets { get; init; }
}
