namespace DesignPatterns.Behavioral.ChainOfResponsibility.Handlers;

/// <summary>
/// VP is the final approver for expenses over $10,000.
/// If the VP cannot approve (e.g., exceeds company policy), the request is rejected.
/// </summary>
public sealed class VpApprovalHandler : ApprovalHandlerBase
{
    private const decimal AbsoluteMax = 100_000m;
    private readonly string _vpName;

    public VpApprovalHandler(string vpName = "Alice Williams (VP)")
    {
        _vpName = vpName;
    }

    public override ApprovalResult? Handle(ExpenseRequest request)
    {
        if (request.Amount <= AbsoluteMax)
        {
            return new ApprovalResult
            {
                IsApproved = true,
                ApprovedBy = _vpName,
                Reason = $"VP approved large expense of ${request.Amount} for '{request.Description}'.",
                Level = ApprovalLevel.VicePresident
            };
        }

        // Beyond even VP authority — reject
        return new ApprovalResult
        {
            IsApproved = false,
            ApprovedBy = _vpName,
            Reason = $"Expense of ${request.Amount} exceeds maximum company policy limit of ${AbsoluteMax}.",
            Level = ApprovalLevel.Rejected
        };
    }
}
