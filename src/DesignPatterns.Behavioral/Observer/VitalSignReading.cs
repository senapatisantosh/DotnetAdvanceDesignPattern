namespace DesignPatterns.Behavioral.Observer;

/// <summary>
/// Represents a vital sign reading from a healthcare device.
/// </summary>
public sealed record VitalSignReading
{
    public required string PatientId { get; init; }
    public required string PatientName { get; init; }
    public required VitalSignType Type { get; init; }
    public required double Value { get; init; }
    public required string Unit { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string DeviceId { get; init; } = string.Empty;

    public bool IsCritical => Type switch
    {
        VitalSignType.HeartRate => Value < 40 || Value > 150,
        VitalSignType.BloodOxygen => Value < 90,
        VitalSignType.Temperature => Value < 35.0 || Value > 39.5,
        VitalSignType.BloodPressureSystolic => Value < 80 || Value > 180,
        VitalSignType.BloodPressureDiastolic => Value < 50 || Value > 120,
        VitalSignType.RespiratoryRate => Value < 8 || Value > 30,
        _ => false
    };
}

public enum VitalSignType
{
    HeartRate,
    BloodOxygen,
    Temperature,
    BloodPressureSystolic,
    BloodPressureDiastolic,
    RespiratoryRate
}
