namespace DesignPatterns.Creational.Prototype;

/// <summary>
/// Prototype Registry — stores prototype instances and creates new objects by cloning them.
///
/// Instead of creating feature flag configs from scratch every time, the registry
/// stores pre-configured "template" flags. New flags are created by cloning a template
/// and customizing the clone — much faster and less error-prone than building from scratch.
/// </summary>
public sealed class FeatureFlagRegistry
{
    private readonly Dictionary<string, FeatureFlagConfig> _prototypes = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registers a prototype configuration under a key.
    /// </summary>
    public void Register(string templateKey, FeatureFlagConfig prototype)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateKey);
        ArgumentNullException.ThrowIfNull(prototype);

        // Store a deep clone so the registry's copy is independent of the caller's reference.
        _prototypes[templateKey] = prototype.DeepClone();
    }

    /// <summary>
    /// Creates a new FeatureFlagConfig by deep-cloning a registered prototype.
    /// Returns a fully independent copy that can be safely modified.
    /// </summary>
    public FeatureFlagConfig CreateFromTemplate(string templateKey)
    {
        if (!_prototypes.TryGetValue(templateKey, out var prototype))
            throw new KeyNotFoundException($"No prototype registered with key '{templateKey}'.");

        return prototype.DeepClone();
    }

    /// <summary>
    /// Creates a new FeatureFlagConfig by cloning a prototype and applying customization.
    /// </summary>
    public FeatureFlagConfig CreateFromTemplate(string templateKey, Action<FeatureFlagConfig> customize)
    {
        var clone = CreateFromTemplate(templateKey);
        customize(clone);
        return clone;
    }

    /// <summary>
    /// Checks whether a template is registered.
    /// </summary>
    public bool HasTemplate(string templateKey) => _prototypes.ContainsKey(templateKey);

    /// <summary>
    /// Returns all registered template keys.
    /// </summary>
    public IReadOnlyCollection<string> TemplateKeys => _prototypes.Keys;

    /// <summary>
    /// Removes a prototype from the registry.
    /// </summary>
    public bool Unregister(string templateKey) => _prototypes.Remove(templateKey);
}
