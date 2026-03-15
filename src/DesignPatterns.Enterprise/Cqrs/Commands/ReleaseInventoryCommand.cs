namespace DesignPatterns.Enterprise.Cqrs.Commands;

public sealed record ReleaseInventoryCommand(string Sku, int Quantity, string Reason) : ICommand;
