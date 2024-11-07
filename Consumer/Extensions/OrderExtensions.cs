using OrdersAsyncContract;

namespace Consumer.Extensions;

public static class OrderExtensions
{
    public static string ToStringOrder(this Order? order)
    {
        return order is null 
            ? "Empty order" 
            : $"Order: Id: {order.Id}, Time: {order.CreatedAt}, Cost: {order.TotalCost}, Items: [{string.Join(", ", order.Dishes)}]";
    }
}