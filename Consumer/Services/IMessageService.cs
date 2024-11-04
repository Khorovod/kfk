using OrdersMessage;

namespace Consumer.Services;

public interface IMessageService
{
    Order GetOrderFrom(byte[] data);
}