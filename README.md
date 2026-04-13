# Aya Chess Engine

Aya is a modern chess engine written in C# (.NET 9.0). It implements the Universal Chess Interface (UCI) protocol, allowing it to be used with various chess GUIs like Arena, Cute Chess, or the included Python-based visualizer.

## Features

### Search
- **Alpha-Beta Pruning**: Efficiently prunes the search tree to explore deeper lines.
- **Move Ordering**: Prioritizes moves using **MVV-LVA** (Most Valuable Victim - Least Valuable Attacker) and promotion scoring to improve pruning efficiency.
- **Opening Book**: Support for predefined opening sequences to ensure a strong start.

### Evaluation
- **Tapered Evaluation**: Smoothly transitions between opening and endgame evaluation parameters based on the material remaining on the board.
- **Piece-Square Tables (PST)**: Position-based scoring for all pieces to encourage developmental and strategic play.
- **Material Weights**: Fine-tuned values for pawns, knights, bishops, rooks, and queens.

### Protocol
- **UCI Compatible**: Supports standard commands including `uci`, `isready`, `position`, and `go depth`.

## Project Structure

- `src/Aya/`: The core engine library containing board representation, move generation, search, and evaluation logic.
- `src/Aya.CLI/`: The console application that handles UCI communication.
- `tests/Aya.Tests/`: A comprehensive xUnit test suite covering move generation and search accuracy.
- `tools/Aya.GUI/`: A Python-based GUI for quick visualization and manual testing.

## Getting Started

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Python 3.x (optional, for the GUI)

### Building the Engine
To build the engine, run the following command from the root directory:
```bash
dotnet build
```

### Running the Engine
You can run the CLI directly to interact with the engine via UCI:
```bash
dotnet run --project src/Aya.CLI
```

### Running Tests
To ensure everything is working correctly:
```bash
dotnet test
```

### Using the GUI
The project includes a simple Python GUI located in `tools/Aya.GUI`. To use it, ensure you have `pygame` and `python-chess` installed:
```bash
pip install pygame python-chess
python tools/Aya.GUI/main.py
```

## License
This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

## Author
- **Lucas Campos**
