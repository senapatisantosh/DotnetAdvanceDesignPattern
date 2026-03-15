namespace DesignPatterns.Enterprise.ValueObject;

/// <summary>
/// Represents an immutable inclusive date range [Start, End].
/// Useful for booking periods, promotions, subscriptions, etc.
/// </summary>
public sealed class DateRange : ValueObject, IComparable<DateRange>
{
    public DateTime Start { get; }
    public DateTime End { get; }

    public DateRange(DateTime start, DateTime end)
    {
        if (end < start)
            throw new ArgumentException($"End ({end:d}) cannot be before Start ({start:d}).");

        Start = start.Date; // Strip time component
        End = end.Date;
    }

    public int Days => (End - Start).Days + 1; // inclusive
    public TimeSpan Duration => End - Start;

    public bool Contains(DateTime date) => date.Date >= Start && date.Date <= End;

    public bool Overlaps(DateRange other) =>
        Start <= other.End && other.Start <= End;

    public DateRange? Intersect(DateRange other)
    {
        if (!Overlaps(other)) return null;
        var start = Start > other.Start ? Start : other.Start;
        var end = End < other.End ? End : other.End;
        return new DateRange(start, end);
    }

    public DateRange Merge(DateRange other)
    {
        if (!Overlaps(other) && (other.Start - End).Days > 1 && (Start - other.End).Days > 1)
            throw new InvalidOperationException("Cannot merge non-overlapping, non-adjacent date ranges.");

        var start = Start < other.Start ? Start : other.Start;
        var end = End > other.End ? End : other.End;
        return new DateRange(start, end);
    }

    public int CompareTo(DateRange? other)
    {
        if (other is null) return 1;
        var startCompare = Start.CompareTo(other.Start);
        return startCompare != 0 ? startCompare : End.CompareTo(other.End);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }

    public override string ToString() => $"{Start:yyyy-MM-dd} to {End:yyyy-MM-dd} ({Days} days)";
}
