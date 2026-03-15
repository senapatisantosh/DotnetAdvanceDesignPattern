namespace DesignPatterns.Behavioral.ChainOfResponsibility;

/// <summary>
/// Defines a handler in the expense approval chain.
/// Each handler either processes the request or passes it to the next handler.
/// </summary>
public interface IApprovalHandler
{
    /// <summary>
    /// Sets the next handler in the chain.
    /// Returns the next handler for fluent chaining.
    /// </summary>
    IApprovalHandler SetNext(IApprovalHandler next);

    /// <summary>
    /// Attempts to handle the expense request.
    /// Returns null if this handler cannot process it (passes to next).
    /// </summary>
    ApprovalResult? Handle(ExpenseRequest request);
}

/// <summary>
/// Base class providing default chain-forwarding behavior.
/// </summary>
public abstract class ApprovalHandlerBase : IApprovalHandler
{
    private IApprovalHandler? _next;

    public IApprovalHandler SetNext(IApprovalHandler next)
    {
        _next = next;
        return next;
    }

    public virtual ApprovalResult? Handle(ExpenseRequest request)
    {
        return _next?.Handle(request);
    }
}
