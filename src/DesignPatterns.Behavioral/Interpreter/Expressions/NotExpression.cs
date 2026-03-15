namespace DesignPatterns.Behavioral.Interpreter.Expressions;

/// <summary>
/// Non-terminal expression: logical NOT (negation) of an expression.
/// </summary>
public sealed class NotExpression : ISearchExpression
{
    private readonly ISearchExpression _expression;

    public NotExpression(ISearchExpression expression)
    {
        _expression = expression;
    }

    public bool Interpret(Product product)
    {
        return !_expression.Interpret(product);
    }

    public override string ToString() => $"NOT ({_expression})";
}
