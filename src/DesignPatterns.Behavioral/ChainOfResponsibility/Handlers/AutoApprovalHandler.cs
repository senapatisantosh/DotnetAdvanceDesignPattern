namespace DesignPatterns.Behavioral.ChainOfResponsibility.Handlers;

/// <summary>
/// Automatically approves expenses under $100.
/// </summary>
public sealed class AutoApprovalHandler : ApprovalHandlerBase
{
    private const decimal AutoApprovalThreshold = 100m;

    public override ApprovalResult? Handle(ExpenseRequest request)
    {
        if (request.Amount < AutoApprovalThreshold)
        {
            return new ApprovalResult
            {
                IsApproved = true,
                ApprovedBy = "System (Auto-Approval)",
                Reason = $"Amount ${request.Amount} is below auto-approval threshold of ${AutoApprovalThreshold}.",
                Level = ApprovalLevel.Auto
            };
        }

        return base.Handle(request);
    }
}
