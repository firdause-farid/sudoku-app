namespace Sudoku.Core.Application.Commands;

/// <summary>Indexes registered handlers by token patterns, then routes input through the index.</summary>
public sealed class CommandHandlerRegistry
{
    private readonly Node _root = new();

    public CommandHandlerRegistry(IEnumerable<ICommandHandler> handlers)
    {
        ArgumentNullException.ThrowIfNull(handlers);
        foreach (ICommandHandler handler in handlers)
            Register(handler);
    }

    public bool TryFind(string input, out ICommandHandler handler)
    {
        string[] tokens = input.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        ICommandHandler? found = Find(_root, tokens, 0);
        handler = found!;
        return found is not null;
    }

    private void Register(ICommandHandler handler)
    {
        string[] pattern = handler.RoutePattern.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (pattern.Length == 0)
            throw new ArgumentException("A handler route pattern cannot be empty.", nameof(handler));

        Node current = _root;
        foreach (string part in pattern)
        {
            if (IsParameter(part))
            {
                current.Parameter ??= new Node();
                current = current.Parameter;
            }
            else
            {
                if (!current.Literals.TryGetValue(part, out Node? literalNode))
                    current.Literals[part] = literalNode = new Node();
                current = literalNode;
            }
        }

        if (current.Handler is not null)
            throw new ArgumentException($"Route pattern '{handler.RoutePattern}' is already registered.", nameof(handler));
        current.Handler = handler;
    }

    private static ICommandHandler? Find(Node node, string[] tokens, int index)
    {
        if (index == tokens.Length)
            return node.Handler;

        // Literal routes take precedence over parameter routes. This keeps a command
        // such as "hint 2" from being interpreted as a cell-placement command.
        if (node.Literals.TryGetValue(tokens[index], out Node? literalNode))
            return Find(literalNode, tokens, index + 1);

        if (node.Parameter is not null)
        {
            ICommandHandler? match = Find(node.Parameter, tokens, index + 1);
            if (match is not null)
                return match;
        }

        return null;
    }

    private static bool IsParameter(string part) =>
        part.Length > 2 && part[0] == '{' && part[^1] == '}';

    private sealed class Node
    {
        public Dictionary<string, Node> Literals { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Node? Parameter { get; set; }
        public ICommandHandler? Handler { get; set; }
    }
}
