using Avro.IO;
using Avro.Specific;
using Confluent.Kafka;
using OrdersAsyncContract;

namespace Producer.Extensions;

public static class OrderExtension
{
    public static Message<string, byte[]> ToKafkaMessage(this Order order)
    {
        byte[] value;
        using (var ms = new MemoryStream())
        {
            var writer = new BinaryEncoder(ms);
            var datumWriter = new SpecificDatumWriter<Order>(order.Schema);
            datumWriter.Write(order, writer);
            value = ms.ToArray();
        }
        
        var message = new Message<string, byte[]>
        {
            Key = Guid.NewGuid().ToString(),
            Value = value
        };
        return message;
    }
}