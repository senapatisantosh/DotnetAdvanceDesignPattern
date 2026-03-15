namespace DesignPatterns.Enterprise.Cqrs;

/// <summary>Marker for a command that mutates state and returns no value.</summary>
public interface ICommand { }

/// <summary>Marker for a command that mutates state and returns a result.</summary>
public interface ICommand<TResult> { }
