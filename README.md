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

With no options you get the standard 9×9 game with 30 prefilled cells. The `Variants` folder is not needed for this.

### Options

Options go after `--` so that `dotnet run` passes them to the game:

```sh
dotnet run --project src/Sudoku.Console -- --ui-enhancements
dotnet run --project src/Sudoku.Console -- --variant mini-6x6 --prefilled-cells 18
```

| Option | What it does |
|---|---|
| `--ui-enhancements` | Styled grid: bold yellow prefilled cells, coloured headers, a separator line. Needs a terminal that understands ANSI colours. |
| `--variant <id>` | Play a different grid definition from `src/Sudoku.Console/Variants` (for example `mini-6x6` or `classic-9`). |
| `--prefilled-cells <count>` | Override the number of prefilled cells. Must be within the range allowed by the chosen variant. |

Option names are not case-sensitive. Unknown options, repeated options, an option with no value and an invalid value are all rejected with a message, and the game does not start. Additional grid definitions are JSON files; see *Writing a variant file* below.

### Exit codes

| Code | Meaning |
|---|---|
| `0` | The session ended normally, including after `quit` or end-of-input. |
| `1` | A puzzle could not be generated, or an unhandled application error ended the run. Details are in `logs/exceptions.log`. |
| `2` | The command-line options were invalid. The reason is printed and nothing is logged. |

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

Commands are not case-sensitive. Pre-filled cells cannot be changed, and symbols must belong to the selected definition. Anything else gets "Unrecognized command" and the current grid. When the grid is completely and correctly filled you are told you have won and can press any key to start a new puzzle.

## Design in brief

The project is split in two so the game rules do not depend on the terminal:

- **`Sudoku.Core`** holds everything about the game: the grid model, validation, puzzle generation, the commands, the game loop, message text and grid rendering. Grid dimensions, subgrid shape, symbols, row labels and default prefilled-cell counts come from a `SudokuDefinition`. Core never calls `System.Console` directly.
- **`Sudoku.Console`** is the runnable host. It reads the command-line options, loads variant JSON files, and supplies the real console, random source and file logger. `Program.cs` is the one place where the real implementations are chosen.

Things Core needs from the outside world (reading and writing text, rendering, logging, randomness) are described as small interfaces ("ports"), and `Program.cs` plugs in the real versions. In tests, fakes are plugged in instead, so a full game can be played without a keyboard.

```
Sudoku.sln
Directory.Build.props         Settings shared by all projects
src/
  Sudoku.Core/                The game. No direct console access.
    Application/
      Commands/               ICommandHandler, the route registry, CommandAction
        Handlers/             One handler per command: place, clear, hint, check, quit
      Ports/                  What Core needs from outside: IConsole, IRenderer, IExceptionLogger
      Runtime/                ConsoleGameRunner (the game loop) and CommandDispatcher
    Domain/
      Model/                  SudokuDefinition, Position, Grid, Cell, constraint checking
      Game/                   SudokuGame, its factory, move and hint results
      Generation/             Puzzle generator, solution generator, solution counter
      Validation/             Validator and validation results
      Randomness/             IRandomSource
    Presentation/
      Messages/               All player-facing text (UserMessages.resx)
      Rendering/              GridRenderer and the grid styles
  Sudoku.Console/             The program
    Adapters/                 Real console and random source
    Configuration/            Command-line parsing and the variant catalog
    Logging/                  File exception logger
    Variants/                 Grid definitions (JSON)
tests/
  Sudoku.Tests/               Unit, Integration and EndToEnd tests
```

### What happens when you type a command

1. `ConsoleGameRunner` reads the line and gives it to the `CommandDispatcher`.
2. The dispatcher asks the `CommandHandlerRegistry` which handler matches the words typed. Matching is by the handler's route pattern (`hint`, `{cell} clear`, `{cell} {number}`).
3. The matching handler runs. It validates its own values (cell, number), calls the game (`SudokuGame.Place`, `Clear`, `Hint` or `Check`), and prints the outcome. The game returns result objects; it never prints.
4. If nothing matches, or the handler says the input is not a command, the dispatcher prints "Unrecognized command" and the grid.
5. Back in the runner: a quit ends the session; an accepted move that completes the grid shows the win message and starts a new game.

