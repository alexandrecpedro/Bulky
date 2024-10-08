using Bulky.Utility.Enum;

namespace Bulky.Utility;

public static class OrderStatusManagement
{
    public static readonly string Makes_Payment = OrderStatusManagementEnum.MakesPayment.ToString();
    public static readonly string Order_Confirmation = OrderStatusManagementEnum.OrderConfirmation.ToString();
    public static readonly string Processing = OrderStatusManagementEnum.Processing.ToString();
    public static readonly string Shipped = OrderStatusManagementEnum.Shipped.ToString();

    public static readonly Dictionary<string, (string PaymentStatus, string OrderStatus)> orderStatusCustomer = new()
    {
        [Makes_Payment] = (SD.PaymentStatusPending, SD.StatusPending),
        [Order_Confirmation] = (SD.PaymentStatusApproved, SD.StatusApproved),
        [Processing] = (SD.PaymentStatusApproved, SD.StatusInProcess),
        [Shipped] = (SD.PaymentStatusApproved, SD.StatusShipped)
    };

    public static readonly Dictionary<string, (string PaymentStatus, string OrderStatus)> orderStatusCompany = new()
    {
        [Order_Confirmation] = (SD.PaymentStatusDelayedPayment, SD.StatusApproved),
        [Processing] = (SD.PaymentStatusDelayedPayment, SD.StatusInProcess),
        [Shipped] = (SD.PaymentStatusDelayedPayment, SD.StatusShipped),
        [Makes_Payment] = (SD.PaymentStatusApproved, SD.StatusShipped)
    };
}
