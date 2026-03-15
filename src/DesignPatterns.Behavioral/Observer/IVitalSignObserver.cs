namespace DesignPatterns.Behavioral.Observer;

/// <summary>
/// Observer that reacts to vital sign readings.
/// </summary>
public interface IVitalSignObserver
{
    void OnVitalSignReceived(VitalSignReading reading);
}
