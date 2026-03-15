namespace DesignPatterns.Behavioral.Interpreter.Expressions;

/// <summary>
/// Non-terminal expression: logical AND of two expressions.
/// </summary>
public sealed class AndExpression : ISearchExpression
{
    private readonly ISearchExpression _left;
    private readonly ISearchExpression _right;

    public AndExpression(ISearchExpression left, ISearchExpression right)
    {
        _left = left;
        _right = right;
    }

    public bool Interpret(Product product)
    {
        return _left.Interpret(product) && _right.Interpret(product);
    }

    public override string ToString() => $"({_left} AND {_right})";
}
