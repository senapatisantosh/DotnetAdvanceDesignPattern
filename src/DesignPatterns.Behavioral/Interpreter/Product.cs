namespace DesignPatterns.Behavioral.Interpreter;

/// <summary>
/// A product with searchable fields — the context for our interpreter.
/// </summary>
public sealed record Product
{
    public required string Name { get; init; }
    public required string Category { get; init; }
    public required string Brand { get; init; }
    public required decimal Price { get; init; }
    public required string Color { get; init; }
    public Dictionary<string, string> Attributes { get; init; } = [];

    /// <summary>
    /// Gets a field value by name for dynamic search expression evaluation.
    /// </summary>
    public string? GetField(string fieldName) => fieldName.ToLowerInvariant() switch
    {
        "name" => Name,
        "category" => Category,
        "brand" => Brand,
        "price" => Price.ToString(),
        "color" => Color,
        _ => Attributes.GetValueOrDefault(fieldName)
    };
}
