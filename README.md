# Sudoku Console Game

A command-line Sudoku game written in C# on .NET 8. It generates uniquely solvable Sudoku puzzles, lets you fill and clear cells, ask for hints, check your grid for rule violations, and tells you when you have solved it.

## Environment

| Requirement | Detail |
|---|---|
| Operating system | Windows, Linux or macOS (no OS-specific code) |
| SDK | [.NET 8 SDK](https://dotnet.microsoft.com/download) |
| Terminal | Any terminal. ANSI colour support is only needed for the optional `--ui-enhancements` mode |

No external libraries are used for the game itself. The only packages are xUnit and its test tooling, used by the test project.

## Run the game

From the folder that contains `Sudoku.sln`:

```sh
dotnet restore Sudoku.sln
dotnet build Sudoku.sln
dotnet run --project src/Sudoku.Console
```

Optional styled grid (bold yellow prefilled cells, coloured headers, separator line):

```sh
dotnet run --project src/Sudoku.Console -- --ui-enhancements
```

Choose a grid definition and prefilled-cell count at startup:

```sh
dotnet run --project src/Sudoku.Console -- --variant mini-6x6 --prefilled-cells 18
```

The built-in default is classic 9×9. Additional definitions are JSON files in `src/Sudoku.Console/Variants`. To add a size, add a JSON file with its grid size, subgrid dimensions, symbols, row labels, minimum prefilled-cell count, and default prefilled-cell count. The project copies all matching files to the output directory, so no application source changes are needed to add another definition. Use `--variant <id>` to select one and `--prefilled-cells <count>` to override its default.

## Run the tests

```sh
dotnet test Sudoku.sln
```

## How to play

Rows and symbols depend on the selected grid definition. Columns are numbered from 1, and empty cells are shown as `_`.

```
Welcome to Sudoku!

Here is your puzzle:
    1 2 3 4 5 6 7 8 9
  A 5 3 _ _ 7 _ _ _ _
  B 6 _ _ 1 9 5 _ _ _
  ...

Enter command (e.g., A3 4, C5 clear, hint, check, quit):
A3 4

Move accepted.
```

| Command | What it does | Example |
|---|---|---|
| `<cell> <symbol>` | Put a symbol from the selected definition in a cell | `B3 7` |
| `<cell> clear` | Empty a cell you filled | `C5 clear` |
| `hint` | Show one correct value for a random empty cell (it is not placed for you) | `hint` |
| `check` | Report the first rule violation found, or confirm there are none | `check` |
| `quit` | Leave the game | `quit` |

Commands are not case-sensitive. Pre-filled cells cannot be changed, and symbols must belong to the selected definition. When the grid is completely and correctly filled you are told you have won and can press any key to start a new puzzle.

## Design in brief

The project is split in two so the game rules do not depend on the terminal:

- **`Sudoku.Core`** holds the grid model, validation, puzzle generation, command handlers, message text and grid rendering. Grid dimensions, subgrid shape, symbols, row labels, and default prefilled-cell counts come from a `SudokuDefinition`. Core never calls `System.Console` directly.
- **`Sudoku.Console`** is the runnable host. It loads variant JSON files, selects the requested definition and prefilled-cell count, and supplies the real console adapter, random source, and file logger.

Things Core needs from the outside world (reading and writing text, rendering, logging, randomness) are described as small interfaces, and `Program.cs` plugs in the real versions. In tests, fakes are plugged in instead, so a full game can be played without a keyboard.

### Extension points

- **Grid sizes and prefilled-cell defaults:** add a JSON definition under `src/Sudoku.Console/Variants`. Select it with `--variant <id>` and optionally override its default with `--prefilled-cells <count>`.
- **Commands:** implement `ICommandHandler` with a route pattern, then add it to `CreateCommandHandlers` in `Program.cs`. The route registry dispatches from the registered patterns (see *Command route guidelines*).
- **Command-line options:** see *Adding a command-line option* below.
- **Grid appearance:** implement `IGridStyle` and select it when composing the renderer.

```
src/
  Sudoku.Core/
    Application/   Command parsing, game orchestration, and the interfaces the application needs
    Domain/        Grid model, game state, validation, puzzle generation
    Presentation/  User-facing messages (.resx) and grid rendering
  Sudoku.Console/  Program entry point, console adapter, random source, file logger
tests/
  Sudoku.Tests/    Unit, Integration and EndToEnd tests
```

### How a puzzle is generated

1. Build a complete valid grid by trying symbols in random order and backing up when stuck (randomised backtracking).
2. Visit the grid cells in random order and try removing each value.
3. Keep a removal only if the puzzle still has exactly one solution.
4. Stop at the requested prefilled-cell count. If an attempt cannot reach that count with a unique solution, start over (up to 100 attempts, then report an error rather than return a puzzle with the wrong number of prefilled cells).

### How validation works

`check` scans every row, then every column, then every configured box, and reports the first duplicate it finds. The game counts as finished only when every cell is filled **and** there are no duplicates.

### Adding a command-line option

Start-up options are read in one place, `src/Sudoku.Console/Configuration/CommandLineOptionsParser.cs`, which turns the arguments into an immutable `GameOptions` record. `Program.cs` then builds the game from that record. The parser reports bad input as a message and exit code 2, never as an exception.

To add an option, for example `--seed <number>`:

1. **Add a property** to `GameOptions` (`Configuration/GameOptions.cs`), such as `int? Seed`.
2. **Add the option name** as a constant in `CommandLineOptionsParser`, next to the existing ones, and list it in `FlagOptions` or `ValueOptions`.
3. **Add one read step** in `TryParse`:
   - a flag (no value) uses `ReadFlag`, as `--ui-enhancements` does;
   - an option with a value uses `TryReadValue`, then a small private method that checks the value and returns a message if it is invalid, as `--prefilled-cells` does. Chain it into the `if` in `TryParse` with the others.
   Pass the new value into the `GameOptions` created at the end of `TryParse`.
4. **Add the error messages** to `UserMessages.resx` with a matching method in `UserMessages.cs`. Do not hard-code message text in the parser or `Program.cs`.
5. **Use the value** in the matching `Create...` method in `Program.cs` (`CreateGameFactory`, `CreateRenderer` and so on), by reading it from `GameOptions`. `Main` itself should not need to change.
6. **Add tests** in `tests/Sudoku.Tests/Unit/Configuration/CommandLineOptionsParserTests.cs`: the default when the option is absent, a valid value, an invalid value, and the option given without a value.
7. **Document it** in the *Run the game* section above.

Things to keep in mind:

- An option that depends on another (for example the prefilled count depends on the chosen variant) must be validated after the one it depends on. Keep it in the same `if` chain so the order is clear.
- Only read external resources when the option needs them. The variants folder is loaded only when `--variant` is given, so a plain `dotnet run` works without it.
- An unknown or invalid value should stop the program with a message. It should not silently fall back to a default.
- Register the new option's name in `FlagOptions` (no value) or `ValueOptions` (takes a value) in the parser. Unknown arguments, such as a typo like `--prefilled-cell`, and repeated options are rejected with exit code 2 before anything else is read, and this is what makes that check work.

### Command route guidelines

Each command handler declares a route pattern, such as `hint`, `{cell} clear` or `{cell} {number}`. Keep these guidelines in mind when adding one.

**Writing the pattern**

1. Words are separated by spaces. Extra spaces in the player's input do not matter, because input is split on whitespace.
2. A plain word is a literal. It must match exactly, ignoring case, so `hint` matches `HINT`.
3. `{name}` is a wildcard. It matches any single word and is captured under that name. It cannot hold spaces, so a value made of several words will not work.
4. A route must match every word the player typed. `hint now` does not match `hint`, so extra words mean an unrecognised command.

**What is rejected at startup**

5. An empty pattern.
6. A route with the same shape as an existing one. `{cell} clear` and `{player} clear` clash, so a route must differ from the others by at least one literal word.
7. A parameter name used twice in one route. `{cell} {cell}` is rejected; use distinct names such as `{from} {to}`.
8. Exact duplicates, such as two handlers both registering `hint`.

**How matching behaves**

9. Literals beat wildcards. If a word matches a literal at that position, that branch is taken even if a wildcard route would also fit, so `A1 clear` goes to clear and not to place.
10. There is no fallback after a literal match. If `hint` and `{cell} {number}` are both registered, `hint 5` is an unrecognised command and is not treated as a cell.
11. A leading wildcard matches any word, so `zzz 5` reaches the place handler. The handler must validate its own values. The clear handler prints its own invalid-cell message. The place handler does the same for a bad cell or an out-of-range number, and returns `CommandAction.Unrecognized` when the number is not a number at all (`A1 abc`), so the player gets the standard unrecognised-command message.

## Assumptions

- The default definition is standard 9×9 Sudoku with 30 prefilled cells. A 6×6 definition is included as an example of adding a grid through data.
- Every generated puzzle has **one unique solution**, so a hint is never ambiguous and "correctly filled" has one meaning.
- `hint` shows a value but does not place it, and only picks among empty cells.
- `check` reports only the first violation, in the order row, column, then box.
- Each definition sets a minimum prefilled-cell count and a default. The requested count must leave at least one cell empty; generation also checks uniqueness.
- After a win, "press any key" starts a new puzzle. If input is redirected (piped), the game continues to read lines instead of waiting for a key, and ends when input ends.
- Unexpected errors are written to `logs/exceptions.log` under the current working directory, and the player sees a short generic message. Run the app from the folder containing `Sudoku.sln` to keep the log beside the solution file; inspect it with `cat logs/exceptions.log`.

## Testing approach

- **Unit tests** cover single classes: positions, definitions, cells and grids, validation, rendering, solution counting, command handlers, game rules, and logger.
- **Integration tests** connect several classes: the generator with the solution counter, and the game loop with a fake console.
- **End-to-end test** plays a whole game to completion through a fake console and checks that a new game starts.
- Random numbers come from a seeded source in tests, so generated puzzles are repeatable.

The code is layered with ports and adapters, but it is not strict Clean Architecture: the layers share one Core project, and application runtime classes use presentation messages and rendering abstractions.

## Test traceability

The tests below are the ones that directly cover each requirement/example from the problem statement. Run a subset with `dotnet test --filter "FullyQualifiedName~<MethodName>"`.

| Requirement / spec example | Covering test(s) |
|---|---|
| 30 pre-filled cells, rest empty (`_`) | `PuzzleGeneratorTests` (prefilled-cell count); `GridRendererTests` |
| Place number in a cell (`B3 7`) | `PlaceCommandHandlerTests`; `CommandHandlerRegistryTests`; `SudokuGameTests.Place_ValidMove_ReturnsAccepted` |
| Clear a cell (`C5 clear`) | `ClearCommandHandlerTests`; `CommandHandlerRegistryTests`; `SudokuGameTests.Clear_UserFilled_ClearsCell` |
| Hint reveals one correct value, not placed | `HintCommandHandlerTests`; `SudokuGameTests.Hint_ReturnsSolutionValueForEmptyCell` |
| Check reports first violation or none | `CheckCommandHandlerTests`; `SudokuValidatorTests` (Row/Column/Box duplicate) |
| Quit at any time | `QuitCommandHandlerTests`; `ConsoleGameRunnerTests.Quit_PrintsThanksAndExits` |
| Cannot modify pre-filled cells | `SudokuGameTests.Place_OnPrefilledCell_ReturnsPrefilledCellStatus`; `ConsoleGameRunnerTests.Place_OnPrefilledCell_PrintsPrefilledMessage` |
| Number must be 1–9 | `SudokuGameTests.Place_NumberOutsideRange_ReturnsInvalid`; `ConsoleGameRunnerTests.Place_NumberOutsideRange_PrintsInvalidNumberMessage` |
| **Violation example 1** – "Number 3 already exists in Row A." | `SudokuValidatorTests.RowDuplicate_ReturnsStructuredIssue` |
| **Violation example 2** – "Number 5 already exists in Column 1." | `SudokuValidatorTests.ColumnDuplicate_ReturnsStructuredIssue` |
| **Violation example 3** – "Number 8 already exists in the same 3×3 subgrid." | `SudokuValidatorTests.BoxDuplicate_ReturnsStructuredIssue` |
| Full game completes and starts a new game | `GamePlaythroughTests` |
| Puzzle has a unique solution | `SudokuSolutionCounterTests`; `PuzzleGeneratorTests` |
| Grid rendering layout | `GridRendererTests` |
| Exception logging on unexpected errors | `FileExceptionLoggerTests` |

## Exception Handling and Logging

The application includes exception handling to capture unexpected errors during execution and assist with troubleshooting.

### Logging behaviour

- Unexpected exceptions are caught by the application's exception-handling mechanism.
- Exception details are written to `logs/exceptions.log`.
- The log file provides information useful for diagnosing runtime issues.
- Logging is handled separately from the core Sudoku game logic.

### Log file location

```text
Sudoku/
└── logs/
    └── exceptions.log
```

The log directory and file are created when required by the logging implementation.

### Example log entry

```text
[2026-11-06 12:00:00] ERROR
Exception: InvalidOperationException
Message: An unexpected error occurred.
StackTrace: ...
```

**Note:** Log entries are intended for troubleshooting and development purposes. The application will provide a user-friendly error message rather than exposing internal exception details directly in the console.



