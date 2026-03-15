namespace DesignPatterns.Behavioral.Memento;

/// <summary>
/// Caretaker — manages document mementos for undo/redo functionality.
/// Never inspects or modifies the memento's internal state.
/// </summary>
public sealed class DocumentHistory
{
    private readonly Document _document;
    private readonly Stack<DocumentMemento> _undoStack = new();
    private readonly Stack<DocumentMemento> _redoStack = new();

    public int UndoCount => _undoStack.Count;
    public int RedoCount => _redoStack.Count;
    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public DocumentHistory(Document document)
    {
        _document = document;
    }

    /// <summary>
    /// Saves the current state before a change is made.
    /// Call this before modifying the document.
    /// </summary>
    public void SaveState(string label = "")
    {
        _undoStack.Push(_document.Save(label));
        _redoStack.Clear(); // New change invalidates redo history
    }

    /// <summary>
    /// Undoes the last change by restoring the previous state.
    /// </summary>
    public void Undo()
    {
        if (!CanUndo)
            throw new InvalidOperationException("Nothing to undo.");

        // Save current state for redo before restoring
        _redoStack.Push(_document.Save("Before undo"));

        var memento = _undoStack.Pop();
        _document.Restore(memento);
    }

    /// <summary>
    /// Redoes a previously undone change.
    /// </summary>
    public void Redo()
    {
        if (!CanRedo)
            throw new InvalidOperationException("Nothing to redo.");

        _undoStack.Push(_document.Save("Before redo"));

        var memento = _redoStack.Pop();
        _document.Restore(memento);
    }

    /// <summary>
    /// Returns labels of all undo states (most recent first).
    /// </summary>
    public IEnumerable<string> GetUndoHistory() =>
        _undoStack.Select(m => m.Label);
}
