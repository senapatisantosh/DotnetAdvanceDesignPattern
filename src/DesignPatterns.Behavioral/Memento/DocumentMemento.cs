namespace DesignPatterns.Behavioral.Memento;

/// <summary>
/// Memento — an immutable snapshot of document state.
/// The internal state is only accessible to the Document (originator).
/// </summary>
public sealed class DocumentMemento
{
    internal string Title { get; }
    internal string Content { get; }
    internal string FontFamily { get; }
    internal int FontSize { get; }
    public DateTime SavedAt { get; }
    public string Label { get; }

    internal DocumentMemento(string title, string content, string fontFamily, int fontSize, string label)
    {
        Title = title;
        Content = content;
        FontFamily = fontFamily;
        FontSize = fontSize;
        SavedAt = DateTime.UtcNow;
        Label = label;
    }

    public override string ToString() => $"[{SavedAt:HH:mm:ss}] {Label}";
}
