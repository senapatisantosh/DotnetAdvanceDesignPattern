namespace DesignPatterns.Behavioral.Command;

/// <summary>
/// Invoker that executes commands and maintains history for undo/redo.
/// </summary>
public sealed class OrderCommandInvoker
{
    private readonly Stack<ICommand> _undoStack = new();
    private readonly Stack<ICommand> _redoStack = new();

    public IReadOnlyCollection<ICommand> History => _undoStack.Reverse().ToList().AsReadOnly();
    public int UndoCount => _undoStack.Count;
    public int RedoCount => _redoStack.Count;

    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        _undoStack.Push(command);
        _redoStack.Clear(); // New command invalidates redo history
    }

    public void Undo()
    {
        if (_undoStack.Count == 0)
            throw new InvalidOperationException("Nothing to undo.");

        var command = _undoStack.Pop();
        command.Undo();
        _redoStack.Push(command);
    }

    public void Redo()
    {
        if (_redoStack.Count == 0)
            throw new InvalidOperationException("Nothing to redo.");

        var command = _redoStack.Pop();
        command.Execute();
        _undoStack.Push(command);
    }
}