### How a puzzle is generated

1. Build a complete valid grid by trying symbols in random order and backing up when stuck (randomised backtracking).
2. Visit the grid cells in random order and try removing each value.
3. Keep a removal only if the puzzle still has exactly one solution.
4. Stop at the requested prefilled-cell count. If an attempt cannot reach that count with a unique solution, start over (up to 100 attempts, then report an error rather than return a puzzle with the wrong number of prefilled cells).

The solution is kept by the game so that `hint` can show a correct value.

### How validation works

`check` scans every row, then every column, then every subgrid, and reports the first duplicate it finds. The game counts as finished only when every cell is filled **and** there are no duplicates. One method, `SudokuConstraintChecker.TryFindFirstViolation`, decides the rules and their order; both the validator and the solution counter use it.

### Design principles in practice

| Principle | Where it shows |
|---|---|
| Single responsibility | The registry routes, handlers execute, the game holds the rules, the validator validates, the renderer draws, the runner loops, the options parser parses, the logger logs. |
| Open/closed | A new command is a handler plus one registration line in `Program.cs`; it does not require changes to existing handlers or the registry. A new grid size is a JSON file. A new look is an `IGridStyle` implementation. A new command-line option requires a `GameOptions` property and parser changes, then its consumer must use the new value. |
| Liskov | The fake console and the real console are interchangeable behind `IConsole`; the same holds for the other ports. |
| Interface segregation | Small interfaces. The renderer and validator receive `IGridView`, which is read-only. |
| Dependency inversion | Core depends on interfaces. `Program.cs` chooses the concrete classes. |
| DRY | One rule for the prefilled-cell range, one rule for value range, one scan for violations, one place for shared message output. |

The code is layered with ports and adapters, but it is not strict Clean Architecture: the layers share one Core project, and application runtime classes use presentation messages and rendering abstractions.

### Extension points

- **Grid sizes and prefilled-cell defaults:** add a JSON definition. See *Writing a variant file*.
- **Commands:** implement `ICommandHandler` with a route pattern, then add it to `CreateCommandHandlers` in `Program.cs`. See *Command route guidelines*.
- **Command-line options:** see *Adding a command-line option*.
- **Grid appearance:** implement `IGridStyle` and select it where the renderer is created (`CreateRenderer` in `Program.cs`).

### Writing a variant file

A variant is one JSON file in `src/Sudoku.Console/Variants`. The file name does not matter, only the `id` inside it. For example, `mini-6x6.json`:

```json
{
  "id": "mini-6x6",
  "gridSize": 6,
  "subgridRows": 2,
  "subgridColumns": 3,
  "symbols": ["1", "2", "3", "4", "5", "6"],
  "rowLabels": ["A", "B", "C", "D", "E", "F"],
  "minimumPrefilledCellCount": 8,
  "defaultPrefilledCellCount": 18
}
```

All fields are required, and the property names are not case-sensitive.

**Rules the program checks.** A file that breaks one of these stops the game with the generic unexpected-error message, and the reason is written to `logs/exceptions.log`:

| Field | Rule |
|---|---|
| `id` | Not blank. Unique across all files, ignoring case. `--variant` matches it ignoring case. |
| `gridSize` | At least 2. This is the number of rows, columns and distinct values. |
| `subgridRows`, `subgridColumns` | Both greater than 0, and `subgridRows × subgridColumns` must equal `gridSize` (a 6×6 grid uses 2×3 or 3×2, a 9×9 grid uses 3×3). |
| `symbols` | Exactly `gridSize` entries. None blank. All different, ignoring case. None contains a space, and none is `_` or `clear` (any case), because the game already uses those. The first symbol is value 1, the second is value 2, and so on. |
| `rowLabels` | Exactly `gridSize` entries. None blank. All different, ignoring case. None contains a digit or a space. They are listed from the top row to the bottom row. |
| `minimumPrefilledCellCount` | At least 1 and fewer than `gridSize × gridSize`. |
| `defaultPrefilledCellCount` | Not below the minimum, and fewer than `gridSize × gridSize` so at least one cell is empty. |

