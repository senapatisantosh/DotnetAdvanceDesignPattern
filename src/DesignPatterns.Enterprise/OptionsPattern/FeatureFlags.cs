namespace DesignPatterns.Enterprise.OptionsPattern;

/// <summary>
/// Feature flags that can be toggled at runtime without redeployment.
/// Use IOptionsMonitor to pick up changes without restarting the app.
/// </summary>
public sealed class FeatureFlags
{
    public const string SectionName = "Features";

    public bool EnableNewCheckout { get; set; }
    public bool EnableDarkMode { get; set; }
    public bool EnableBetaSearch { get; set; }
    public bool MaintenanceMode { get; set; }
    public int MaxUploadSizeMb { get; set; } = 10;
    public List<string> BetaUserIds { get; set; } = new();

    public bool IsUserInBeta(string userId) =>
        EnableBetaSearch && BetaUserIds.Contains(userId, StringComparer.OrdinalIgnoreCase);
}
