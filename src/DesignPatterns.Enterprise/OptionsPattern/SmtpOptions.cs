using System.ComponentModel.DataAnnotations;

namespace DesignPatterns.Enterprise.OptionsPattern;

/// <summary>
/// SMTP configuration options — typically bound from appsettings.json.
/// Uses DataAnnotation validation to catch misconfiguration at startup.
/// </summary>
public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    [Required(ErrorMessage = "SMTP host is required.")]
    public string Host { get; set; } = string.Empty;

    [Range(1, 65535, ErrorMessage = "Port must be between 1 and 65535.")]
    public int Port { get; set; } = 587;

    [Required(ErrorMessage = "Sender email is required.")]
    [EmailAddress(ErrorMessage = "Sender email format is invalid.")]
    public string SenderEmail { get; set; } = string.Empty;

    public string SenderName { get; set; } = "System";

    public bool UseSsl { get; set; } = true;

    public string? Username { get; set; }
    public string? Password { get; set; }

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
