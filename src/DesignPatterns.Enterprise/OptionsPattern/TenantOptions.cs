namespace DesignPatterns.Enterprise.OptionsPattern;

/// <summary>
/// Per-tenant configuration. In a multi-tenant SaaS app, each tenant can have
/// different settings — connection strings, themes, quotas.
/// Named options (IOptionsSnapshot&lt;T&gt;) allow resolving config by tenant name.
/// </summary>
public sealed class TenantOptions
{
    public const string SectionName = "Tenants";

    public string TenantName { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string Theme { get; set; } = "default";
    public int MaxUsersAllowed { get; set; } = 100;
    public bool IsPremium { get; set; }
    public StorageQuota Storage { get; set; } = new();
}

public sealed class StorageQuota
{
    public long MaxStorageMb { get; set; } = 1024;
    public long UsedStorageMb { get; set; }
    public double UsagePercentage => MaxStorageMb > 0 ? (double)UsedStorageMb / MaxStorageMb * 100 : 0;
    public bool IsNearLimit => UsagePercentage >= 80;
}
