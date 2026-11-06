# Sudoku Console Game

A command-line Sudoku game written in C# on .NET 8. It generates a puzzle with exactly 30 pre-filled numbers, lets you fill and clear cells, ask for hints, check your grid for rule violations, and tells you when you have solved it.

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
dotnet run --project src/Sudoku.Console
```

Optional styled grid (bold yellow prefilled cells, coloured headers, separator line):

```sh
dotnet run --project src/Sudoku.Console -- --ui-enhancements
```

## Run the tests

```sh
dotnet test Sudoku.sln
```

## How to play

Rows are letters `A`–`I`, columns are numbers `1`–`9`. Empty cells are shown as `_`.

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
| `<cell> <number>` | Put a number from 1 to 9 in a cell | `B3 7` |
| `<cell> clear` | Empty a cell you filled | `C5 clear` |
| `hint` | Show one correct value for a random empty cell (it is not placed for you) | `hint` |
| `check` | Report the first rule violation found, or confirm there are none | `check` |
| `quit`, `exit` or `q` | Leave the game | `quit` |

Commands are not case-sensitive. Pre-filled cells cannot be changed, and numbers must be between 1 and 9. When the grid is completely and correctly filled you are told you have won and can press any key to start a new puzzle.

## Design in brief

The project is split in two so the game rules do not depend on the terminal:

- **`Sudoku.Core`** holds everything about the game: the grid model, validation, puzzle generation, command parsing, message text and grid rendering. It never calls `System.Console` directly.
- **`Sudoku.Console`** is the runnable host. It supplies the real console adapter, random source, and file logger; the application loop and command handling are in Core's Application layer.

Things Core needs from the outside world (reading and writing text, rendering, logging, randomness) are described as small interfaces, and `Program.cs` plugs in the real versions. In tests, fakes are plugged in instead, so a full game can be played without a keyboard.

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

1. Build a complete valid grid by trying numbers in random order and backing up when stuck (randomised backtracking).
2. Visit the 81 cells in random order and try removing each number.
3. Keep a removal only if the puzzle still has exactly one solution.
4. Stop at 30 remaining numbers. If an attempt cannot reach 30 with a unique solution, start over (up to 100 attempts, then report an error rather than return the wrong number of clues).

### How validation works

`check` scans every row, then every column, then every 3×3 box, and reports the first duplicate it finds. The game counts as finished only when every cell is filled **and** there are no duplicates.

## Assumptions

- This is standard 9×9 Sudoku.
- The brief requires 30 pre-filled cells. I additionally decided every puzzle should have **one unique solution**, so a hint is never ambiguous and "correctly filled" has one meaning.
- `hint` shows a value but does not place it, and only picks among empty cells.
- `check` reports only the first violation, in the order row, column, 3×3 box.
- The generator accepts 17–80 clues. The game always asks for 30; the range exists so the count could become a setting later. 17 is the known minimum for a unique 9×9 puzzle, and 80 keeps at least one cell empty.
- After a win, "press any key" starts a new puzzle. If input is redirected (piped), the game continues to read lines instead of waiting for a key, and ends when input ends.
- Unexpected errors are written to `logs/exceptions.log` under the current working directory, and the player sees a short generic message. Run the app from the folder containing `Sudoku.sln` to keep the log beside the solution file; inspect it with `cat logs/exceptions.log`.

## Testing approach

- **Unit tests** cover single classes: position and command parsing, cells and grid, validation, rendering, solution counting, game rules, logger.
- **Integration tests** connect several classes: the generator with the solution counter, and the game loop with a fake console.
- **End-to-end test** plays a whole game to completion through a fake console and checks that a new game starts.
- Random numbers come from a seeded source in tests, so generated puzzles are repeatable.

The code is layered with ports and adapters, but it is not strict Clean Architecture: the layers share one Core project, and application runtime classes use presentation messages and rendering abstractions.

## Test traceability

The tests below are the ones that directly cover each requirement/example from the problem statement. Run a subset with `dotnet test --filter "FullyQualifiedName~<MethodName>"`.

| Requirement / spec example | Covering test(s) |
|---|---|
| 30 pre-filled cells, rest empty (`_`) | `PuzzleGeneratorTests` (clue count); `GridRendererTests` |
| Place number in a cell (`B3 7`) | `CommandParserTests.TryParse_PlaceText_ReturnsPositionAndNumber`; `SudokuGameTests.Place_ValidMove_ReturnsAccepted` |
| Clear a cell (`C5 clear`) | `CommandParserTests.TryParse_ClearText_ReturnsPosition`; `SudokuGameTests.Clear_UserFilled_ClearsCell` |
| Hint reveals one correct value, not placed | `CommandParserTests` (HintCommand); `SudokuGameTests.Hint_ReturnsSolutionValueForEmptyCell`; `ConsoleGameRunnerTests.Hint_PrintsExpectedFormatAndDoesNotPrintMoveAccepted` |
| Check reports first violation or none | `SudokuValidatorTests` (Row/Column/Box duplicate); `ConsoleGameRunnerTests.Check_OnFreshPuzzle_PrintsNoViolations` |
| Quit at any time | `CommandParserTests` (quit/exit/q); `ConsoleGameRunnerTests.Quit_PrintsThanksAndExits` |
| Cannot modify pre-filled cells | `SudokuGameTests.Place_OnPrefilledCell_ReturnsPrefilledCellStatus`; `ConsoleGameRunnerTests.Place_OnPrefilledCell_PrintsPrefilledMessage` |
| Number must be 1–9 | `SudokuGameTests.Place_NumberOutsideRange_ReturnsInvalid`; `ConsoleGameRunnerTests.Place_NumberOutsideRange_PrintsInvalidNumberMessage` |
| **Violation example 1** – "Number 3 already exists in Row A." | `SudokuValidatorTests.RowDuplicate_ReturnsStructuredIssue` |
| **Violation example 2** – "Number 5 already exists in Column 1." | `SudokuValidatorTests.ColumnDuplicate_ReturnsStructuredIssue` |
| **Violation example 3** – "Number 8 already exists in the same 3×3 subgrid." | `SudokuValidatorTests.BoxDuplicate_ReturnsStructuredIssue` |
| Full game completes and starts a new game | `GamePlaythroughTests` |
| Puzzle has a unique solution | `SudokuSolutionCounterTests`; `PuzzleGeneratorTests` |
| Grid rendering layout | `GridRendererTests` |
| Exception logging on unexpected errors | `FileExceptionLoggerTests` |


