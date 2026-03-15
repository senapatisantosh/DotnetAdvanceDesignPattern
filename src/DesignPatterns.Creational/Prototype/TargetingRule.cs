namespace DesignPatterns.Creational.Prototype;

/// <summary>
/// Defines a targeting rule for a feature flag.
/// Rules determine which users see which variant of a feature.
/// </summary>
public sealed class TargetingRule
{
    public required string RuleId { get; set; }
    public required string Description { get; set; }

    /// <summary>Priority (lower = higher priority). Rules are evaluated in order.</summary>
    public int Priority { get; set; }

    /// <summary>The percentage of matched users who should see the feature (0-100).</summary>
    public double RolloutPercentage { get; set; } = 100.0;

    /// <summary>User segments this rule applies to.</summary>
    public List<UserSegment> TargetSegments { get; set; } = [];

    /// <summary>The variant to serve when this rule matches (e.g., "control", "variant-a").</summary>
    public string VariantKey { get; set; } = "default";

    /// <summary>
    /// Deep clone — creates a completely independent copy, including all nested segments.
    /// </summary>
    public TargetingRule DeepClone()
    {
        return new TargetingRule
        {
            RuleId = RuleId,
            Description = Description,
            Priority = Priority,
            RolloutPercentage = RolloutPercentage,
            TargetSegments = TargetSegments.Select(s => s.DeepClone()).ToList(),
            VariantKey = VariantKey
        };
    }
}
