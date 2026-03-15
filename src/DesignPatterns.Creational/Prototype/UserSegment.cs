namespace DesignPatterns.Creational.Prototype;

/// <summary>
/// Represents a user segment for feature flag targeting.
/// A segment defines a group of users by criteria (e.g., "Enterprise customers in US").
/// </summary>
public sealed class UserSegment
{
    public required string SegmentId { get; set; }
    public required string Name { get; set; }
    public List<string> IncludedUserIds { get; set; } = [];
    public List<string> ExcludedUserIds { get; set; } = [];
    public Dictionary<string, string> AttributeFilters { get; set; } = new();

    /// <summary>
    /// Deep clone — creates a completely independent copy of this segment.
    /// </summary>
    public UserSegment DeepClone()
    {
        return new UserSegment
        {
            SegmentId = SegmentId,
            Name = Name,
            IncludedUserIds = [.. IncludedUserIds],
            ExcludedUserIds = [.. ExcludedUserIds],
            AttributeFilters = new Dictionary<string, string>(AttributeFilters)
        };
    }
}
