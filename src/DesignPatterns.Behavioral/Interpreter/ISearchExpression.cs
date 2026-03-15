namespace DesignPatterns.Behavioral.Interpreter;

/// <summary>
/// Abstract expression in the search query DSL.
/// Each expression can evaluate whether a product matches.
/// </summary>
public interface ISearchExpression
{
    bool Interpret(Product product);
}
