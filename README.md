# Bede Lottery Console

A well-architected lottery simulation console application built with .NET 10, demonstrating professional software engineering practices and clean code principles.

## 🎯 Overview

This project is a configurable lottery game simulator that supports multiple players (including CPU-controlled players), ticket purchasing, and random prize distribution. The application showcases best practices in software design, testing, and maintainability.

## ✨ Features

- **Interactive Console Interface** - Clean, template-based output with real-time game state updates
- **Configurable Game Settings** - Adjust balance, ticket prices, player limits, and currency via JSON configuration
- **Fair Random Distribution** - Fisher-Yates shuffle algorithm ensures unbiased winner selection
- **CPU Players** - Automated players with random ticket purchasing behavior
- **State Management** - Robust game state machine with multiple game conditions
- **Comprehensive Testing** - Unit tests with 76.39% line coverage and 66.3% branch coverage
- **Extensible Architecture** - Command pattern and dependency injection for easy feature additions

## 🚀 Quick Start

### Prerequisites

- .NET 10.0 SDK or later
- Visual Studio Code (recommended) or any .NET-compatible IDE

### Running the Application

```bash
cd BedeLotteryConsole
dotnet run
```

## ⚙️ Configuration

Edit `BedeLotteryConsole/appsettings.json` to customize game settings:

```json
{
  "LottoSettings": {
    "InitialBalance": 10.0,
    "TicketPrice": 1.0,
    "MaxTicketsPerPlayers": 10,
    "MaxPlayersPerGame": 15,
    "Currency": "$"
  }
}
```

## 🏗️ Architecture & Design

### Project Structure

```
BedeLotteryConsole/
├── Algos/              # Core algorithms (Fisher-Yates shuffle, winner selection)
├── Commands/           # Command pattern implementations (Bet, Exit, NextRound)
├── Exceptions/         # Custom exception types
├── Extensions/         # Service collection and utility extensions
├── IO/                 # Input/output processing and console management
├── Models/             # Data models and DTOs
├── Services/           # Business logic and game orchestration
├── Settings/           # Configuration settings classes
└── Templates/          # Console output templates

BedeLotteryConsole.Tests/
└── Unit tests mirroring the main project structure
```

### Design Principles Demonstrated

#### 🔍 Readability
- **Organized Namespace Structure** - Classes grouped by type with separated interfaces
- **Descriptive Naming** - Self-documenting class, method, and variable names
- **Channel-Based Communication** - Modern async patterns instead of traditional event listeners
- **Template-Based Output** - Consistent console formatting without string interpolation
- **Modern Solution Format** - Uses `.slnx` format for improved readability

#### 🧪 Testability
- **SOLID Principles** - Well-separated concerns enable easy unit testing
- **Dependency Injection** - Constructor injection for loose coupling
- **Static Helpers** - Appropriate use of static methods for stateless operations
- **Pure Functions** - Core algorithms isolated for independent testing
- **Snapshot Testing** - Verify UI output consistency

#### 🔧 Extensibility
- **Command Pattern** - Add new commands by implementing `ICommand`
- **Strategy Pattern** - Pluggable algorithms for shuffling and winner selection
- **Open/Closed Principle** - Extend functionality without modifying existing code
- **Template Method** - Customize output format via templates

#### ⚙️ Configurability
- **Options Pattern** - Strongly-typed configuration via `IOptions<LottoSettings>`
- **JSON Configuration** - Runtime-adjustable settings without recompilation
- **Validation** - Configuration validation on startup

### Key Technologies

- **.NET 10** - Latest .NET runtime
- **Microsoft.Extensions.Hosting** - Generic host for dependency injection and configuration
- **System.Threading.Channels** - Asynchronous producer/consumer patterns
- **Coverlet** - Code coverage analysis
- **ReportGenerator** - HTML and text coverage reports

## 🧪 Testing

### Run Tests

```bash
dotnet test
```

### Generate Coverage Report

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

cat coverage-report/Summary.txt 2>/dev/null | \
  grep -E "Line coverage|Branch coverage" || echo "Coverage data not available"
```

### Update Snapshot Tests

After verifying snapshot changes are correct:

```bash
cd BedeLotteryConsole.Tests/IO/Snapshots
for file in *.received.txt; do 
  mv "$file" "${file/.received.txt/.verified.txt}"
done
```

## 🐳 CI/CD

### Run Pipeline Locally with Podman

```bash
# Install act (one-time setup)
curl https://raw.githubusercontent.com/nektos/act/master/install.sh | sudo bash

# Configure Podman socket
export DOCKER_HOST=unix:///run/user/$(id -u)/podman/podman.sock

# Run pipeline
./bin/act
```

## 🎲 Game Logic

### Prize Distribution

Each round distributes prizes as follows:
- **Grand Prize** (50% of pot) - 1 ticket wins
- **Second Tier** (30% of pot) - Shared equally among 10% of tickets
- **Third Tier** (10% of pot) - Shared equally among 20% of tickets

**Remaining 10%** of revenue is retained by the house.

### Game Over Conditions

The game ends when:
- Player 1 has insufficient balance to purchase a ticket
- Too few players remain (increasing grand prize odds unfairly)

## 🛠️ Future Enhancements

- **Increased Test Coverage** - Target 80%+ line coverage
- **Integration Tests** - End-to-end game flow validation
- **Additional Configuration** - More customizable prize tiers and distribution
- **Persistence Layer** - Save/load game state
- **Multiplayer Network Support** - Real players across network
- **Enhanced UI** - Rich terminal UI with colors and animations

## 📝 Development Notes

> **IDE Note:** This project was developed using Visual Studio Code. While it works with any .NET-compatible IDE, some project features are optimized for VS Code.

## 📄 License

This project was created as part of a technical assessment for Bede Gaming.

---

**Ready to test your luck?** Run the application and see if fortune favors you! 🍀