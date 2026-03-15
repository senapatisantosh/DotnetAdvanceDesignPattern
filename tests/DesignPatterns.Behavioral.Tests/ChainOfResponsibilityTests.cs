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

        auto.SetNext(manager);
        manager.SetNext(director);
        director.SetNext(vp);

        return auto;
    }

    private static ExpenseRequest CreateRequest(decimal amount) => new()
    {
        Id = Guid.NewGuid(),
        EmployeeName = "Alice",
        Amount = amount,
        Description = "Conference travel",
        Department = "Engineering"
    };

    [Fact]
    public void Expense_Under100_AutoApproved()
    {
        var chain = BuildChain();
        var result = chain.Handle(CreateRequest(50m));

        result.Should().NotBeNull();
        result!.IsApproved.Should().BeTrue();
        result.Level.Should().Be(ApprovalLevel.Auto);
    }

    [Fact]
    public void Expense_500_ApprovedByManager()
    {
        var chain = BuildChain();
        var result = chain.Handle(CreateRequest(500m));

        result.Should().NotBeNull();
        result!.IsApproved.Should().BeTrue();
        result.Level.Should().Be(ApprovalLevel.Manager);
    }

    [Fact]
    public void Expense_5000_ApprovedByDirector()
    {
        var chain = BuildChain();
        var result = chain.Handle(CreateRequest(5_000m));

        result.Should().NotBeNull();
        result!.IsApproved.Should().BeTrue();
        result.Level.Should().Be(ApprovalLevel.Director);
    }

    [Fact]
    public void Expense_50000_ApprovedByVP()
    {
        var chain = BuildChain();
        var result = chain.Handle(CreateRequest(50_000m));

        result.Should().NotBeNull();
        result!.IsApproved.Should().BeTrue();
        result.Level.Should().Be(ApprovalLevel.VicePresident);
    }

    [Fact]
    public void Expense_Over100000_Rejected()
    {
        var chain = BuildChain();
        var result = chain.Handle(CreateRequest(200_000m));

        result.Should().NotBeNull();
        result!.IsApproved.Should().BeFalse();
        result.Level.Should().Be(ApprovalLevel.Rejected);
    }

    [Fact]
    public async Task Pipeline_ProcessesAllSteps()
    {
        var persistence = new PersistenceStep();
        var pipeline = new Pipeline<ExpenseRequest>()
            .AddStep(new ValidationStep())
            .AddStep(new EnrichmentStep())
            .AddStep(persistence);

        var request = CreateRequest(250m);
        var result = await pipeline.ExecuteAsync(request);

        result.Metadata.Should().ContainKey("Validated");
        result.Metadata.Should().ContainKey("CostCenter");
        result.Metadata.Should().ContainKey("Persisted");
        persistence.PersistedRequests.Should().HaveCount(1);
    }

    [Fact]
    public async Task Pipeline_ValidationStep_RejectsInvalidAmount()
    {
        var pipeline = new Pipeline<ExpenseRequest>()
            .AddStep(new ValidationStep());

        var request = CreateRequest(-10m);

        var act = () => pipeline.ExecuteAsync(request);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Pipeline_EnrichmentStep_AssignsCostCenter()
    {
        var pipeline = new Pipeline<ExpenseRequest>()
            .AddStep(new EnrichmentStep());

        var request = CreateRequest(100m);
        var result = await pipeline.ExecuteAsync(request);

        result.Metadata.Should().ContainKey("CostCenter");
        result.Metadata["CostCenter"].Should().Be("CC-1001");
    }
}
