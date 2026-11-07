namespace Sudoku.Core.Application.Commands;

/// <summary>
/// Indexes registered handlers by token patterns, then routes input through the index.
/// The index matches by shape only; each route keeps its own parameter names, so different
/// routes may name the wildcard in the same position differently ("{cell} clear", "{player} rename").
/// </summary>
public sealed class CommandHandlerRegistry
{
    private readonly Node _root = new();

    public CommandHandlerRegistry(IEnumerable<ICommandHandler> handlers)
    {
        ArgumentNullException.ThrowIfNull(handlers);
        foreach (ICommandHandler handler in handlers)
            Register(handler);
    }

    /// <summary>
    /// Finds the handler whose pattern matches the input and returns the text captured by each {parameter},
    /// keyed by the names used in that handler's own pattern.
    /// </summary>
    public bool TryFind(string input, out ICommandHandler handler, out IReadOnlyDictionary<string, string> routeValues)
    {
        string[] tokens = input.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var captured = new List<string>();
        Route? route = Find(_root, tokens, 0, captured);

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (route is not null)
            for (int i = 0; i < route.ParameterNames.Count; i++)
                values[route.ParameterNames[i]] = captured[i];

        handler = route?.Handler!;
        routeValues = values;
        return route is not null;
    }

    private void Register(ICommandHandler handler)
    {
        string[] pattern = handler.RoutePattern.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (pattern.Length == 0)
            throw new ArgumentException("A handler route pattern cannot be empty.", nameof(handler));

        var parameterNames = new List<string>();
        Node current = _root;
        foreach (string part in pattern)
        {
            if (IsParameter(part))
            {
                string name = part[1..^1];
                if (parameterNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                    throw new ArgumentException(
                        $"Route pattern '{handler.RoutePattern}' uses the parameter '{name}' more than once.", nameof(handler));
                parameterNames.Add(name);
                current = current.Parameter ??= new Node();
            }
            else
            {
                if (!current.Literals.TryGetValue(part, out Node? literalNode))
                    current.Literals[part] = literalNode = new Node();
                current = literalNode;
            }
        }

        if (current.Route is not null)
            throw new ArgumentException(
                $"Route pattern '{handler.RoutePattern}' has the same shape as '{current.Route.Handler.RoutePattern}'.",
                nameof(handler));
        current.Route = new Route(handler, parameterNames);
    }

    private static Route? Find(Node node, string[] tokens, int index, List<string> captured)
    {
        if (index == tokens.Length)
            return node.Route;

        // Literal routes take precedence over parameter routes. This keeps a command
        // such as "hint 2" from being interpreted as a cell-placement command.
        if (node.Literals.TryGetValue(tokens[index], out Node? literalNode))
            return Find(literalNode, tokens, index + 1, captured);

        if (node.Parameter is not null)
        {
            captured.Add(tokens[index]);
            Route? match = Find(node.Parameter, tokens, index + 1, captured);
            if (match is not null)
                return match;
            captured.RemoveAt(captured.Count - 1);
        }

        return null;
    }

    private static bool IsParameter(string part) =>
        part.Length > 2 && part[0] == '{' && part[^1] == '}';

    private sealed record Route(ICommandHandler Handler, IReadOnlyList<string> ParameterNames);

    private sealed class Node
    {
        public Dictionary<string, Node> Literals { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Node? Parameter { get; set; }
        public Route? Route { get; set; }
    }
}
