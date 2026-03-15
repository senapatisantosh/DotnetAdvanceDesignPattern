using Microsoft.Extensions.Options;

namespace DesignPatterns.Enterprise.OptionsPattern;

/// <summary>
/// Demonstrates the three Options interfaces and when to use each:
///
/// IOptions&lt;T&gt;       — Singleton, read once at startup, never refreshes.
/// IOptionsSnapshot&lt;T&gt; — Scoped, re-reads config each request. Supports named options.
/// IOptionsMonitor&lt;T&gt;  — Singleton, but pushes change notifications.
/// </summary>
public sealed class OptionsExample
{
    /// <summary>
    /// Example service that uses IOptions — good for config that never changes after startup.
    /// </summary>
    public sealed class EmailService(IOptions<SmtpOptions> smtpOptions)
    {
        private readonly SmtpOptions _config = smtpOptions.Value;

        public string GetSmtpHost() => _config.Host;
        public int GetSmtpPort() => _config.Port;
        public string GetSenderEmail() => _config.SenderEmail;
        public bool IsSslEnabled() => _config.UseSsl;
    }

    /// <summary>
    /// Example service that uses IOptionsSnapshot — good for per-request config reads
    /// and named options (e.g., per-tenant config).
    /// </summary>
    public sealed class TenantService(IOptionsSnapshot<TenantOptions> tenantOptions)
    {
        public TenantOptions GetTenantConfig(string tenantName) =>
            tenantOptions.Get(tenantName);

        public bool IsPremiumTenant(string tenantName) =>
            tenantOptions.Get(tenantName).IsPremium;

        public int GetMaxUsers(string tenantName) =>
            tenantOptions.Get(tenantName).MaxUsersAllowed;
    }

    /// <summary>
    /// Example service that uses IOptionsMonitor — good for long-lived singletons
    /// that need to react to config changes (feature flags, circuit breakers).
    /// </summary>
    public sealed class FeatureFlagService : IDisposable
    {
        private readonly IOptionsMonitor<FeatureFlags> _monitor;
        private readonly IDisposable? _changeListener;
        private readonly List<string> _changeLog = new();

        public IReadOnlyList<string> ChangeLog => _changeLog.AsReadOnly();

        public FeatureFlagService(IOptionsMonitor<FeatureFlags> monitor)
        {
            _monitor = monitor;
            _changeListener = _monitor.OnChange((flags, _) =>
            {
                _changeLog.Add($"Feature flags changed at {DateTime.UtcNow:O}: " +
                               $"NewCheckout={flags.EnableNewCheckout}, DarkMode={flags.EnableDarkMode}");
            });
        }

        public bool IsNewCheckoutEnabled() => _monitor.CurrentValue.EnableNewCheckout;
        public bool IsDarkModeEnabled() => _monitor.CurrentValue.EnableDarkMode;
        public bool IsMaintenanceMode() => _monitor.CurrentValue.MaintenanceMode;
        public bool IsUserInBeta(string userId) => _monitor.CurrentValue.IsUserInBeta(userId);

        public void Dispose() => _changeListener?.Dispose();
    }
}
