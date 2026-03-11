using Confluent.Kafka;
using HospitalApi.Data;
using HospitalApi.Events;
using HospitalApi.Models;
using System.Text.Json;

namespace HospitalApi.Kafka
{
    public class KafkaPaymentConsumer : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<KafkaPaymentConsumer> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public const string Topic = "payment-events";

        public KafkaPaymentConsumer(IConfiguration config, ILogger<KafkaPaymentConsumer> logger, IServiceScopeFactory scopeFactory)
        {
            _config = config;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 Kafka Payment Consumer started. Listening on topic: {Topic}", Topic);

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _config["Kafka:BootstrapServers"] ?? "localhost:9092",
                GroupId = "payment-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,   // read from beginning if no offset
                EnableAutoCommit = false,                      // manual commit for reliability
            };

            using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            consumer.Subscribe(Topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Poll with a short timeout so we check cancellation regularly
                    var result = consumer.Consume(TimeSpan.FromMilliseconds(500));
                    if (result == null) continue;

                    _logger.LogInformation(
                        "📨 Kafka event received → Topic: {Topic} | Key: {Key} | Offset: {Offset}",
                        result.Topic, result.Message.Key, result.Offset);

                    await HandleEventAsync(result.Message.Key, result.Message.Value);

                    // Manually commit offset only after successful processing
                    consumer.Commit(result);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "❌ Kafka consume error: {Error}", ex.Error.Reason);
                    await Task.Delay(1000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Unexpected error in Kafka consumer");
                    await Task.Delay(1000, stoppingToken);
                }
            }

            consumer.Close();
            _logger.LogInformation("🛑 Kafka Payment Consumer stopped.");
        }

        private async Task HandleEventAsync(string key, string payload)
        {
            try
            {
                var evt = JsonSerializer.Deserialize<PaymentMadeEvent>(payload);
                if (evt == null) return;

                // Use scoped DbContext (BackgroundService is singleton, DbContext is scoped)
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Check for duplicate events (idempotency)
                var exists = db.PaymentEvents.Any(e => e.EventId == evt.EventId);
                if (exists)
                {
                    _logger.LogWarning("⚠️ Duplicate event skipped → EventId: {EventId}", evt.EventId);
                    return;
                }

                var paymentEvent = new PaymentEvent
                {
                    EventId = evt.EventId,
                    EventType = evt.EventType,
                    OccurredAt = evt.OccurredAt,
                    BillId = evt.BillId,
                    InvoiceNumber = evt.InvoiceNumber,
                    PaidAmount = evt.PaidAmount,
                    TotalAmount = evt.TotalAmount,
                    RemainingBalance = evt.RemainingBalance,
                    PaymentMethod = evt.PaymentMethod,
                    TransactionReference = evt.TransactionReference,
                    PaymentStatus = evt.PaymentStatus,
                    PatientId = evt.PatientId,
                    PatientName = evt.PatientName,
                    DoctorId = evt.DoctorId,
                    DoctorName = evt.DoctorName,
                    AppointmentId = evt.AppointmentId,
                    PaidByUserId = evt.PaidByUserId,
                    RawPayload = payload,
                    ConsumedAt = DateTime.UtcNow
                };

                db.PaymentEvents.Add(paymentEvent);
                await db.SaveChangesAsync();

                _logger.LogInformation(
                    "✅ Payment event saved → BillId: {BillId} | Amount: {Amount} | Status: {Status}",
                    evt.BillId, evt.PaidAmount, evt.PaymentStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to handle Kafka event. Payload: {Payload}", payload);
            }
        }
    }
}