namespace DesignPatterns.Behavioral.Observer.Observers;

/// <summary>
/// Observer that triggers alerts when vital signs are critical.
/// </summary>
public sealed class AlertService : IVitalSignObserver
{
    private readonly List<Alert> _alerts = [];

    public IReadOnlyList<Alert> Alerts => _alerts.AsReadOnly();

    public void OnVitalSignReceived(VitalSignReading reading)
    {
        if (reading.IsCritical)
        {
            _alerts.Add(new Alert
            {
                PatientId = reading.PatientId,
                PatientName = reading.PatientName,
                Message = $"CRITICAL: {reading.Type} is {reading.Value}{reading.Unit} for patient {reading.PatientName}.",
                Severity = AlertSeverity.Critical,
                Timestamp = reading.Timestamp
            });
        }
    }
}

public sealed record Alert
{
    public required string PatientId { get; init; }
    public required string PatientName { get; init; }
    public required string Message { get; init; }
    public required AlertSeverity Severity { get; init; }
    public required DateTime Timestamp { get; init; }
}

public enum AlertSeverity
{
    Info,
    Warning,
    Critical
}
