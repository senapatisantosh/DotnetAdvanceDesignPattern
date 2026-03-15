namespace DesignPatterns.Creational.FactoryMethod;

/// <summary>
/// Represents the outcome of a payment processing attempt.
/// Uses a record for value semantics and immutability.
/// </summary>
public sealed record PaymentResult
{
    public bool IsSuccess { get; init; }
    public string TransactionId { get; init; } = string.Empty;
    public string Gateway { get; init; } = string.Empty;
    public decimal AmountCharged { get; init; }
    public string Currency { get; init; } = "USD";
    public string? ErrorMessage { get; init; }
    public DateTimeOffset ProcessedAt { get; init; } = DateTimeOffset.UtcNow;

    public static PaymentResult Success(string transactionId, string gateway, decimal amount, string currency = "USD")
        => new()
        {
            IsSuccess = true,
            TransactionId = transactionId,
            Gateway = gateway,
            AmountCharged = amount,
            Currency = currency
        };

    public static PaymentResult Failure(string gateway, string error)
        => new()
        {
            IsSuccess = false,
            Gateway = gateway,
            ErrorMessage = error
        };
}
