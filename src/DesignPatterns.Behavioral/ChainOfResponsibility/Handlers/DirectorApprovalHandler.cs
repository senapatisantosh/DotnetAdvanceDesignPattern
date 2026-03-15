namespace DesignPatterns.Behavioral.ChainOfResponsibility.Handlers;

/// <summary>
/// Director can approve expenses between $1,000 and $10,000.
/// </summary>
public sealed class DirectorApprovalHandler : ApprovalHandlerBase
{
    private const decimal MaxAmount = 10_000m;
    private readonly string _directorName;

    public DirectorApprovalHandler(string directorName = "Bob Johnson (Director)")
    {
        _directorName = directorName;
    }

    public override ApprovalResult? Handle(ExpenseRequest request)
    {
        if (request.Amount <= MaxAmount)
        {
            return new ApprovalResult
            {
                IsApproved = true,
                ApprovedBy = _directorName,
                Reason = $"Director approved expense of ${request.Amount} for '{request.Description}'.",
                Level = ApprovalLevel.Director
            };
        }

        return base.Handle(request);
    }
}
