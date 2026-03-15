namespace DesignPatterns.Behavioral.Interpreter.Expressions;

/// <summary>
/// Terminal expression: matches a field against a value (case-insensitive contains).
/// Example: category:Electronics
/// </summary>
public sealed class FieldMatchExpression : ISearchExpression
{
    private readonly string _fieldName;
    private readonly string _value;

    public FieldMatchExpression(string fieldName, string value)
    {
        _fieldName = fieldName;
        _value = value;
    }

    public bool Interpret(Product product)
    {
        var fieldValue = product.GetField(_fieldName);
        if (fieldValue is null) return false;

        return fieldValue.Contains(_value, StringComparison.OrdinalIgnoreCase);
    }

    public override string ToString() => $"{_fieldName}:{_value}";
}
