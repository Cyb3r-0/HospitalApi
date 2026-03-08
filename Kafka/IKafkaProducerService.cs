namespace HospitalApi.Kafka
{
    public interface IKafkaProducerService
    {
        Task PublishAsync<T>(string topic, string key, T message);
    }
}