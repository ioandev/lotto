
namespace BedeLotteryConsole.Algos;

public static class FisherAtesRandomShuffler
{
    public static (int, int[], int[]) DrawWinners(Random rng, int totalTickets)
    {
        int[] ticketPool = Enumerable.Range(1, totalTickets).ToArray();

        for (int i = ticketPool.Length - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            (ticketPool[i], ticketPool[j]) = (ticketPool[j], ticketPool[i]); // swap
        }

        // Calculate number of tickets for each tier based on percentages
        int secondTierCount = (int)Math.Floor(totalTickets * 0.10); // 10% of tickets
        int thirdTierCount = (int)Math.Floor(totalTickets * 0.20);  // 20% of tickets

        // Draw winners sequentially from shuffled array
        int grandPrizeTicket = ticketPool[0];           // 1st ticket
        int[] secondTierTickets = ticketPool[1..(1 + secondTierCount)];
        int[] thirdTierTickets = ticketPool[(1 + secondTierCount)..(1 + secondTierCount + thirdTierCount)];

        return (grandPrizeTicket, secondTierTickets, thirdTierTickets);
    }
}