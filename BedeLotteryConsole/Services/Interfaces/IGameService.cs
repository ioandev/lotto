using System.Threading.Channels;
using BedeLotteryConsole.Models;
using BedeLotteryConsole.Commands.Interfaces;

namespace BedeLotteryConsole.Services.Interfaces;

public interface IGameService
{
    ChannelReader<GameState> StateUpdates { get; }
    ChannelWriter<ICommand> UserInput { get; }
    ChannelReader<Exception> Exception { get; }
    CancellationToken AppCancellationToken { get; }
    Task InitializeAsync();
}