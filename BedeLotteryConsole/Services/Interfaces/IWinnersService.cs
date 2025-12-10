using BedeLotteryConsole.Models;
using BedeLotteryConsole.Settings;

namespace BedeLotteryConsole.Services.Interfaces;

public interface IWinnersService
{
    WinnersOutput? CalculateWinners(WinnersInput input, Random random);
    (int, int[], int[]) DrawWinners(Random rng, int totalTickets);
}
