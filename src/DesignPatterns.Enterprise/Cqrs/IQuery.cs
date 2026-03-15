namespace DesignPatterns.Enterprise.Cqrs;

/// <summary>
/// Marker for a read-only query that returns data without side effects.
/// </summary>
public interface IQuery<TResult> { }
