
using BedeLotteryConsole.Models;

namespace BedeLotteryConsole.IO.Interfaces;

public interface IInputProcessor
{
    Task Process(GameState state, string input);
}