**Conventions the program does not check.** Breaking these will not stop the game from starting, but the game may behave badly:

- **Row labels should be letters, one character each.** A cell is typed as the row label followed directly by the column number, such as `B3`, and columns are always the numbers `1` to `gridSize`. (Labels with digits or spaces are rejected, because they could not be typed reliably.) A label that starts with another label (`A` and `AB`) is confusing. Multi-character labels also push the grid out of alignment, because the header spacing assumes one character.
- **Symbols should be single characters, normally `1` to `gridSize` in order.**
  - (`clear`, `_` and symbols containing spaces are rejected, because the game already uses them.)
  - The "Number must be between 1 and N" message assumes numeric symbols, so non-numeric symbols give a misleading message when someone types a plain number.
- **Keep the grid small.** Only 6×6 and 9×9 are covered by tests. Past 9 columns, column numbers have two digits and the grid no longer lines up. The puzzle generator is simple, so large grids may take a long time to generate.
- **Keep the default prefilled count comfortably above the minimum.** Counts close to the lowest possible are hard to reach with a unique solution, and if the generator gives up after 100 attempts, the player sees "Unable to start a new game." The classic file uses 17 as the minimum, which is the lowest known for a unique 9×9 puzzle.
- **Choose an `id` with no spaces** and not starting with `--`, because it is typed on the command line after `--variant`.

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
- `check` reports only the first violation, in the order row, column, then subgrid.
- `quit` is the only quit command. There are no hidden aliases such as `exit` or `q`. The game also ends when its input ends (for example a piped file).
- Each definition sets a minimum prefilled-cell count and a default. The requested count must leave at least one cell empty; generation also checks uniqueness.
- After a win, "press any key" starts a new puzzle. If input is redirected (piped), the game continues to read lines instead of waiting for a key, and ends when input ends.
- The log is written under the current working directory (see *Exception handling and logging*). Run the app from the folder containing `Sudoku.sln` to keep the log beside the solution file.

## Exception handling and logging

Player mistakes are normal feedback, not errors. Only unexpected exceptions are logged.

| Situation | Logged | What the player sees |
|---|---|---|
| Unknown command, bad cell, number out of range, move on a prefilled cell | No | A short message and the current grid |
| Invalid command-line options | No | The reason on the error stream; exit code 2 |
| An unexpected exception while running a command | Yes ("Handling a player command") | "An unexpected error occurred. Details were written to the application log." The game continues. |
| A puzzle cannot be generated (at the start or after a win) | Yes ("Starting a new game") | "Unable to start a new game. Please try again later." The game ends with exit code 1. |
| A key cannot be read for the replay pause | Yes | Nothing. The game carries on without the pause. |
| Anything else outside command handling, such as a broken variant file while loading a requested variant | Yes ("Unhandled application exception") | The generic message above on the error stream; exit code 1 |

- **Where:** `logs/exceptions.log`, inside the folder the program was started from. The folder is created on first use, and `logs/` is excluded from source control. Inspect it with `cat logs/exceptions.log`.
- **Format:** each entry starts with a UTC timestamp and a short description of what the program was doing, followed by the full exception with its stack trace. New entries are appended.

```text
[2026-11-08T07:15:30.1234567+00:00] Handling a player command
System.InvalidOperationException: Demo
   at Sudoku.Core.Application.Commands.Handlers.HintCommandHandler.Handle(...)
   ...
```

