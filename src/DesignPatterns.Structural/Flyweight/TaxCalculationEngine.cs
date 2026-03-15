namespace DesignPatterns.Structural.Flyweight;

/// <summary>
/// Client code that uses the Flyweight pattern to process tax calculations
/// efficiently. Demonstrates that millions of TaxCalculation contexts share
/// a small pool of TaxRate flyweight objects.
/// </summary>
public sealed class TaxCalculationEngine
{
    private readonly TaxRateFactory _factory;

    public TaxCalculationEngine(TaxRateFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <summary>
    /// Calculates tax for a single transaction.
    /// The TaxRate flyweight is retrieved (or created once) from the factory.
    /// </summary>
    public TaxCalculation CalculateTax(
        string transactionId,
        decimal taxableAmount,
        string jurisdictionCode,
        DateTime transactionDate)
    {
        // The flyweight factory ensures we reuse the same TaxRate object
        var taxRate = _factory.GetTaxRate(jurisdictionCode);

        return new TaxCalculation
        {
            TaxRate = taxRate,
            TaxableAmount = taxableAmount,
            TransactionDate = transactionDate,
            TransactionId = transactionId
        };
    }

    /// <summary>
    /// Processes a batch of transactions, demonstrating flyweight reuse.
    /// Even with thousands of transactions, only a handful of TaxRate objects exist.
    /// </summary>
    public IReadOnlyList<TaxCalculation> ProcessBatch(
        IEnumerable<(string TransactionId, decimal Amount, string JurisdictionCode, DateTime Date)> transactions)
    {
        return transactions
            .Select(t => CalculateTax(t.TransactionId, t.Amount, t.JurisdictionCode, t.Date))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Summarizes tax collected by jurisdiction across a set of calculations.
    /// </summary>
    public IReadOnlyDictionary<string, decimal> SummarizeByJurisdiction(
        IEnumerable<TaxCalculation> calculations)
    {
        return calculations
            .GroupBy(c => c.TaxRate.JurisdictionCode)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(c => c.TaxAmount));
    }

    /// <summary>Shows how many unique flyweight objects are in the pool.</summary>
    public int GetFlyweightPoolSize() => _factory.PoolSize;
}
