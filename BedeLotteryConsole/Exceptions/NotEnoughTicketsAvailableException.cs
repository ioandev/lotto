namespace BedeLotteryConsole.Exceptions;

public class NotEnoughTicketsAvailableException : Exception
{
    public int MinimumTickets { get; }
    public int TicketsBet { get; }

    public NotEnoughTicketsAvailableException(int minimumTickets, int ticketsBet)
        : base($"Not enough tickets available. Minimum required: {minimumTickets}, but only {ticketsBet} tickets were bet.")
    {
        MinimumTickets = minimumTickets;
        TicketsBet = ticketsBet;
    }

    public NotEnoughTicketsAvailableException(int minimumTickets, int ticketsBet, string message)
        : base(message)
    {
        MinimumTickets = minimumTickets;
        TicketsBet = ticketsBet;
    }

    public NotEnoughTicketsAvailableException(int minimumTickets, int ticketsBet, string message, Exception innerException)
        : base(message, innerException)
    {
        MinimumTickets = minimumTickets;
        TicketsBet = ticketsBet;
    }

    public override string ToString()
    {
        return $"{GetType().Name}: Not enough tickets available for lottery draw.\n" +
               $"Minimum tickets required: {MinimumTickets}\n" +
               $"Tickets bet: {TicketsBet}\n" +
               $"Shortage: {MinimumTickets - TicketsBet} ticket(s)\n" +
               $"Message: {Message}\n";
    }
}
