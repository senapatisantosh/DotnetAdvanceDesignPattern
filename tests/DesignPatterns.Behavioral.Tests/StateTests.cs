using DesignPatterns.Behavioral.State;
using FluentAssertions;

namespace DesignPatterns.Behavioral.Tests;

public class StateTests
{
    private static OrderContext CreateOrder() =>
        new(Guid.NewGuid(), "Alice");

    [Fact]
    public void NewOrder_StartsInDraftState()
    {
        var order = CreateOrder();
        order.CurrentStateName.Should().Be("Draft");
    }

    [Fact]
    public void DraftOrder_CanBeSubmitted()
    {
        var order = CreateOrder();
        order.Submit();
        order.CurrentStateName.Should().Be("Submitted");
    }

    [Fact]
    public void SubmittedOrder_CanBeApproved()
    {
        var order = CreateOrder();
        order.Submit();
        order.Approve();
        order.CurrentStateName.Should().Be("Approved");
    }

    [Fact]
    public void ApprovedOrder_CanBeShipped()
    {
        var order = CreateOrder();
        order.Submit();
        order.Approve();
        order.Ship();
        order.CurrentStateName.Should().Be("Shipped");
    }

    [Fact]
    public void ShippedOrder_CanBeDelivered()
    {
        var order = CreateOrder();
        order.Submit();
        order.Approve();
        order.Ship();
        order.Deliver();
        order.CurrentStateName.Should().Be("Delivered");
    }

    [Fact]
    public void FullLifecycle_RecordsTransitions()
    {
        var order = CreateOrder();
        order.Submit();
        order.Approve();
        order.Ship();
        order.Deliver();

        order.TransitionLog.Should().HaveCount(5); // Created + 4 transitions
    }

    [Fact]
    public void DraftOrder_CanBeCancelled()
    {
        var order = CreateOrder();
        order.Cancel();
        order.CurrentStateName.Should().Be("Cancelled");
    }

    [Fact]
    public void SubmittedOrder_CanBeCancelled()
    {
        var order = CreateOrder();
        order.Submit();
        order.Cancel();
        order.CurrentStateName.Should().Be("Cancelled");
    }

    [Fact]
    public void ShippedOrder_CannotBeCancelled()
    {
        var order = CreateOrder();
        order.Submit();
        order.Approve();
        order.Ship();

        var act = () => order.Cancel();
        act.Should().Throw<InvalidOperationException>().WithMessage("*shipped*");
    }

    [Fact]
    public void DeliveredOrder_CannotBeCancelled()
    {
        var order = CreateOrder();
        order.Submit();
        order.Approve();
        order.Ship();
        order.Deliver();

        var act = () => order.Cancel();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void DraftOrder_CannotBeApproved()
    {
        var order = CreateOrder();
        var act = () => order.Approve();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void DraftOrder_CannotBeShipped()
    {
        var order = CreateOrder();
        var act = () => order.Ship();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CancelledOrder_CannotTransition()
    {
        var order = CreateOrder();
        order.Cancel();

        var submitAct = () => order.Submit();
        var approveAct = () => order.Approve();
        var cancelAct = () => order.Cancel();

        submitAct.Should().Throw<InvalidOperationException>();
        approveAct.Should().Throw<InvalidOperationException>();
        cancelAct.Should().Throw<InvalidOperationException>();
    }
}
