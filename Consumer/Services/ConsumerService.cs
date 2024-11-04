using Confluent.Kafka;
using Consumer.Extensions;
using Consumer.Infrastructure;
using Microsoft.Extensions.Options;

namespace Consumer.Services;

public class ConsumerService : IHostedService, IDisposable
{
    private readonly KafkaConfiguration _kafkaConfiguration;
    private readonly IConsumer<Null, byte[]> _consumer;
    private readonly IMessageService _messageService;

    public ConsumerService(IOptions<KafkaConfiguration> kafkaConfigurationOptions)
    {
        _kafkaConfiguration = kafkaConfigurationOptions?.Value ?? throw new ArgumentException(nameof(kafkaConfigurationOptions));
        _messageService = new MessageService();
        _consumer = ConfigureConsumer();
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                _consumer.Subscribe(new List<string>() { _kafkaConfiguration.Topic });
                Console.WriteLine($"Consumer has started and reading topic {_kafkaConfiguration.Topic}");
                
                await Consume(cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Consumer is stopping.");

        _consumer.Close();

        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _consumer.Dispose();
    }

    private IConsumer<Null, byte[]> ConfigureConsumer()
    {
        Console.WriteLine($"Configure consuming on: {_kafkaConfiguration.Brokers}");
        
        var config = new ConsumerConfig()
        {
            BootstrapServers = _kafkaConfiguration.Brokers,
            GroupId = _kafkaConfiguration.ConsumerGroup,
            SecurityProtocol = SecurityProtocol.Plaintext,
            EnableAutoCommit = false,
            StatisticsIntervalMs = 5000,
            SessionTimeoutMs = 6000,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnablePartitionEof = true
        };

        return new ConsumerBuilder<Null, byte[]>(config)
            .SetStatisticsHandler((_, statistics) => KafkaStatsHandler(statistics))
            .SetErrorHandler((_, error) => KafkaErrorHandler(error))
            .Build();
    }
    
    private static void KafkaStatsHandler(string statistics)
    {
        Task.Run(() =>
        {
            //Console.WriteLine($"Statistics: {statistics}");
        });
    }
    
    private static void KafkaErrorHandler(Error error)
    {
        Task.Run(() =>
        {
            Console.WriteLine($"Error[{error.Code}]: Reason:[{error.Reason}] Message:[{error}]");
        });
    }
    
    private async Task Consume(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(cancellationToken);

                if (consumeResult?.Message is null)
                {
                    continue;
                }
                
                if (consumeResult.IsPartitionEOF)
                {
                    Console.WriteLine(
                        $"End of topic {consumeResult.Topic}, partition {consumeResult.Partition}, offset {consumeResult.Offset}.");

                    continue;
                }
                
                if (consumeResult.Topic.Equals(_kafkaConfiguration.Topic))
                {
                    await Task.Run(() =>
                    {
                        var order = _messageService.GetOrderFrom(consumeResult.Message.Value);
                        Console.WriteLine($"Received message at {consumeResult.TopicPartitionOffset}: {order.ToStringOrder()}");
                        
                        
                    }, cancellationToken);
                }

                _consumer.Commit();
                Console.WriteLine($"Offset committed for {consumeResult.TopicPartitionOffset}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}