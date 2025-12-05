
using BedeLotteryConsole.Models;

namespace BedeLotteryConsole.IO.Interfaces;

public interface IOutputProcessor
{
    void Initialize();
    Task<string> Process(GameState gameState);
}