namespace DesignPatterns.Behavioral.Command;

/// <summary>
/// Command interface with execute and undo support.
/// </summary>
public interface ICommand
{
    string Description { get; }
    void Execute();
    void Undo();
}
