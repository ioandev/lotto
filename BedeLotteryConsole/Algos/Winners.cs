
using BedeLotteryConsole.Settings;
using BedeLotteryConsole.Models;

namespace BedeLotteryConsole.Algos;

internal static class Winners
{
    public const int MinTicketsRequired = 10;

    public static WinnersOutput? CalculateWinners(WinnersInput input, LottoSettings lottoSettings, Random random)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        var ticketsPerPlayers = GetTicketsPerPlayersRandom(input, lottoSettings, random);
        var ticketsAndPlayers = GetTicketsForPlayers(ticketsPerPlayers, random);

        if (ticketsAndPlayers.Count < MinTicketsRequired)
        {
            return null;
        }

        var (grandPrizeTicket, secondTierTickets, thirdTierTickets) = DrawWinners(random, ticketsAndPlayers.Count);

        var totalPoolAmount = ticketsAndPlayers.Count * lottoSettings.TicketPrice;
        var totalGrandPrize = totalPoolAmount * 0.5m; // 50% of revenue
        var totalSecondTierPrize = totalPoolAmount * 0.3m; // 30% of revenue
        var totalThirdTierPrize = totalPoolAmount * 0.1m; // 10% of revenue

        var winnerAmounts = GetWinnerAmounts(
            ticketsAndPlayers,
            grandPrizeTicket,
            secondTierTickets,
            thirdTierTickets,
            totalGrandPrize,
            totalSecondTierPrize,
            totalThirdTierPrize);

        var winnerAmountsBalance = winnerAmounts.ToDictionary(x => x.PlayerId, x => x.AmountTotal);

        var amountPaid = winnerAmounts.Select(x => x.AmountTotal).Sum();
        var totalHouseProfit = totalPoolAmount - amountPaid;

        var prizeResults = winnerAmounts
            .Where(x => x.AmountTotal > 0) // Only include actual winners
            .Select(x =>
            {
                return new PrizeResult
                {
                    PlayerId = x.PlayerId,
                    PrizeType = x.grandPrizeTicket == 1 ? PrizeType.Grand :
                                x.secondTierPrizes > 0 ? PrizeType.SecondTier :
                                x.thirdTierPrizes > 0 ? PrizeType.ThirdTier :
                                PrizeType.Invalid,
                    PrizeAmount = x.AmountTotal,
                    Tickets = x.grandPrizeTicket + x.secondTierPrizes + x.thirdTierPrizes
                };
            }).ToList();

        return new WinnersOutput
        {
            PlayerBalances = input.PlayerBalances.ToDictionary(x => x.Key, x =>
                x.Value
                - (ticketsPerPlayers[x.Key] * lottoSettings.TicketPrice)
                + (winnerAmountsBalance.ContainsKey(x.Key) ? winnerAmountsBalance[x.Key] : 0m)
            ),
            CPUPlayersTicketsCount = ticketsPerPlayers
                .Count(x => x.Key != Constants.HumanPlayerId && x.Value > 0),
            LastRoundResults = new RoundResults
            {
                PrizeResults = prizeResults,
                HouseProfit = totalHouseProfit
            }
        };
    }
    
    internal static (int, int[], int[]) DrawWinners(Random rng, int totalTickets)
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

    // Automatically generated for CPU players (random)
    private static Dictionary<int, int> GetTicketsPerPlayersRandom(WinnersInput input, LottoSettings lottoSettings, Random random)
    {
        var ticketsPerPlayers = 
            Enumerable.Range(2, input.PlayerBalances.Count - 1)
                .Select(playerId => {
                    var playerBalance = input.PlayerBalances[playerId];
                    var maxBettableTicketsAccordingToCurrentBalance = (int)(playerBalance / lottoSettings.TicketPrice);
                    if (maxBettableTicketsAccordingToCurrentBalance < 1)
                    {
                        return (PlayerId: playerId, Tickets: 0);
                    }

                    var maxTicketsForThisPlayer = Math.Min(maxBettableTicketsAccordingToCurrentBalance, lottoSettings.MaxTicketsPerPlayers);
                    return (PlayerId: playerId, Tickets: random.Next(1, maxTicketsForThisPlayer + 1));
                })
                .ToDictionary(x => x.PlayerId, x => x.Tickets);

        ticketsPerPlayers.Add(1, input.Player1TicketsAmount);
        return ticketsPerPlayers;
    }

    private static List<(int TicketId, int PlayerId)> GetTicketsForPlayers(Dictionary<int, int> ticketsPerPlayers, Random random)
    {
        var ticketId = 1;
        return ticketsPerPlayers.SelectMany(x =>
        {
            return Enumerable.Range(0, x.Value)
                .Select(_ => {
                    var thisTicketId = ticketId++;
                    return (TicketId: thisTicketId, PlayerId: x.Key);
                })
                .ToList();
        }).ToList();
    }

    private static List<(int PlayerId, decimal AmountTotal, int grandPrizeTicket, int secondTierPrizes, int thirdTierPrizes)> GetWinnerAmounts(
        List<(int TicketId, int PlayerId)> ticketsAndPlayers,
        int grandPrizeTicket,
        int[] secondTierTickets,
        int[] thirdTierTickets,
        decimal totalGrandPrize,
        decimal totalSecondTierPrize,
        decimal totalThirdTierPrize)
    {
        return ticketsAndPlayers.GroupBy(x => x.PlayerId)
            .Select(g =>
            {
                var playerId = g.Key;
                var ticketIds = g.Select(x => x.TicketId).ToList();

                var amounts = new List<decimal>();
                if (ticketIds.Contains(grandPrizeTicket))
                {
                    amounts.Add(totalGrandPrize);
                }

                var wonSecondTierPrizes = ticketIds.Intersect(secondTierTickets);
                if (wonSecondTierPrizes.Any())
                {
                    amounts.Add(wonSecondTierPrizes.Count() * (totalSecondTierPrize / secondTierTickets.Length));
                } 
                var wonThirdTierPrizes = ticketIds.Intersect(thirdTierTickets);
                if (wonThirdTierPrizes.Any())
                {
                    amounts.Add(wonThirdTierPrizes.Count() * (totalThirdTierPrize / thirdTierTickets.Length));
                }

                return (
                    PlayerId: playerId,
                    AmountTotal: amounts.Sum(),
                    grandPrizeTicket: ticketIds.Contains(grandPrizeTicket) ? 1 : 0,
                    secondTierPrizes: wonSecondTierPrizes.Count(),
                    thirdTierPrizes: wonThirdTierPrizes.Count()
                );
            }).ToList();
    }
}