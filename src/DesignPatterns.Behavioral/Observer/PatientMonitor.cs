namespace DesignPatterns.Behavioral.Observer;

/// <summary>
/// Concrete Subject — monitors a patient's vital signs and notifies all observers.
/// </summary>
public sealed class PatientMonitor : IVitalSignMonitor
{
    private readonly List<IVitalSignObserver> _observers = [];
    private readonly List<VitalSignReading> _readings = [];

    public string PatientId { get; }
    public string PatientName { get; }
    public IReadOnlyList<VitalSignReading> Readings => _readings.AsReadOnly();
    public int ObserverCount => _observers.Count;

    public PatientMonitor(string patientId, string patientName)
    {
        PatientId = patientId;
        PatientName = patientName;
    }

    public void Subscribe(IVitalSignObserver observer)
    {
        if (!_observers.Contains(observer))
            _observers.Add(observer);
    }

    public void Unsubscribe(IVitalSignObserver observer)
    {
        _observers.Remove(observer);
    }

    public void RecordReading(VitalSignReading reading)
    {
        _readings.Add(reading);

        // Notify all observers — each decides independently how to react
        foreach (var observer in _observers.ToList()) // ToList to allow modification during iteration
        {
            observer.OnVitalSignReceived(reading);
        }
    }
}
