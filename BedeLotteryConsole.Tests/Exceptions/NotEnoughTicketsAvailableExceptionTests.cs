using BedeLotteryConsole.Exceptions;

namespace BedeLotteryConsole.Tests.Exceptions;

[TestFixture]
public class NotEnoughTicketsAvailableExceptionTests
{
    [Test]
    public void Constructor_WithMinimumAndTicketsBet_SetsPropertiesCorrectly()
    {
        // Arrange
        int minimumTickets = 10;
        int ticketsBet = 5;

        // Act
        var exception = new NotEnoughTicketsAvailableException(minimumTickets, ticketsBet);

        // Assert
        Assert.That(exception.MinimumTickets, Is.EqualTo(minimumTickets));
        Assert.That(exception.TicketsBet, Is.EqualTo(ticketsBet));
        Assert.That(exception.Message, Is.EqualTo($"Not enough tickets available. Minimum required: {minimumTickets}, but only {ticketsBet} tickets were bet."));
    }

    [Test]
    public void Constructor_WithCustomMessage_SetsPropertiesCorrectly()
    {
        // Arrange
        int minimumTickets = 15;
        int ticketsBet = 7;
        string customMessage = "Custom error message";

        // Act
        var exception = new NotEnoughTicketsAvailableException(minimumTickets, ticketsBet, customMessage);

        // Assert
        Assert.That(exception.MinimumTickets, Is.EqualTo(minimumTickets));
        Assert.That(exception.TicketsBet, Is.EqualTo(ticketsBet));
        Assert.That(exception.Message, Is.EqualTo(customMessage));
    }

    [Test]
    public void Constructor_WithInnerException_SetsPropertiesCorrectly()
    {
        // Arrange
        int minimumTickets = 20;
        int ticketsBet = 10;
        string customMessage = "Custom error with inner exception";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new NotEnoughTicketsAvailableException(minimumTickets, ticketsBet, customMessage, innerException);

        // Assert
        Assert.That(exception.MinimumTickets, Is.EqualTo(minimumTickets));
        Assert.That(exception.TicketsBet, Is.EqualTo(ticketsBet));
        Assert.That(exception.Message, Is.EqualTo(customMessage));
        Assert.That(exception.InnerException, Is.EqualTo(innerException));
    }

    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        int minimumTickets = 10;
        int ticketsBet = 3;
        var exception = new NotEnoughTicketsAvailableException(minimumTickets, ticketsBet);

        // Act
        string result = exception.ToString();

        // Assert
        Assert.That(result, Does.Contain("NotEnoughTicketsAvailableException"));
        Assert.That(result, Does.Contain("Not enough tickets available for lottery draw"));
        Assert.That(result, Does.Contain($"Minimum tickets required: {minimumTickets}"));
        Assert.That(result, Does.Contain($"Tickets bet: {ticketsBet}"));
        Assert.That(result, Does.Contain($"Shortage: {minimumTickets - ticketsBet} ticket(s)"));
        Assert.That(result, Does.Contain(exception.Message));
    }

    [Test]
    public void ToString_WithCustomMessage_ReturnsFormattedStringWithCustomMessage()
    {
        // Arrange
        int minimumTickets = 12;
        int ticketsBet = 4;
        string customMessage = "Special error message";
        var exception = new NotEnoughTicketsAvailableException(minimumTickets, ticketsBet, customMessage);

        // Act
        string result = exception.ToString();

        // Assert
        Assert.That(result, Does.Contain("NotEnoughTicketsAvailableException"));
        Assert.That(result, Does.Contain($"Minimum tickets required: {minimumTickets}"));
        Assert.That(result, Does.Contain($"Tickets bet: {ticketsBet}"));
        Assert.That(result, Does.Contain($"Shortage: {minimumTickets - ticketsBet} ticket(s)"));
        Assert.That(result, Does.Contain(customMessage));
    }

    [Test]
    public void Exception_IsThrowable()
    {
        // Arrange
        int minimumTickets = 10;
        int ticketsBet = 5;

        // Act & Assert
        var exception = Assert.Throws<NotEnoughTicketsAvailableException>(() =>
        {
            throw new NotEnoughTicketsAvailableException(minimumTickets, ticketsBet);
        });

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.MinimumTickets, Is.EqualTo(minimumTickets));
        Assert.That(exception.TicketsBet, Is.EqualTo(ticketsBet));
    }

    [Test]
    public void Exception_InheritsFromException()
    {
        // Arrange
        var exception = new NotEnoughTicketsAvailableException(10, 5);

        // Assert
        Assert.That(exception, Is.InstanceOf<Exception>());
    }

    [Test]
    public void Shortage_IsCalculatedCorrectly()
    {
        // Arrange
        int minimumTickets = 25;
        int ticketsBet = 8;
        var exception = new NotEnoughTicketsAvailableException(minimumTickets, ticketsBet);

        // Act
        string result = exception.ToString();

        // Assert
        int expectedShortage = minimumTickets - ticketsBet;
        Assert.That(result, Does.Contain($"Shortage: {expectedShortage} ticket(s)"));
    }
}
