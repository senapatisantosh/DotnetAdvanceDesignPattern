namespace DesignPatterns.Behavioral.ChainOfResponsibility;

/// <summary>
/// The outcome of processing an expense request through the approval chain.
/// </summary>
public sealed record ApprovalResult
{
    public required bool IsApproved { get; init; }
    public required string ApprovedBy { get; init; }
    public required string Reason { get; init; }
    public ApprovalLevel Level { get; init; }
}

public enum ApprovalLevel
{
    Auto,
    Manager,
    Director,
    VicePresident,
    Rejected
}
