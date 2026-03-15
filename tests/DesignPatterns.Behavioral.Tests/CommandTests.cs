using DesignPatterns.Behavioral.Command;
using DesignPatterns.Behavioral.Command.Commands;
using FluentAssertions;

namespace DesignPatterns.Behavioral.Tests;

public class CommandTests
{
    private static Order CreateOrder() => new()
    {
        CustomerName = "Alice",
        Items = ["Widget A", "Widget B"],
        TotalAmount = 99.99m
    };

    [Fact]
    public void PlaceOrderCommand_PlacesOrder()
    {
        var order = CreateOrder();
        var command = new PlaceOrderCommand(order);

        command.Execute();

        order.Status.Should().Be(OrderStatus.Placed);
    }

    [Fact]
    public void PlaceOrderCommand_Undo_RestoresOriginalStatus()
    {
        var order = CreateOrder();
        var command = new PlaceOrderCommand(order);

        command.Execute();
        command.Undo();

        order.Status.Should().Be(OrderStatus.None);
    }

    [Fact]
    public void CancelOrderCommand_CancelsPlacedOrder()
    {
        var order = CreateOrder();
        new PlaceOrderCommand(order).Execute();

        var cancelCommand = new CancelOrderCommand(order);
        cancelCommand.Execute();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void UpdateShippingCommand_UpdatesAddress()
    {
        var order = CreateOrder();
        var command = new UpdateShippingCommand(order, "123 Main St");

        command.Execute();

        order.ShippingAddress.Should().Be("123 Main St");
    }

    [Fact]
    public void UpdateShippingCommand_Undo_RestoresPreviousAddress()
    {
        var order = CreateOrder();
        new UpdateShippingCommand(order, "123 Main St").Execute();

        var command = new UpdateShippingCommand(order, "456 Oak Ave");
        command.Execute();
        command.Undo();

        order.ShippingAddress.Should().Be("123 Main St");
    }

    [Fact]
    public void Invoker_TracksHistory()
    {
        var order = CreateOrder();
        var invoker = new OrderCommandInvoker();

        invoker.ExecuteCommand(new PlaceOrderCommand(order));
        invoker.ExecuteCommand(new UpdateShippingCommand(order, "123 Main St"));

        invoker.UndoCount.Should().Be(2);
        invoker.History.Should().HaveCount(2);
    }

    [Fact]
    public void Invoker_Undo_RevertsLastCommand()
    {
        var order = CreateOrder();
        var invoker = new OrderCommandInvoker();

        invoker.ExecuteCommand(new PlaceOrderCommand(order));
        invoker.ExecuteCommand(new UpdateShippingCommand(order, "123 Main St"));

        invoker.Undo();

        order.ShippingAddress.Should().BeEmpty();
        invoker.UndoCount.Should().Be(1);
        invoker.RedoCount.Should().Be(1);
    }

    [Fact]
    public void Invoker_Redo_ReappliesUndoneCommand()
    {
        var order = CreateOrder();
        var invoker = new OrderCommandInvoker();

        invoker.ExecuteCommand(new PlaceOrderCommand(order));
        invoker.ExecuteCommand(new UpdateShippingCommand(order, "123 Main St"));

        invoker.Undo();
        invoker.Redo();

        order.ShippingAddress.Should().Be("123 Main St");
    }

    [Fact]
    public void Invoker_NewCommand_ClearsRedoStack()
    {
        var order = CreateOrder();
        var invoker = new OrderCommandInvoker();

        invoker.ExecuteCommand(new PlaceOrderCommand(order));
        invoker.Undo();

        invoker.ExecuteCommand(new PlaceOrderCommand(order));

        invoker.RedoCount.Should().Be(0);
    }

    [Fact]
    public void Invoker_UndoWithNoHistory_Throws()
    {
        var invoker = new OrderCommandInvoker();
        var act = () => invoker.Undo();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Invoker_RedoWithNoHistory_Throws()
    {
        var invoker = new OrderCommandInvoker();
        var act = () => invoker.Redo();
        act.Should().Throw<InvalidOperationException>();
    }
}
