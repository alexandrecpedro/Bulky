using Bulky.Utility.Enum;

namespace Bulky.Utility;

public static class SD
{
    public static readonly string Role_Admin = RoleEnum.Admin.ToString();
    public static readonly string Role_Company = RoleEnum.Company.ToString();
    public static readonly string Role_Customer = RoleEnum.Customer.ToString();
    public static readonly string Role_Employee = RoleEnum.Employee.ToString();

    public static readonly string StatusApproved = StatusEnum.Approved.ToString();
    public static readonly string StatusCancelled = StatusEnum.Cancelled.ToString();
    public static readonly string StatusPending = StatusEnum.Pending.ToString();
    public static readonly string StatusInProcess = StatusEnum.Processing.ToString();
    public static readonly string StatusRefunded = StatusEnum.Refunded.ToString();
    public static readonly string StatusShipped = StatusEnum.Shipped.ToString();

    public static readonly string PaymentStatusApproved = PaymentStatusEnum.Approved.ToString();
    public static readonly string PaymentStatusDelayedPayment = PaymentStatusEnum.ApprovedForDelayedPayment.ToString();
    public static readonly string PaymentStatusPending = PaymentStatusEnum.Pending.ToString();
    public static readonly string PaymentStatusRejected = PaymentStatusEnum.Rejected.ToString();

    public static readonly string SessionCart = SessionCartEnum.SessionShoppingCart.ToString();
}
