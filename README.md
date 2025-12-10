# Bede Lottery Console

A well-architected .NET console application simulating a lottery game with multiple players. Built with clean code principles, dependency injection, and comprehensive testing.

## 🎯 Overview

This lottery simulation features:
- **Multi-tier prize system**: Grand prize, second tier, and third tier winners
- **Interactive gameplay**: Players can buy tickets and participate in lottery rounds
- **Smart CPU players**: Automated players with randomized ticket purchases
- **Fair distribution**: Fisher-Yates shuffle algorithm ensures unbiased winner selection
- **Template-based UI**: Clean console output using Scriban templates

## 🚀 Quick Start

### Prerequisites

- .NET 10.0 SDK or later

### Running the Application

```bash
cd BedeLotteryConsole
dotnet run
```

### Running Tests

```bash
dotnet test
```

## 🏗️ Architecture

### Project Structure

```
BedeLotteryConsole/          # Main application
├── Models/                  # Domain models
├── Services/                # Business logic
│   └── Interfaces/          # Service contracts
├── Settings/                # Configuration classes
└── Templates/               # Scriban UI templates

BedeLotteryConsole.Tests/    # Unit tests
└── Services/                # Service tests
```

### Key Design Principles

#### ✨ Readability
- **Organized namespaces**: Classes grouped by type with separated interfaces
- **Descriptive naming**: Self-documenting classes, methods, and variables
- **Modern solution format**: Uses `.slnx` format for better version control
- **Template-driven UI**: Scriban templates instead of string interpolation

#### 🧪 Testability
- **Dependency injection**: Services registered via Microsoft.Extensions.DependencyInjection
- **Interface-based design**: All services implement contracts for easy mocking
- **Isolated algorithms**: Winner selection logic separated for independent testing
- **60%+ code coverage**: Comprehensive unit tests for business logic

#### 🔧 Extensibility
- **State management**: `GameState` easily extended with new properties
- **Service-oriented**: New features can be added as additional services
- **Template extensibility**: UI changes require only template modifications

#### ⚙️ Configurability

All game parameters are configurable via `appsettings.json`:

| Setting | Description | Default |
|---------|-------------|---------|
| `InitialBalance` | Starting balance for each player | 10.0 |
| `TicketPrice` | Cost per lottery ticket | 1.0 |
| `MaxTicketsPerPlayers` | Maximum tickets per player per round | 10 |
| `MaxPlayersPerGame` | Maximum CPU players in the game | 15 |

## 🎮 How to Play

1. **Start the game**: Run the application to begin
2. **Buy tickets**: Enter commands when prompted (1-10 tickets)
3. **Watch the draw**: CPU players automatically participate
4. **Win prizes**: Three prize tiers are awarded each round


## 🛠️ Development

### Code Coverage

Generate and view code coverage reports:

```bash
# Install report generator (one-time setup)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Generate HTML report
reportgenerator -reports:BedeLotteryConsole.Tests/coverage.opencover.xml \
  -targetdir:coverage-report \
  -reporttypes:Html

# View coverage summary
reportgenerator -reports:BedeLotteryConsole.Tests/coverage.opencover.xml \
  -targetdir:coverage-report \
  -reporttypes:TextSummary && cat coverage-report/Summary.txt
```

### Snapshot Testing

After reviewing and approving snapshot changes:

```bash
cd BedeLotteryConsole.Tests/IO/Snapshots
for file in *.received.txt; do 
  mv "$file" "${file/.received.txt/.verified.txt}"
done
```

### Local CI/CD Pipeline

Run GitHub Actions locally using [act](https://github.com/nektos/act):

```bash
# Install act
curl https://raw.githubusercontent.com/nektos/act/master/install.sh | sudo bash

# Configure for podman
export DOCKER_HOST=unix:///run/user/$(id -u)/podman/podman.sock

# Run the pipeline
./bin/act
```

## 📋 Future Improvements

- [ ] Increase unit test coverage to 80%+
- [ ] Add integration tests for end-to-end scenarios
- [ ] Expand configuration options (e.g., prize distribution percentages)
- [ ] Implement persistent game state
- [ ] Add logging framework for better observability

## 📝 Technical Notes

- **IDE**: Developed using Visual Studio Code
- **Framework**: .NET 10.0
- **DI Container**: Microsoft.Extensions.DependencyInjection
- **Templating**: Scriban for console output rendering

## 📄 License

This project was created as a technical assessment for Bede Gaming.

---

**Enjoy the lottery and good luck! 🍀**