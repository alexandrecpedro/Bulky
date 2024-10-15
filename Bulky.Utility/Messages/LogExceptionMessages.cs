namespace Bulky.Utility.Messages;

public static class LogExceptionMessages
{
    // Category
    public static readonly string CategoryIdNotFoundException = "Category ID not found!";
    public static readonly string CategoryNotFoundException = "Category not found!";

    // Company
    public static readonly string CompanyNotFoundException = "Company not found!";
    public static readonly string CompanyDeleteException = "Error while deleting company!";

    // Operation
    public static readonly string LockUnlockException = "Error while locking/Unlocking!";

    // Order
    public static readonly string OrderIdNotFoundException = "Order ID not found!";
    public static readonly string OrderNotFoundException = "Order not found!";

    // OrderDetail
    public static readonly string OrderDetailListNotFoundException = "Order detail list not found!";

    // OrderHeader
    public static readonly string OrderHeaderIdInvalidDataException = "Invalid order header ID!";
    public static readonly string OrderHeaderInvalidDataException = "Invalid order header!";
    public static readonly string OrderHeaderNotFoundException = "Order header not found!";

    // Product
    public static readonly string ProductIdNotFoundException = "Product ID not found!";
    public static readonly string ProductNotFoundException = "Product not found!";

    // ShoppingCart
    public static readonly string ShoppingCartIdNotFoundException = "Shopping cart ID not found!";
    public static readonly string ShoppingCartNotFoundException = "Shopping cart not found!";

    // User
    public static readonly string UserIdNotFoundException = "User ID not found!";
    public static readonly string UserNotFoundException = "User not found!";
}
