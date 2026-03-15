namespace DesignPatterns.Enterprise.ValueObject;

/// <summary>
/// Represents a monetary amount with its currency.
/// Prevents accidental arithmetic between different currencies.
/// </summary>
public sealed class Money : ValueObject, IComparable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        Amount = Math.Round(amount, 2);
        Currency = currency.ToUpperInvariant();
    }

    // ── Factory methods ────────────────────────────────────────────────
    public static Money USD(decimal amount) => new(amount, "USD");
    public static Money EUR(decimal amount) => new(amount, "EUR");
    public static Money GBP(decimal amount) => new(amount, "GBP");
    public static Money Zero(string currency) => new(0m, currency);

    // ── Arithmetic ─────────────────────────────────────────────────────
    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor) => new(Amount * factor, Currency);

    public bool IsPositive => Amount > 0;
    public bool IsZero => Amount == 0;
    public bool IsNegative => Amount < 0;

    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money money, decimal factor) => money.Multiply(factor);
    public static bool operator >(Money left, Money right) => left.CompareTo(right) > 0;
    public static bool operator <(Money left, Money right) => left.CompareTo(right) < 0;
    public static bool operator >=(Money left, Money right) => left.CompareTo(right) >= 0;
    public static bool operator <=(Money left, Money right) => left.CompareTo(right) <= 0;

    public int CompareTo(Money? other)
    {
        if (other is null) return 1;
        EnsureSameCurrency(other);
        return Amount.CompareTo(other.Amount);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"Cannot operate on different currencies: {Currency} vs {other.Currency}.");
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}
