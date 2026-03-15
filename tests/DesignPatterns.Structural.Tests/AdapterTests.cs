using DesignPatterns.Structural.Adapter;
using DesignPatterns.Structural.Adapter.ClassAdapter;
using DesignPatterns.Structural.Adapter.ThirdParty;
using FluentAssertions;

namespace DesignPatterns.Structural.Tests;

public class AdapterTests
{
    private static ShippingRequest CreateSampleRequest() => new()
    {
        OriginPostalCode = "90210",
        DestinationPostalCode = "10001",
        WeightInPounds = 10.0,
        LengthInInches = 12.0,
        WidthInInches = 8.0,
        HeightInInches = 6.0,
        ShipDate = new DateTime(2025, 6, 15),
        RequiresSignature = true,
        IsResidential = true
    };

    [Fact]
    public async Task FedExAdapter_GetQuote_TranslatesResponseToUnifiedFormat()
    {
        // Arrange
        var fedExApi = new FedExApi();
        IShippingService service = new FedExAdapter(fedExApi);

        // Act
        var quote = await service.GetQuoteAsync(CreateSampleRequest());

        // Assert
        quote.CarrierName.Should().Be("FedEx");
        quote.Price.Should().BeGreaterThan(0);
        quote.EstimatedDaysInTransit.Should().BeGreaterThan(0);
        quote.TrackingNumberPrefix.Should().Be("FDX");
        quote.ServiceLevel.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpsAdapter_GetQuote_TranslatesResponseToUnifiedFormat()
    {
        // Arrange
        var upsApi = new UpsApi();
        IShippingService service = new UpsAdapter(upsApi);

        // Act
        var quote = await service.GetQuoteAsync(CreateSampleRequest());

        // Assert
        quote.CarrierName.Should().Be("UPS");
        quote.Price.Should().BeGreaterThan(0);
        quote.EstimatedDaysInTransit.Should().BeGreaterThan(0);
        quote.TrackingNumberPrefix.Should().Be("1Z");
    }

    [Fact]
    public async Task FedExAdapter_CreateShipment_ReturnsTrackingNumber()
    {
        var service = new FedExAdapter(new FedExApi());

        var trackingNumber = await service.CreateShipmentAsync(CreateSampleRequest());

        trackingNumber.Should().StartWith("FDX");
        trackingNumber.Should().HaveLength(12);
    }

    [Fact]
    public async Task UpsAdapter_CreateShipment_ReturnsTrackingNumber()
    {
        var service = new UpsAdapter(new UpsApi());

        var trackingNumber = await service.CreateShipmentAsync(CreateSampleRequest());

        trackingNumber.Should().StartWith("1Z");
    }

    [Fact]
    public async Task FedExAdapter_GetTracking_ReturnsStatusDescription()
    {
        var service = new FedExAdapter(new FedExApi());

        var status = await service.GetTrackingStatusAsync("FDX123456789");

        status.Should().NotBeNullOrEmpty();
        status.Should().Contain("FedEx");
    }

    [Fact]
    public async Task UpsAdapter_GetTracking_ReturnsStatusDescription()
    {
        var service = new UpsAdapter(new UpsApi());

        var status = await service.GetTrackingStatusAsync("1Z12345678");

        status.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task FedExAdapter_ResidentialSignature_IncludesSurcharges()
    {
        var service = new FedExAdapter(new FedExApi());

        var withSurcharges = await service.GetQuoteAsync(CreateSampleRequest());

        var basicRequest = CreateSampleRequest() with
        {
            RequiresSignature = false,
            IsResidential = false
        };
        var withoutSurcharges = await service.GetQuoteAsync(basicRequest);

        withSurcharges.Price.Should().BeGreaterThan(withoutSurcharges.Price);
    }

    [Fact]
    public async Task ClassAdapter_GetQuote_WorksIdenticallyToObjectAdapter()
    {
        // Arrange
        IShippingService classAdapter = new FedExClassAdapter();
        IShippingService objectAdapter = new FedExAdapter(new FedExApi());
        var request = CreateSampleRequest();

        // Act
        var classQuote = await classAdapter.GetQuoteAsync(request);
        var objectQuote = await objectAdapter.GetQuoteAsync(request);

        // Assert — same pricing logic, different carrier name
        classQuote.Price.Should().Be(objectQuote.Price);
        classQuote.EstimatedDaysInTransit.Should().Be(objectQuote.EstimatedDaysInTransit);
        classQuote.CarrierName.Should().Contain("FedEx");
    }

    [Fact]
    public async Task AllAdapters_ImplementSameInterface_CanBeUsedPolymorphically()
    {
        // Arrange — all adapters behind the same interface
        IShippingService[] carriers =
        [
            new FedExAdapter(new FedExApi()),
            new UpsAdapter(new UpsApi()),
            new FedExClassAdapter()
        ];

        var request = CreateSampleRequest();

        // Act — get quotes from all carriers uniformly
        var quotes = new List<ShippingQuote>();
        foreach (var carrier in carriers)
        {
            quotes.Add(await carrier.GetQuoteAsync(request));
        }

        // Assert
        quotes.Should().HaveCount(3);
        quotes.Should().OnlyContain(q => q.Price > 0);
        quotes.Select(q => q.CarrierName).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void ShippingRequest_VolumeCalculation_IsCorrect()
    {
        var request = CreateSampleRequest();

        request.VolumeInCubicInches.Should().Be(12.0 * 8.0 * 6.0);
    }
}