- **Safe by design:** if the log file cannot be written (for example a read-only folder), the logger reports that on the error stream and carries on, so logging can never cause a second crash.
- **Broken variant files:** the variants catalog is loaded when `--variant` is supplied. It reads every JSON file before starting the game, so a broken file prevents that variant run from starting, even if the requested file itself is valid. The error names the bad file, for example "Sudoku definition file 'bad.json' is not valid: …", and the run exits with code 1. The default game does not load the variants folder.

## Testing approach

- **Unit tests** cover single classes: positions, definitions, grids and cells, validation, rendering, solution counting, command handlers, the route registry, the options parser, the variant catalog, game rules and the logger.
- **Integration tests** connect several classes: the generator with the solution counter, and the game loop with a fake console.
- **End-to-end test** plays a whole game to completion through a fake console and checks that a new game starts.
- Random numbers come from a seeded source in tests, so generated puzzles are repeatable.
- The test project can see Core's and Console's internal types (`InternalsVisibleTo`), so small internal pieces such as `Cell` and the options parser are tested directly.

## Test traceability

The tests below are the ones that directly cover each requirement or example from the problem statement, plus the extensions. Run a subset with `dotnet test --filter "FullyQualifiedName~<MethodName>"`.

| Requirement / spec example | Covering test(s) |
|---|---|
| 30 pre-filled cells, rest empty (`_`) | `PuzzleGeneratorTests` (prefilled-cell count); `GridRendererTests` |
| Place number in a cell (`B3 7`) | `PlaceCommandHandlerTests`; `CommandHandlerRegistryTests`; `SudokuGameTests.Place_ValidMove_ReturnsAccepted` |
| Clear a cell (`C5 clear`) | `ClearCommandHandlerTests`; `CommandHandlerRegistryTests`; `SudokuGameTests.Clear_UserFilled_ClearsCell` |
| Hint reveals one correct value, not placed | `HintCommandHandlerTests`; `SudokuGameTests.Hint_ReturnsSolutionValueForEmptyCell` |
| Check reports first violation or none | `CheckCommandHandlerTests`; `SudokuValidatorTests` (Row/Column/Box duplicate) |
| Quit at any time | `QuitCommandHandlerTests`; `ConsoleGameRunnerTests.Quit_PrintsThanksAndExits` |
| Cannot modify pre-filled cells | `SudokuGameTests.Place_OnPrefilledCell_ReturnsPrefilledCellStatus`; `ConsoleGameRunnerTests.Place_OnPrefilledCell_PrintsPrefilledMessage` |
| Number must be in range (1–9 by default) | `SudokuGameTests.Place_NumberOutsideRange_ReturnsInvalid`; `ConsoleGameRunnerTests.Place_NumberOutsideRange_PrintsInvalidNumberMessage` |
| **Violation example 1** – "Number 3 already exists in Row A." | `SudokuValidatorTests.RowDuplicate_ReturnsStructuredIssue` |
| **Violation example 2** – "Number 5 already exists in Column 1." | `SudokuValidatorTests.ColumnDuplicate_ReturnsStructuredIssue` |
| **Violation example 3** – "Number 8 already exists in the same 3×3 subgrid." | `SudokuValidatorTests.BoxDuplicate_ReturnsStructuredIssue` |
| Full game completes and starts a new game | `GamePlaythroughTests` |
| Puzzle has a unique solution | `SudokuSolutionCounterTests`; `PuzzleGeneratorTests` |
| Grid rendering layout | `GridRendererTests` |
| Exception logging on unexpected errors | `FileExceptionLoggerTests`; `ConsoleGameRunnerTests` |
| Command routing rules (literals first, wildcards, duplicates) | `CommandHandlerRegistryTests` |
| Command-line options (defaults, unknown, repeated, ranges) | `CommandLineOptionsParserTests` |
| Variant files (loading, rules, naming the bad file) | `SudokuDefinitionCatalogTests`; `SudokuDefinitionTests` |
| A 6×6 puzzle generates with the right clue count and a unique solution | `PuzzleGeneratorTests.Generate_UsesConfiguredSixBySixDimensionsAndPrefilledCellCount` |
