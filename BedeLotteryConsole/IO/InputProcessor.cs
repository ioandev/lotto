
using BedeLotteryConsole.Models;
using BedeLotteryConsole.Commands;
using BedeLotteryConsole.Services.Interfaces;
using BedeLotteryConsole.IO.Interfaces;
using Microsoft.Extensions.Options;
using BedeLotteryConsole.Settings;

namespace BedeLotteryConsole.IO;

public class InputProcessor : IInputProcessor
{
    private readonly IGameService _gameService;
    private readonly LottoSettings _lottoSettings;

    public InputProcessor(IGameService gameService, IOptions<LottoSettings> lottoSettings)
    {
        _gameService = gameService;
        _lottoSettings = lottoSettings.Value;
    }

    public async Task Process(GameState state, string input)
    {
        if (state is null)
        {
            return;
        }

        if (input == "e" || input == "exit")
        {
            var exitCommand = new ExitCommand();
            await _gameService.UserInput.WriteAsync(exitCommand, CancellationToken.None);
            return;
        }

        if (state.StatusType == StatusType.WaitingForBet)
        {
            var maximumTicketsAffordable = (int)(state.PlayerBalances[Constants.HumanPlayerId] / _lottoSettings.TicketPrice);
            if (int.TryParse(input, out var numberOfTickets) &&
                numberOfTickets > 0 && numberOfTickets <= _lottoSettings.MaxTicketsPerPlayers && 
                numberOfTickets * _lottoSettings.TicketPrice <= state.PlayerBalances[Constants.HumanPlayerId]) // TODO: move this logic
            {
                var betCommand = new BetCommand
                {
                    NumberOfTickets = numberOfTickets
                };
                await _gameService.UserInput.WriteAsync(betCommand, CancellationToken.None);
            }
            else
            {
                throw new InvalidOperationException($"Invalid number of tickets. Please enter a positive integer. Example: 10. Minimum: 1; Maximum: {_lottoSettings.MaxTicketsPerPlayers}; You can afford: {maximumTicketsAffordable} tickets. You entered: {input}");
            }
        }
        else if (state.StatusType == StatusType.ShowingResults)
        {
            if (input == "n")
            {
                var exitCommand = new ExitCommand();
                await _gameService.UserInput.WriteAsync(exitCommand, CancellationToken.None);
                return;
            }
            else if (input.ToLower() == "y")
            {
                var nextRoundCommand = new NextRoundCommand();
                await _gameService.UserInput.WriteAsync(nextRoundCommand, CancellationToken.None);
            }
            else
            {
                throw new InvalidOperationException("Invalid command. Please enter 'y' to play the next round or 'n' to exit. You entered: " + input);
            }
        }
        else if (state.StatusType == StatusType.CannotStartDrawNotEnoughTickets)
        {
            if (input == "n")
            {
                var exitCommand = new ExitCommand();
                await _gameService.UserInput.WriteAsync(exitCommand, CancellationToken.None);
                return;
            }
            else if (input.ToLower() == "y")
            {
                var nextRoundCommand = new NextRoundCommand();
                await _gameService.UserInput.WriteAsync(nextRoundCommand, CancellationToken.None);
            }
            else
            {
                throw new InvalidOperationException("Invalid command. Please enter 'y' to play the next round or 'n' to exit. You entered: " + input);
            }
        }
        else if (state.StatusType == StatusType.GameOver)
        {
            var exitCommand = new ExitCommand();
            await _gameService.UserInput.WriteAsync(exitCommand, CancellationToken.None);
        }
    }
}