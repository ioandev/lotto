
namespace BedeLotteryConsole.Models;

public class PrizeResult
{
    public int PlayerId { get; init; }
    public PrizeType PrizeType { get; init; }
    public decimal PrizeAmount { get; init; }
    public int Tickets { get; init; }
}
