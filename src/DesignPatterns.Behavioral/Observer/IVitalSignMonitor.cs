namespace DesignPatterns.Behavioral.Observer;

/// <summary>
/// Subject interface — vital sign monitor that observers can subscribe to.
/// </summary>
public interface IVitalSignMonitor
{
    void Subscribe(IVitalSignObserver observer);
    void Unsubscribe(IVitalSignObserver observer);
    void RecordReading(VitalSignReading reading);
}
