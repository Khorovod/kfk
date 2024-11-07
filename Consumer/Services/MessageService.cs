using Avro.IO;
using Avro.Specific;
using OrdersMessage;

namespace Consumer.Services;

public class MessageService : IMessageService
{
    public Order GetOrderFrom(byte[] data)
    {
        var userSchema = Order._SCHEMA;
        using (var stream = new MemoryStream(data))
        {
            var reader = new BinaryDecoder(stream);
            var datumReader = new SpecificDatumReader<Order>(userSchema, userSchema);

            return datumReader.Read(null!, reader);
        }
    }
}