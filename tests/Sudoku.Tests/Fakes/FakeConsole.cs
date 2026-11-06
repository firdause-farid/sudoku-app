using Sudoku.Core.Application.Ports;

namespace Sudoku.Tests.Fakes;

public sealed class FakeConsole : IConsole
{
    public Queue<string> Inputs { get; } = new();
    public List<string> Outputs { get; } = new();
    public int ReadKeyCalls { get; private set; }

    public void WriteLine(string? text = null) => Outputs.Add(text ?? string.Empty);

    public string? ReadLine() => Inputs.Count > 0 ? Inputs.Dequeue() : null;

    public ConsoleKeyInfo ReadKey(bool intercept = false)
    {
        ReadKeyCalls++;
        return new ConsoleKeyInfo('\0', ConsoleKey.Enter, false, false, false);
    }
}
