namespace DesignPatterns.Enterprise.Specification;

/// <summary>
/// Abstract base with built-in composite operators.
/// Subclasses only need to implement <see cref="IsSatisfiedBy"/>.
/// </summary>
public abstract class Specification<T> : ISpecification<T>
{
    public abstract bool IsSatisfiedBy(T candidate);

    public ISpecification<T> And(ISpecification<T> other) => new AndSpecification<T>(this, other);
    public ISpecification<T> Or(ISpecification<T> other) => new OrSpecification<T>(this, other);
    public ISpecification<T> Not() => new NotSpecification<T>(this);

    /// <summary>Allows using specifications as predicates via implicit conversion.</summary>
    public static implicit operator Func<T, bool>(Specification<T> spec) => spec.IsSatisfiedBy;
}

internal sealed class AndSpecification<T>(ISpecification<T> left, ISpecification<T> right) : Specification<T>
{
    public override bool IsSatisfiedBy(T candidate) =>
        left.IsSatisfiedBy(candidate) && right.IsSatisfiedBy(candidate);

    public override string ToString() => $"({left} AND {right})";
}

internal sealed class OrSpecification<T>(ISpecification<T> left, ISpecification<T> right) : Specification<T>
{
    public override bool IsSatisfiedBy(T candidate) =>
        left.IsSatisfiedBy(candidate) || right.IsSatisfiedBy(candidate);

    public override string ToString() => $"({left} OR {right})";
}

internal sealed class NotSpecification<T>(ISpecification<T> inner) : Specification<T>
{
    public override bool IsSatisfiedBy(T candidate) => !inner.IsSatisfiedBy(candidate);

    public override string ToString() => $"(NOT {inner})";
}
