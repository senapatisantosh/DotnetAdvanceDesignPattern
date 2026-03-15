namespace DesignPatterns.Behavioral.ChainOfResponsibility.Handlers;

/// <summary>
/// Manager can approve expenses between $100 and $1,000.
/// </summary>
public sealed class ManagerApprovalHandler : ApprovalHandlerBase
{
    private const decimal MaxAmount = 1_000m;
    private readonly string _managerName;

    public ManagerApprovalHandler(string managerName = "Jane Smith (Manager)")
    {
        _managerName = managerName;
    }

    public override ApprovalResult? Handle(ExpenseRequest request)
    {
        if (request.Amount <= MaxAmount)
        {
            return new ApprovalResult
            {
                IsApproved = true,
                ApprovedBy = _managerName,
                Reason = $"Manager approved expense of ${request.Amount} for '{request.Description}'.",
                Level = ApprovalLevel.Manager
            };
        }

        return base.Handle(request);
    }
}
