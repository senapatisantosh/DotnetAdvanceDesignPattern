namespace DesignPatterns.Enterprise.ValueObject;

/// <summary>
/// Base class for value objects — objects defined by their attributes rather than identity.
/// Two value objects are equal when all their components are equal.
/// Value objects should be immutable.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Subclasses return the properties that define equality.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(ValueObject? other)
    {
        if (other is null || other.GetType() != GetType())
            return false;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override bool Equals(object? obj) => Equals(obj as ValueObject);

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(17, (hash, component) =>
                HashCode.Combine(hash, component));
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);
}
