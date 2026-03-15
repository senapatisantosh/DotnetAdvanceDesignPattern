namespace DesignPatterns.Behavioral.Mediator;

public sealed record CheckoutRequest
{
    public required Guid OrderId { get; init; }
    public required string CustomerId { get; init; }
    public required List<CheckoutItem> Items { get; init; }
    public required string ShippingAddress { get; init; }
    public required string PaymentMethod { get; init; }
    public required decimal TotalAmount { get; init; }
}

public sealed record CheckoutItem
{
    public required string ProductId { get; init; }
    public required string ProductName { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
}

public sealed record CheckoutResult
{
    public required bool Success { get; init; }
    public required Guid OrderId { get; init; }
    public string? TrackingNumber { get; init; }
    public string? PaymentTransactionId { get; init; }
    public string? FailureReason { get; init; }
    public List<string> Steps { get; init; } = [];
}
