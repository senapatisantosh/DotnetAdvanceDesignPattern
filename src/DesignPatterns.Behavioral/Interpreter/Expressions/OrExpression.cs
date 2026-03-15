namespace DesignPatterns.Behavioral.Interpreter.Expressions;

/// <summary>
/// Non-terminal expression: logical OR of two expressions.
/// </summary>
public sealed class OrExpression : ISearchExpression
{
    private readonly ISearchExpression _left;
    private readonly ISearchExpression _right;

    public OrExpression(ISearchExpression left, ISearchExpression right)
    {
        _left = left;
        _right = right;
    }

    public bool Interpret(Product product)
    {
        return _left.Interpret(product) || _right.Interpret(product);
    }

    public override string ToString() => $"({_left} OR {_right})";
}
