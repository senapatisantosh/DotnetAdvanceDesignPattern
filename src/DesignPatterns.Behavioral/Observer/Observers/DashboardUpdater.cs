namespace DesignPatterns.Behavioral.Observer.Observers;

/// <summary>
/// Observer that maintains the latest vital sign readings for a dashboard display.
/// </summary>
public sealed class DashboardUpdater : IVitalSignObserver
{
    // Tracks the latest reading per patient per vital sign type
    private readonly Dictionary<string, Dictionary<VitalSignType, VitalSignReading>> _latestReadings = [];

    public void OnVitalSignReceived(VitalSignReading reading)
    {
        if (!_latestReadings.ContainsKey(reading.PatientId))
            _latestReadings[reading.PatientId] = [];

        _latestReadings[reading.PatientId][reading.Type] = reading;
    }

    /// <summary>
    /// Gets the latest reading for a specific patient and vital sign type.
    /// </summary>
    public VitalSignReading? GetLatestReading(string patientId, VitalSignType type)
    {
        if (_latestReadings.TryGetValue(patientId, out var readings) &&
            readings.TryGetValue(type, out var reading))
        {
            return reading;
        }
        return null;
    }

    /// <summary>
    /// Gets all latest readings for a patient.
    /// </summary>
    public IReadOnlyDictionary<VitalSignType, VitalSignReading> GetPatientDashboard(string patientId)
    {
        return _latestReadings.TryGetValue(patientId, out var readings)
            ? readings.AsReadOnly()
            : new Dictionary<VitalSignType, VitalSignReading>().AsReadOnly();
    }
}
