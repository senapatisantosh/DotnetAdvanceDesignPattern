namespace DesignPatterns.Enterprise.Specification;

/// <summary>
/// Encapsulates a boolean business rule that can be evaluated against a candidate.
/// Specifications compose with And / Or / Not to build complex filter trees.
/// </summary>
public interface ISpecification<T>
{
    bool IsSatisfiedBy(T candidate);
    ISpecification<T> And(ISpecification<T> other);
    ISpecification<T> Or(ISpecification<T> other);
    ISpecification<T> Not();
}
