
using BedeLotteryConsole.Models;

namespace BedeLotteryConsole.Services.Interfaces;

public interface IGameService
{
    CancellationToken AppCancellationToken { get; }
    void Initialize();
}