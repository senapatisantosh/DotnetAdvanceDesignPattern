namespace DesignPatterns.Creational.Prototype;

/// <summary>
/// A complex feature flag configuration that supports deep cloning.
///
/// Why Prototype here? When setting up A/B tests, you often start with an existing
/// feature flag configuration and clone it to create a variant. The clone must be
/// completely independent — modifying the clone's targeting rules must NOT affect
/// the original.
///
/// This class demonstrates both shallow and deep cloning to illustrate the difference.
/// </summary>
public sealed class FeatureFlagConfig
{
    public required string FlagKey { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }

    /// <summary>Default rollout percentage for users not matched by any targeting rule.</summary>
    public double DefaultRolloutPercentage { get; set; }

    /// <summary>Targeting rules evaluated in priority order.</summary>
    public List<TargetingRule> TargetingRules { get; set; } = [];

    /// <summary>Available variants (e.g., {"control": "old UI", "variant-a": "new UI"}).</summary>
    public Dictionary<string, string> Variants { get; set; } = new();

    /// <summary>Arbitrary metadata tags.</summary>
    public Dictionary<string, string> Tags { get; set; } = new();

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastModifiedAt { get; set; }

    /// <summary>
    /// SHALLOW CLONE — copies value types and references.
    /// Collections are shared: modifying a clone's list modifies the original's list too.
    /// This is almost never what you want for complex objects.
    /// </summary>
    public FeatureFlagConfig ShallowClone()
    {
        return (FeatureFlagConfig)MemberwiseClone();
    }

    /// <summary>
    /// DEEP CLONE — creates a fully independent copy.
    /// Every collection and nested object is recursively cloned.
    /// Modifying the clone has zero effect on the original.
    /// </summary>
    public FeatureFlagConfig DeepClone()
    {
        return new FeatureFlagConfig
        {
            FlagKey = FlagKey,
            Name = Name,
            Description = Description,
            IsEnabled = IsEnabled,
            DefaultRolloutPercentage = DefaultRolloutPercentage,
            TargetingRules = TargetingRules.Select(r => r.DeepClone()).ToList(),
            Variants = new Dictionary<string, string>(Variants),
            Tags = new Dictionary<string, string>(Tags),
            CreatedAt = CreatedAt,
            LastModifiedAt = LastModifiedAt
        };
    }

    /// <summary>
    /// Creates a clone configured as an A/B test variant.
    /// Demonstrates a domain-specific clone operation that also modifies the clone.
    /// </summary>
    public FeatureFlagConfig CloneAsAbTestVariant(string variantName, double rolloutPercentage)
    {
        var clone = DeepClone();
        clone.FlagKey = $"{FlagKey}-{variantName}";
        clone.Name = $"{Name} ({variantName})";
        clone.DefaultRolloutPercentage = rolloutPercentage;
        clone.LastModifiedAt = DateTimeOffset.UtcNow;
        clone.Tags["ab-test-variant"] = variantName;
        clone.Tags["ab-test-parent"] = FlagKey;
        return clone;
    }
}
