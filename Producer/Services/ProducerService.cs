using Confluent.Kafka;
using Microsoft.Extensions.Options;
using OrdersAsyncContract;
using Producer.Extensions;
using Producer.Infrastructure;

namespace Producer.Services;

public class ProducerService : IHostedService, IDisposable
{
    private readonly IProducer<string, byte[]> _producer;
    private readonly KafkaConfiguration _kafkaConfiguration;
    private int _id;
    
    public ProducerService(IOptions<KafkaConfiguration> kafkaConfigurationOptions)
    {
        _kafkaConfiguration = kafkaConfigurationOptions?.Value ?? throw new ArgumentException(nameof(kafkaConfigurationOptions));

        _producer = ConfigureProducer();
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var rnd = new Random();
        if (!cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine("Producer is starting...");

            try
            {
                for (var i = 0; i < 10; i++)
                {
                    var order = new Order()
                    {
                        Id = _id++,
                        CreatedAt = DateTime.Now,
                        Dishes = GetDishes(),
                        TotalCost = rnd.Next(1000, 10000),
                    };
                
                    await Produce(order, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Producer is stopping...");

        _producer.Flush(cancellationToken);

        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
    
    private async Task Produce(Order order, CancellationToken cancellationToken)
    {
        try
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                var msg = order.ToKafkaMessage();
                await _producer.ProduceAsync(_kafkaConfiguration.Topic, msg, cancellationToken);
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
        }
    }
    
    private IProducer<string, byte[]> ConfigureProducer()
    {
        Console.WriteLine($"Configure producing on: {_kafkaConfiguration.Brokers}");
        
        var config = new ProducerConfig()
        {
            BootstrapServers = _kafkaConfiguration.Brokers,
            SecurityProtocol = SecurityProtocol.Plaintext,
            EnableDeliveryReports = false,
            QueueBufferingMaxMessages = 10000000,
            QueueBufferingMaxKbytes = 100000000,
            BatchNumMessages = 500,
            //Acks = Acks.All,
        };

        return new ProducerBuilder<string, byte[]>(config).Build();
    }

    private static string[] GetDishes()
    {
        var dishes = new List<string> { "Сасиcка", "Булочька", "Чай", "Пряники", "Муровьед", "Фольцвагн" };

        int index = new Random().Next(dishes.Count);
        return dishes.GetRange(index, dishes.Count - index).ToArray();
    }
}