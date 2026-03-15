using DesignPatterns.Behavioral.Command;
using DesignPatterns.Behavioral.Command.Commands;
using FluentAssertions;

namespace DesignPatterns.Behavioral.Tests;

public class CommandTests
{
    private static Order CreateOrder() => new()
    {
        CustomerName = "John Doe",
        Items = ["Widget A", "Widget B"],
        TotalAmount = 99.99m
    };

    [Fact]
    public void PlaceOrderCommand_ExecutesAndUndoes()
    {
        var order = CreateOrder();
        var command = new PlaceOrderCommand(order);

        command.Execute();
        order.Status.Should().Be(OrderStatus.Placed);

        command.Undo();
        order.Status.Should().Be(OrderStatus.None);
    }

    [Fact]
    public void CancelOrderCommand_ExecutesAndUndoes()
    {
        var order = CreateOrder();
        order.Place();

        var command = new CancelOrderCommand(order);
        command.Execute();
        order.Status.Should().Be(OrderStatus.Cancelled);

        command.Undo();
        order.Status.Should().Be(OrderStatus.Placed);
    }

    [Fact]
    public void UpdateShippingCommand_ExecutesAndUndoes()
    {
        var order = CreateOrder();
        var command = new UpdateShippingCommand(order, "123 Main St");

        command.Execute();
        order.ShippingAddress.Should().Be("123 Main St");

        command.Undo();
        order.ShippingAddress.Should().BeEmpty();
    }

    [Fact]
    public void Invoker_MaintainsHistory()
    {
        var order = CreateOrder();
        var invoker = new OrderCommandInvoker();

        invoker.ExecuteCommand(new PlaceOrderCommand(order));
        invoker.ExecuteCommand(new UpdateShippingCommand(order, "456 Oak Ave"));

        invoker.UndoCount.Should().Be(2);
        invoker.History.Should().HaveCount(2);
    }

    [Fact]
    public void Invoker_UndoAndRedo()
    {
        var order = CreateOrder();
        var invoker = new OrderCommandInvoker();

        invoker.ExecuteCommand(new PlaceOrderCommand(order));
        invoker.ExecuteCommand(new UpdateShippingCommand(order, "456 Oak Ave"));

        invoker.Undo(); // Undo shipping update
        order.ShippingAddress.Should().BeEmpty();
        invoker.RedoCount.Should().Be(1);

        invoker.Redo(); // Redo shipping update
        order.ShippingAddress.Should().Be("456 Oak Ave");
        invoker.RedoCount.Should().Be(0);
    }

    [Fact]
    public void Invoker_NewCommand_ClearsRedoStack()
    {
        var order = CreateOrder();
        var invoker = new OrderCommandInvoker();

        invoker.ExecuteCommand(new PlaceOrderCommand(order));
        invoker.Undo();
        invoker.RedoCount.Should().Be(1);

        invoker.ExecuteCommand(new PlaceOrderCommand(order));
        invoker.RedoCount.Should().Be(0);
    }

    [Fact]
    public void Invoker_UndoEmpty_Throws()
    {
        var invoker = new OrderCommandInvoker();
        var act = () => invoker.Undo();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Invoker_RedoEmpty_Throws()
    {
        var invoker = new OrderCommandInvoker();
        var act = () => invoker.Redo();
        act.Should().Throw<InvalidOperationException>();
    }
}
