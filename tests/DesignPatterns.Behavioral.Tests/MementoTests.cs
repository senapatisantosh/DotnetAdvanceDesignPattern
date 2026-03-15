using DesignPatterns.Behavioral.Memento;
using FluentAssertions;

namespace DesignPatterns.Behavioral.Tests;

public class MementoTests
{
    [Fact]
    public void Save_CapturesCurrentState()
    {
        var doc = new Document { Title = "My Doc", Content = "Hello", FontFamily = "Arial", FontSize = 12 };
        var memento = doc.Save("Initial save");

        // Modify the document
        doc.Content = "Changed";
        doc.FontSize = 16;

        // Verify memento captured original state (via restore)
        doc.Restore(memento);
        doc.Content.Should().Be("Hello");
        doc.FontSize.Should().Be(12);
    }

    [Fact]
    public void Restore_RevertsToSavedState()
    {
        var doc = new Document { Title = "Report", Content = "Original content" };
        var memento = doc.Save();

        doc.Title = "Changed Title";
        doc.Content = "New content";
        doc.FontFamily = "Times New Roman";

        doc.Restore(memento);

        doc.Title.Should().Be("Report");
        doc.Content.Should().Be("Original content");
        doc.FontFamily.Should().Be("Arial");
    }

    [Fact]
    public void DocumentHistory_Undo_RestoresPreviousState()
    {
        var doc = new Document { Title = "Draft" };
        var history = new DocumentHistory(doc);

        history.SaveState("Before edit");
        doc.Content = "First paragraph";

        history.SaveState("Before second edit");
        doc.Content = "Second paragraph";

        history.Undo();
        doc.Content.Should().Be("First paragraph");

        history.Undo();
        doc.Content.Should().BeEmpty();
    }

    [Fact]
    public void DocumentHistory_Redo_ReappliesUndoneChange()
    {
        var doc = new Document { Title = "Draft" };
        var history = new DocumentHistory(doc);

        history.SaveState();
        doc.Content = "Some content";

        history.Undo();
        doc.Content.Should().BeEmpty();

        history.Redo();
        doc.Content.Should().Be("Some content");
    }

    [Fact]
    public void DocumentHistory_NewChange_ClearsRedoStack()
    {
        var doc = new Document();
        var history = new DocumentHistory(doc);

        history.SaveState();
        doc.Content = "Version 1";

        history.Undo();
        history.CanRedo.Should().BeTrue();

        history.SaveState();
        doc.Content = "Version 2";

        history.CanRedo.Should().BeFalse();
    }

    [Fact]
    public void DocumentHistory_UndoWithNoHistory_Throws()
    {
        var doc = new Document();
        var history = new DocumentHistory(doc);

        var act = () => history.Undo();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void DocumentHistory_RedoWithNoHistory_Throws()
    {
        var doc = new Document();
        var history = new DocumentHistory(doc);

        var act = () => history.Redo();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void DocumentHistory_GetUndoHistory_ReturnsLabels()
    {
        var doc = new Document();
        var history = new DocumentHistory(doc);

        history.SaveState("First edit");
        doc.Content = "A";

        history.SaveState("Second edit");
        doc.Content = "B";

        history.GetUndoHistory().Should().ContainInOrder("Second edit", "First edit");
    }
}
