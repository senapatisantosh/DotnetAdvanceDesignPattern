namespace DesignPatterns.Shared;

/// <summary>
/// Every pattern demo implements this so the Playground can discover and run them.
/// </summary>
public interface IPatternDemo
{
    string PatternName { get; }
    string Category { get; }
    string Description { get; }
    Task RunAsync();
}
