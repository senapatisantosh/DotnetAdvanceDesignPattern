namespace DesignPatterns.Behavioral.Observer.EventBased;

/// <summary>
/// Event-based variant of the Observer pattern using C# events.
/// More idiomatic in .NET than the classic interface-based approach.
/// </summary>
public sealed class VitalSignEventMonitor
{
    private readonly List<VitalSignReading> _readings = [];

    /// <summary>
    /// Raised whenever a new vital sign reading is recorded.
    /// </summary>
    public event EventHandler<VitalSignReadingEventArgs>? ReadingReceived;

    /// <summary>
    /// Raised only when a critical reading is detected.
    /// </summary>
    public event EventHandler<VitalSignReadingEventArgs>? CriticalReadingDetected;

    public string PatientId { get; }
    public IReadOnlyList<VitalSignReading> Readings => _readings.AsReadOnly();

    public VitalSignEventMonitor(string patientId)
    {
        PatientId = patientId;
    }

    public void RecordReading(VitalSignReading reading)
    {
        _readings.Add(reading);

        ReadingReceived?.Invoke(this, new VitalSignReadingEventArgs(reading));

        if (reading.IsCritical)
        {
            CriticalReadingDetected?.Invoke(this, new VitalSignReadingEventArgs(reading));
        }
    }
}

public sealed class VitalSignReadingEventArgs : EventArgs
{
    public VitalSignReading Reading { get; }

    public VitalSignReadingEventArgs(VitalSignReading reading)
    {
        Reading = reading;
    }
}
