namespace DesignPatterns.Behavioral.Memento;

/// <summary>
/// Originator — a rich text document whose state can be saved and restored.
/// </summary>
public sealed class Document
{
    public string Title { get; set; } = "Untitled";
    public string Content { get; set; } = string.Empty;
    public string FontFamily { get; set; } = "Arial";
    public int FontSize { get; set; } = 12;

    /// <summary>
    /// Creates a memento capturing the current document state.
    /// </summary>
    public DocumentMemento Save(string label = "")
    {
        var effectiveLabel = string.IsNullOrWhiteSpace(label)
            ? $"Snapshot of '{Title}'"
            : label;

        return new DocumentMemento(Title, Content, FontFamily, FontSize, effectiveLabel);
    }

    /// <summary>
    /// Restores the document to a previously saved state.
    /// </summary>
    public void Restore(DocumentMemento memento)
    {
        Title = memento.Title;
        Content = memento.Content;
        FontFamily = memento.FontFamily;
        FontSize = memento.FontSize;
    }

    public override string ToString() =>
        $"Document(Title='{Title}', Content='{Content}', Font={FontFamily} {FontSize}pt)";
}
