using Scriban;
using System.Reflection;
using BedeLotteryConsole.Models;
using BedeLotteryConsole.IO.Interfaces;
using Microsoft.Extensions.Options;
using Scriban.Runtime;
using BedeLotteryConsole.Settings;

namespace BedeLotteryConsole.IO;

public class OutputProcessor : IOutputProcessor
{
    private readonly LottoSettings _lottoSettings;
    private Template? _template;

    public OutputProcessor(IOptions<LottoSettings> lottoSettings)
    {
        _lottoSettings = lottoSettings.Value;
    }

    public void Initialize()
    {
        InitTemplate();

        void InitTemplate()
        {
            // clean up everything in the console
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "BedeLotteryConsole.Templates.ConsoleTemplate.txt";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream!);
            var templateContent = reader.ReadToEnd();

            _template = Template.Parse(templateContent);
        }
    }

    public async Task<string> Process(GameState gameState)
    {
        if (_template is null)
        {
            throw new InvalidOperationException("Template is not initialized.");
        }

        return await _template.RenderAsync(new
        {
            state = gameState,
            currency = _lottoSettings.Currency,
            ticket_price = _lottoSettings.TicketPrice
        }, member => StandardMemberRenamer.Default(member));
    }
}