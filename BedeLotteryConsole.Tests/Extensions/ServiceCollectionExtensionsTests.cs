using BedeLotteryConsole.Extensions;
using BedeLotteryConsole.IO;
using BedeLotteryConsole.IO.Interfaces;
using BedeLotteryConsole.Services;
using BedeLotteryConsole.Services.Interfaces;
using BedeLotteryConsole.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BedeLotteryConsole.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    private IServiceCollection _services = null!;
    private IConfiguration _configuration = null!;

    [SetUp]
    public void SetUp()
    {
        _services = new ServiceCollection();

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "LottoSettings:InitialBalance", "10.0" },
            { "LottoSettings:TicketPrice", "1.0" },
            { "LottoSettings:MaxTicketsPerPlayers", "10" },
            { "LottoSettings:MaxPlayersPerGame", "15" },
            { "LottoSettings:Currency", "$" }
        });
        _configuration = configurationBuilder.Build();
    }

    [Test]
    public void AddApplicationServices_ReturnsServiceCollection()
    {
        // Act
        var result = _services.AddApplicationServices(_configuration);

        // Assert
        Assert.That(result, Is.SameAs(_services));
    }

    [Test]
    public void AddApplicationServices_RegistersLottoSettings()
    {
        // Act
        _services.AddApplicationServices(_configuration);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetService<IOptions<LottoSettings>>();
        Assert.That(options, Is.Not.Null);
        Assert.That(options!.Value, Is.Not.Null);
        Assert.That(options.Value.InitialBalance, Is.EqualTo(10.0m));
        Assert.That(options.Value.TicketPrice, Is.EqualTo(1.0m));
        Assert.That(options.Value.MaxTicketsPerPlayers, Is.EqualTo(10));
        Assert.That(options.Value.MaxPlayersPerGame, Is.EqualTo(15));
        Assert.That(options.Value.Currency, Is.EqualTo("$"));
    }

    [Test]
    public void AddApplicationServices_RegistersGameServiceAsSingleton()
    {
        // Act
        _services.AddApplicationServices(_configuration);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var service1 = serviceProvider.GetService<IGameService>();
        var service2 = serviceProvider.GetService<IGameService>();

        Assert.That(service1, Is.Not.Null);
        Assert.That(service1, Is.InstanceOf<GameService>());
        Assert.That(service2, Is.SameAs(service1), "Should be singleton");
    }

    [Test]
    public void AddApplicationServices_RegistersInputProcessorAsSingleton()
    {
        // Act
        _services.AddApplicationServices(_configuration);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var service1 = serviceProvider.GetService<IInputProcessor>();
        var service2 = serviceProvider.GetService<IInputProcessor>();

        Assert.That(service1, Is.Not.Null);
        Assert.That(service1, Is.InstanceOf<InputProcessor>());
        Assert.That(service2, Is.SameAs(service1), "Should be singleton");
    }

    [Test]
    public void AddApplicationServices_RegistersOutputProcessorAsSingleton()
    {
        // Act
        _services.AddApplicationServices(_configuration);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var service1 = serviceProvider.GetService<IOutputProcessor>();
        var service2 = serviceProvider.GetService<IOutputProcessor>();

        Assert.That(service1, Is.Not.Null);
        Assert.That(service1, Is.InstanceOf<OutputProcessor>());
        Assert.That(service2, Is.SameAs(service1), "Should be singleton");
    }

    [Test]
    public void AddApplicationServices_RegistersConsoleIOAsSingleton()
    {
        // Act
        _services.AddApplicationServices(_configuration);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var service1 = serviceProvider.GetService<IConsoleIO>();
        var service2 = serviceProvider.GetService<IConsoleIO>();

        Assert.That(service1, Is.Not.Null);
        Assert.That(service1, Is.InstanceOf<ConsoleIO>());
        Assert.That(service2, Is.SameAs(service1), "Should be singleton");
    }

    [Test]
    public void AddApplicationServices_RegistersAllServices()
    {
        // Act
        _services.AddApplicationServices(_configuration);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert - verify all services can be resolved
        Assert.That(serviceProvider.GetService<IGameService>(), Is.Not.Null);
        Assert.That(serviceProvider.GetService<IInputProcessor>(), Is.Not.Null);
        Assert.That(serviceProvider.GetService<IOutputProcessor>(), Is.Not.Null);
        Assert.That(serviceProvider.GetService<IConsoleIO>(), Is.Not.Null);
        Assert.That(serviceProvider.GetService<IOptions<LottoSettings>>(), Is.Not.Null);
    }

    [Test]
    public void AddApplicationServices_WithCustomConfiguration_UsesCustomValues()
    {
        // Arrange
        var customConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "LottoSettings:InitialBalance", "50.0" },
                { "LottoSettings:TicketPrice", "2.5" },
                { "LottoSettings:MaxTicketsPerPlayers", "20" },
                { "LottoSettings:MaxPlayersPerGame", "30" },
                { "LottoSettings:Currency", "€" }
            })
            .Build();

        // Act
        _services.AddApplicationServices(customConfig);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetService<IOptions<LottoSettings>>();
        Assert.That(options, Is.Not.Null);
        Assert.That(options!.Value.InitialBalance, Is.EqualTo(50.0m));
        Assert.That(options.Value.TicketPrice, Is.EqualTo(2.5m));
        Assert.That(options.Value.MaxTicketsPerPlayers, Is.EqualTo(20));
        Assert.That(options.Value.MaxPlayersPerGame, Is.EqualTo(30));
        Assert.That(options.Value.Currency, Is.EqualTo("€"));
    }

    [Test]
    public void AddApplicationServices_WithMissingConfiguration_UsesDefaultValues()
    {
        // Arrange
        var emptyConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Act
        _services.AddApplicationServices(emptyConfig);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetService<IOptions<LottoSettings>>();
        Assert.That(options, Is.Not.Null);
        Assert.That(options!.Value, Is.Not.Null);
        // Should use default values from LottoSettings class
        Assert.That(options.Value.InitialBalance, Is.EqualTo(10.0m));
        Assert.That(options.Value.TicketPrice, Is.EqualTo(1.0m));
        Assert.That(options.Value.MaxTicketsPerPlayers, Is.EqualTo(10));
        Assert.That(options.Value.MaxPlayersPerGame, Is.EqualTo(15));
        Assert.That(options.Value.Currency, Is.EqualTo("$"));
    }
}
