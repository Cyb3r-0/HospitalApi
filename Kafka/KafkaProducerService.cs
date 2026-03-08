using Confluent.Kafka;
using System.Text.Json;

namespace HospitalApi.Kafka
{
    public class KafkaProducerService : IKafkaProducerService, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaProducerService> _logger;

        public KafkaProducerService(IConfiguration config, ILogger<KafkaProducerService> logger)
        {
            _logger = logger;

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"] ?? "localhost:9092",
                Acks = Acks.All,
                MessageTimeoutMs = 5000,
                RetryBackoffMs = 500,
            };

            _producer = new ProducerBuilder<string, string>(producerConfig).Build();
        }

        public async Task PublishAsync<T>(string topic, string key, T message)
        {
            var json = JsonSerializer.Serialize(message);

            try
            {
                var result = await _producer.ProduceAsync(topic, new Message<string, string>
                {
                    Key = key,
                    Value = json
                });

                _logger.LogInformation(
                    "✅ Kafka event published → Topic: {Topic} | Key: {Key} | Offset: {Offset}",
                    topic, key, result.Offset);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex,
                    "❌ Kafka publish failed → Topic: {Topic} | Key: {Key} | Error: {Error}",
                    topic, key, ex.Error.Reason);

                // Don't throw — payment already saved to DB, Kafka failure shouldn't break the request
            }
        }

        public void Dispose()
        {
            _producer.Flush(TimeSpan.FromSeconds(5));
            _producer.Dispose();
        }
    }
}