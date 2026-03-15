namespace DesignPatterns.Enterprise.Cqrs.Commands;

public sealed record ReserveInventoryCommand(string Sku, int Quantity, string OrderId) : ICommand;
