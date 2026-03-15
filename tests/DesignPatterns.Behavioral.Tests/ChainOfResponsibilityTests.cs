using DesignPatterns.Behavioral.ChainOfResponsibility;
using DesignPatterns.Behavioral.ChainOfResponsibility.Handlers;
using DesignPatterns.Behavioral.ChainOfResponsibility.Pipeline;
using FluentAssertions;

namespace DesignPatterns.Behavioral.Tests;

public class ChainOfResponsibilityTests
{
    private static IApprovalHandler BuildChain()
    {
        var auto = new AutoApprovalHandler();
        var manager = new ManagerApprovalHandler();
        var director = new DirectorApprovalHandler();
        var vp = new VpApprovalHandler();

        auto.SetNext(manager).SetNext(director).SetNext(vp);
        return auto;
    }

    private static ExpenseRequest CreateRequest(decimal amount, string description = "Test expense") => new()
    {
        Id = Guid.NewGuid(),
        EmployeeName = "John Doe",
        Amount = amount,
        Description = description,
        Department = "Engineering"
    };

    [Theory]
    [InlineData(50, ApprovalLevel.Auto)]
    [InlineData(99.99, ApprovalLevel.Auto)]
    public void SmallExpenses_AreAutoApproved(decimal amount, ApprovalLevel expectedLevel)
    {
        var chain = BuildChain();
        var result = chain.Handle(CreateRequest(amount));

        result.Should().NotBeNull();
        result!.IsApproved.Should().BeTrue();
        result.Level.Should().Be(expectedLevel);
    }

    [Theory]
    [InlineData(100, ApprovalLevel.Manager)]
    [InlineData(500, ApprovalLevel.Manager)]
    [InlineData(1000, ApprovalLevel.Manager)]
    public void MediumExpenses_RequireManagerApproval(decimal amount, ApprovalLevel expectedLevel)
    {
        var chain = BuildChain();
        var result = chain.Handle(CreateRequest(amount));

        result.Should().NotBeNull();
        result!.IsApproved.Should().BeTrue();
        result.Level.Should().Be(expectedLevel);
    }

    [Theory]
    [InlineData(1001, ApprovalLevel.Director)]
    [InlineData(5000, ApprovalLevel.Director)]
    [InlineData(10000, ApprovalLevel.Director)]
    public void LargeExpenses_RequireDirectorApproval(decimal amount, ApprovalLevel expectedLevel)
    {
        var chain = BuildChain();
        var result = chain.Handle(CreateRequest(amount));

        result.Should().NotBeNull();
        result!.IsApproved.Should().BeTrue();
        result.Level.Should().Be(expectedLevel);
    }

    [Fact]
    public void VeryLargeExpenses_RequireVpApproval()
    {
        var chain = BuildChain();
        var result = chain.Handle(CreateRequest(50_000));

        result.Should().NotBeNull();
        result!.IsApproved.Should().BeTrue();
        result.Level.Should().Be(ApprovalLevel.VicePresident);
    }

    [Fact]
    public void ExcessiveExpenses_AreRejectedByVp()
    {
        var chain = BuildChain();
        var result = chain.Handle(CreateRequest(150_000));

        result.Should().NotBeNull();
        result!.IsApproved.Should().BeFalse();
        result.Level.Should().Be(ApprovalLevel.Rejected);
    }

    [Fact]
    public async Task Pipeline_ExecutesAllSteps()
    {
        var persistenceStep = new PersistenceStep();
        var pipeline = new Pipeline<ExpenseRequest>()
            .AddStep(new ValidationStep())
            .AddStep(new EnrichmentStep())
            .AddStep(persistenceStep);

        var request = CreateRequest(250, "Office supplies");
        var result = await pipeline.ExecuteAsync(request);

        result.Metadata.Should().ContainKey("Validated");
        result.Metadata.Should().ContainKey("CostCenter");
        result.Metadata.Should().ContainKey("Persisted");
        persistenceStep.PersistedRequests.Should().HaveCount(1);
    }

    [Fact]
    public async Task Pipeline_ValidationStep_ThrowsOnInvalidData()
    {
        var pipeline = new Pipeline<ExpenseRequest>()
            .AddStep(new ValidationStep());

        var invalidRequest = new ExpenseRequest
        {
            Id = Guid.NewGuid(),
            EmployeeName = "John",
            Amount = -10,
            Description = "Invalid",
            Department = "Engineering"
        };

        var act = () => pipeline.ExecuteAsync(invalidRequest);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Pipeline_EnrichmentStep_SetsCorrectCostCenter()
    {
        var pipeline = new Pipeline<ExpenseRequest>()
            .AddStep(new EnrichmentStep());

        var request = CreateRequest(100) with { Department = "Marketing" };
        var result = await pipeline.ExecuteAsync(request);

        result.Metadata["CostCenter"].Should().Be("CC-2001");
    }
}
