namespace DesignPatterns.Behavioral.Observer.Observers;

/// <summary>
/// Observer that logs every vital sign reading for audit/compliance purposes.
/// </summary>
public sealed class AuditLogger : IVitalSignObserver
{
    private readonly List<AuditLogEntry> _log = [];

    public IReadOnlyList<AuditLogEntry> Log => _log.AsReadOnly();

    public void OnVitalSignReceived(VitalSignReading reading)
    {
        _log.Add(new AuditLogEntry
        {
            PatientId = reading.PatientId,
            VitalSignType = reading.Type,
            Value = reading.Value,
            Unit = reading.Unit,
            DeviceId = reading.DeviceId,
            RecordedAt = reading.Timestamp,
            IsCritical = reading.IsCritical
        });
    }
}

public sealed record AuditLogEntry
{
    public required string PatientId { get; init; }
    public required VitalSignType VitalSignType { get; init; }
    public required double Value { get; init; }
    public required string Unit { get; init; }
    public required string DeviceId { get; init; }
    public required DateTime RecordedAt { get; init; }
    public required bool IsCritical { get; init; }
}
