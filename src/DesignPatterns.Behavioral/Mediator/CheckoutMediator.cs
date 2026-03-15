using DesignPatterns.Behavioral.Mediator.Colleagues;

namespace DesignPatterns.Behavioral.Mediator;

/// <summary>
/// Concrete mediator that orchestrates checkout between inventory, payment,
/// shipping, and notification colleagues. None of these colleagues know about each other.
/// </summary>
public sealed class CheckoutMediator : ICheckoutMediator
{
    private readonly InventoryColleague _inventory;
    private readonly PaymentColleague _payment;
    private readonly ShippingColleague _shipping;
    private readonly NotificationColleague _notification;
    private readonly List<string> _steps = [];

    public CheckoutMediator(
        InventoryColleague inventory,
        PaymentColleague payment,
        ShippingColleague shipping,
        NotificationColleague notification)
    {
        _inventory = inventory;
        _payment = payment;
        _shipping = shipping;
        _notification = notification;
    }

    public async Task<CheckoutResult> ProcessCheckoutAsync(CheckoutRequest request)
    {
        _steps.Clear();

        // Step 1: Reserve inventory
        NotifyStepCompleted("Mediator", "Starting inventory reservation...");
        var (inventoryOk, inventoryMsg) = await _inventory.ReserveItemsAsync(request.Items);
        NotifyStepCompleted("Inventory", inventoryMsg);

        if (!inventoryOk)
        {
            await _notification.SendInventoryFailureAsync(request.CustomerId, inventoryMsg);
            NotifyStepCompleted("Notification", "Customer notified of inventory failure.");

            return new CheckoutResult
            {
                Success = false,
                OrderId = request.OrderId,
                FailureReason = inventoryMsg,
                Steps = [.. _steps]
            };
        }

        // Step 2: Process payment
        NotifyStepCompleted("Mediator", "Starting payment processing...");
        var (paymentOk, transactionId, paymentMsg) =
            await _payment.ProcessPaymentAsync(request.CustomerId, request.TotalAmount, request.PaymentMethod);
        NotifyStepCompleted("Payment", paymentMsg);

        if (!paymentOk)
        {
            // Compensate: release inventory
            await _inventory.ReleaseReservationAsync(request.Items);
            NotifyStepCompleted("Inventory", "Inventory reservation released due to payment failure.");

            await _notification.SendPaymentFailureAsync(request.CustomerId, paymentMsg);
            NotifyStepCompleted("Notification", "Customer notified of payment failure.");

            return new CheckoutResult
            {
                Success = false,
                OrderId = request.OrderId,
                FailureReason = paymentMsg,
                Steps = [.. _steps]
            };
        }

        // Step 3: Create shipment
        NotifyStepCompleted("Mediator", "Creating shipment...");
        var (_, trackingNumber, shippingMsg) =
            await _shipping.CreateShipmentAsync(request.OrderId, request.ShippingAddress, request.Items);
        NotifyStepCompleted("Shipping", shippingMsg);

        // Step 4: Send confirmation
        await _notification.SendOrderConfirmationAsync(request.CustomerId, request.OrderId, trackingNumber);
        NotifyStepCompleted("Notification", "Order confirmation sent to customer.");

        return new CheckoutResult
        {
            Success = true,
            OrderId = request.OrderId,
            TrackingNumber = trackingNumber,
            PaymentTransactionId = transactionId,
            Steps = [.. _steps]
        };
    }

    public void NotifyStepCompleted(string colleague, string message)
    {
        _steps.Add($"[{colleague}] {message}");
    }
}
