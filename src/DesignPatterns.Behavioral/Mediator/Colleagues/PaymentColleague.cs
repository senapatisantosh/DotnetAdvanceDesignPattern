namespace DesignPatterns.Behavioral.Mediator.Colleagues;

/// <summary>
/// Handles payment processing. Only interacts with the mediator, not other colleagues.
/// </summary>
public sealed class PaymentColleague
{
    private readonly bool _shouldFail;
    private readonly List<PaymentRecord> _processedPayments = [];

    public PaymentColleague(bool shouldFail = false)
    {
        _shouldFail = shouldFail;
    }

    public IReadOnlyList<PaymentRecord> ProcessedPayments => _processedPayments.AsReadOnly();

    public Task<(bool Success, string TransactionId, string Message)> ProcessPaymentAsync(
        string customerId, decimal amount, string paymentMethod)
    {
        if (_shouldFail)
        {
            return Task.FromResult((false, string.Empty, "Payment declined by processor."));
        }

        var transactionId = $"TXN-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";
        _processedPayments.Add(new PaymentRecord(customerId, amount, paymentMethod, transactionId));

        return Task.FromResult((true, transactionId, $"Payment of ${amount} processed successfully."));
    }

    public Task RefundAsync(string transactionId)
    {
        // Simulate refund
        return Task.CompletedTask;
    }
}

public sealed record PaymentRecord(
    string CustomerId,
    decimal Amount,
    string PaymentMethod,
    string TransactionId);
