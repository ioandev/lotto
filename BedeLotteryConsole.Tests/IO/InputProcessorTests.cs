using System.Threading.Channels;
using BedeLotteryConsole.Commands;
using BedeLotteryConsole.Commands.Interfaces;
using BedeLotteryConsole.IO;
using BedeLotteryConsole.Models;
using BedeLotteryConsole.Services.Interfaces;
using Microsoft.Extensions.Options;
using BedeLotteryConsole.Settings;
using Moq;

namespace BedeLotteryConsole.Tests.IO;

[TestFixture]
public class InputProcessorTests
{
    private Mock<IGameService> _mockGameService;
    private Mock<IOptions<LottoSettings>> _mockLottoSettings;
    private Channel<ICommand> _commandChannel;
    private InputProcessor _inputProcessor;

    [SetUp]
    public void SetUp()
    {
        _mockGameService = new Mock<IGameService>();
        _commandChannel = Channel.CreateUnbounded<ICommand>();
        
        _mockGameService.Setup(x => x.UserInput).Returns(_commandChannel.Writer);
        
        _mockLottoSettings = new Mock<IOptions<LottoSettings>>();
        _mockLottoSettings.Setup(x => x.Value).Returns(new LottoSettings());
        
        _inputProcessor = new InputProcessor(_mockGameService.Object, _mockLottoSettings.Object);
    }

    [Test]
    public async Task Process_WithNullState_DoesNothing()
    {
        // Act
        await _inputProcessor.Process(null!, "anything");

        // Assert
        Assert.That(_commandChannel.Reader.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task Process_WithExitCommand_WritesExitCommandToChannel()
    {
        // Arrange
        var state = new GameState { StatusType = StatusType.WaitingForBet };

        // Act
        await _inputProcessor.Process(state, "exit");

        // Assert
        var command = await _commandChannel.Reader.ReadAsync();
        Assert.That(command, Is.InstanceOf<ExitCommand>());
    }

    [Test]
    public async Task Process_WithECommand_WritesExitCommandToChannel()
    {
        // Arrange
        var state = new GameState { StatusType = StatusType.WaitingForBet };

        // Act
        await _inputProcessor.Process(state, "e");

        // Assert
        var command = await _commandChannel.Reader.ReadAsync();
        Assert.That(command, Is.InstanceOf<ExitCommand>());
    }

    [Test]
    public async Task Process_WaitingForBet_WithValidNumber_WritesBetCommand()
    {
        // Arrange
        var state = new GameState 
        { 
            StatusType = StatusType.WaitingForBet,
            PlayerBalances = new Dictionary<int, decimal> { { 1, 10.0m } }
        };

        // Act
        await _inputProcessor.Process(state, "5");

        // Assert
        var command = await _commandChannel.Reader.ReadAsync();
        Assert.That(command, Is.InstanceOf<BetCommand>());
        var betCommand = (BetCommand)command;
        Assert.That(betCommand.NumberOfTickets, Is.EqualTo(5));
    }

    [Test]
    public async Task Process_WaitingForBet_WithLargeValidNumber_WritesBetCommand()
    {
        // Arrange
        var state = new GameState 
        { 
            StatusType = StatusType.WaitingForBet,
            PlayerBalances = new Dictionary<int, decimal> { { 1, 10.0m } }
        };

        // Act
        await _inputProcessor.Process(state, "10");

        // Assert
        var command = await _commandChannel.Reader.ReadAsync();
        Assert.That(command, Is.InstanceOf<BetCommand>());
        var betCommand = (BetCommand)command;
        Assert.That(betCommand.NumberOfTickets, Is.EqualTo(10));
    }

    [Test]
    public void Process_WaitingForBet_WithZero_ThrowsInvalidOperationException()
    {
        // Arrange
        var state = new GameState { StatusType = StatusType.WaitingForBet, PlayerBalances = new Dictionary<int, decimal> { { 1, 10.0m } } };

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _inputProcessor.Process(state, "0"));
    }

    [Test]
    public void Process_WaitingForBet_WithNegativeNumber_ThrowsInvalidOperationException()
    {
        // Arrange
        var state = new GameState { StatusType = StatusType.WaitingForBet, PlayerBalances = new Dictionary<int, decimal> { { 1, 10.0m } } };

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _inputProcessor.Process(state, "-5"));
    }

    [Test]
    public void Process_WaitingForBet_WithInvalidText_ThrowsInvalidOperationException()
    {
        // Arrange
        var state = new GameState { StatusType = StatusType.WaitingForBet, PlayerBalances = new Dictionary<int, decimal> { { 1, 10.0m } } };

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _inputProcessor.Process(state, "abc"));
    }

    [Test]
    public void Process_WaitingForBet_WithDecimalNumber_ThrowsInvalidOperationException()
    {
        // Arrange
        var state = new GameState { StatusType = StatusType.WaitingForBet, PlayerBalances = new Dictionary<int, decimal> { { 1, 10.0m } } };

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _inputProcessor.Process(state, "5.5"));
    }

    [Test]
    public async Task Process_ShowingResults_WithYCommand_WritesNextRoundCommand()
    {
        // Arrange
        var state = new GameState { StatusType = StatusType.ShowingResults };

        // Act
        await _inputProcessor.Process(state, "y");

        // Assert
        var command = await _commandChannel.Reader.ReadAsync();
        Assert.That(command, Is.InstanceOf<NextRoundCommand>());
    }

    [Test]
    public async Task Process_ShowingResults_WithUppercaseYCommand_WritesNextRoundCommand()
    {
        // Arrange
        var state = new GameState { StatusType = StatusType.ShowingResults };

        // Act
        await _inputProcessor.Process(state, "Y");

        // Assert
        var command = await _commandChannel.Reader.ReadAsync();
        Assert.That(command, Is.InstanceOf<NextRoundCommand>());
    }

    [Test]
    public async Task Process_ShowingResults_WithNCommand_WritesExitCommand()
    {
        // Arrange
        var state = new GameState { StatusType = StatusType.ShowingResults };

        // Act
        await _inputProcessor.Process(state, "n");

        // Assert
        var command = await _commandChannel.Reader.ReadAsync();
        Assert.That(command, Is.InstanceOf<ExitCommand>());
    }

    [Test]
    public void Process_ShowingResults_WithInvalidCommand_ThrowsInvalidOperationException()
    {
        // Arrange
        var state = new GameState { StatusType = StatusType.ShowingResults };

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _inputProcessor.Process(state, "invalid"));
    }

    [Test]
    public void Process_ShowingResults_WithRandomText_ThrowsInvalidOperationException()
    {
        // Arrange
        var state = new GameState { StatusType = StatusType.ShowingResults };

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _inputProcessor.Process(state, "xyz"));
    }
}
