using DesignPatterns.Behavioral.Observer;
using DesignPatterns.Behavioral.Observer.EventBased;
using DesignPatterns.Behavioral.Observer.Observers;
using FluentAssertions;

namespace DesignPatterns.Behavioral.Tests;

public class ObserverTests
{
    private static VitalSignReading CreateReading(
        VitalSignType type, double value, string unit = "bpm", string patientId = "P-001") => new()
    {
        PatientId = patientId,
        PatientName = "John Doe",
        Type = type,
        Value = value,
        Unit = unit,
        DeviceId = "DEV-100"
    };

    [Fact]
    public void AlertService_TriggersOnCriticalReading()
    {
        var monitor = new PatientMonitor("P-001", "John Doe");
        var alertService = new AlertService();
        monitor.Subscribe(alertService);

        monitor.RecordReading(CreateReading(VitalSignType.HeartRate, 160)); // Critical: > 150

        alertService.Alerts.Should().ContainSingle();
        alertService.Alerts[0].Severity.Should().Be(AlertSeverity.Critical);
    }

    [Fact]
    public void AlertService_DoesNotTriggerOnNormalReading()
    {
        var monitor = new PatientMonitor("P-001", "John Doe");
        var alertService = new AlertService();
        monitor.Subscribe(alertService);

        monitor.RecordReading(CreateReading(VitalSignType.HeartRate, 72));

        alertService.Alerts.Should().BeEmpty();
    }

    [Fact]
    public void DashboardUpdater_TracksLatestReadings()
    {
        var monitor = new PatientMonitor("P-001", "John Doe");
        var dashboard = new DashboardUpdater();
        monitor.Subscribe(dashboard);

        monitor.RecordReading(CreateReading(VitalSignType.HeartRate, 70));
        monitor.RecordReading(CreateReading(VitalSignType.HeartRate, 75));
        monitor.RecordReading(CreateReading(VitalSignType.BloodOxygen, 98, "%"));

        var latestHR = dashboard.GetLatestReading("P-001", VitalSignType.HeartRate);
        latestHR.Should().NotBeNull();
        latestHR!.Value.Should().Be(75); // Latest value

        var patientDashboard = dashboard.GetPatientDashboard("P-001");
        patientDashboard.Should().HaveCount(2);
    }

    [Fact]
    public void AuditLogger_LogsAllReadings()
    {
        var monitor = new PatientMonitor("P-001", "John Doe");
        var logger = new AuditLogger();
        monitor.Subscribe(logger);

        monitor.RecordReading(CreateReading(VitalSignType.HeartRate, 72));
        monitor.RecordReading(CreateReading(VitalSignType.Temperature, 37.0, "C"));

        logger.Log.Should().HaveCount(2);
        logger.Log[0].VitalSignType.Should().Be(VitalSignType.HeartRate);
        logger.Log[1].VitalSignType.Should().Be(VitalSignType.Temperature);
    }

    [Fact]
    public void MultipleObservers_AllReceiveNotifications()
    {
        var monitor = new PatientMonitor("P-001", "John Doe");
        var alertService = new AlertService();
        var dashboard = new DashboardUpdater();
        var logger = new AuditLogger();

        monitor.Subscribe(alertService);
        monitor.Subscribe(dashboard);
        monitor.Subscribe(logger);

        monitor.RecordReading(CreateReading(VitalSignType.BloodOxygen, 85, "%")); // Critical

        alertService.Alerts.Should().HaveCount(1);
        dashboard.GetLatestReading("P-001", VitalSignType.BloodOxygen).Should().NotBeNull();
        logger.Log.Should().HaveCount(1);
    }

    [Fact]
    public void Unsubscribe_StopsNotifications()
    {
        var monitor = new PatientMonitor("P-001", "John Doe");
        var logger = new AuditLogger();

        monitor.Subscribe(logger);
        monitor.RecordReading(CreateReading(VitalSignType.HeartRate, 72));
        monitor.Unsubscribe(logger);
        monitor.RecordReading(CreateReading(VitalSignType.HeartRate, 80));

        logger.Log.Should().HaveCount(1);
    }

    [Fact]
    public void VitalSignReading_IsCritical_DetectsAbnormalValues()
    {
        CreateReading(VitalSignType.HeartRate, 30).IsCritical.Should().BeTrue();
        CreateReading(VitalSignType.HeartRate, 75).IsCritical.Should().BeFalse();
        CreateReading(VitalSignType.BloodOxygen, 85, "%").IsCritical.Should().BeTrue();
        CreateReading(VitalSignType.BloodOxygen, 98, "%").IsCritical.Should().BeFalse();
    }

    [Fact]
    public void EventBasedMonitor_RaisesReadingReceived()
    {
        var monitor = new VitalSignEventMonitor("P-001");
        VitalSignReading? received = null;

        monitor.ReadingReceived += (_, args) => received = args.Reading;
        monitor.RecordReading(CreateReading(VitalSignType.HeartRate, 72));

        received.Should().NotBeNull();
        received!.Value.Should().Be(72);
    }

    [Fact]
    public void EventBasedMonitor_RaisesCriticalEvent_OnlyForCriticalReadings()
    {
        var monitor = new VitalSignEventMonitor("P-001");
        var criticalReadings = new List<VitalSignReading>();

        monitor.CriticalReadingDetected += (_, args) => criticalReadings.Add(args.Reading);

        monitor.RecordReading(CreateReading(VitalSignType.HeartRate, 72)); // Normal
        monitor.RecordReading(CreateReading(VitalSignType.HeartRate, 160)); // Critical

        criticalReadings.Should().ContainSingle();
        criticalReadings[0].Value.Should().Be(160);
    }
}
