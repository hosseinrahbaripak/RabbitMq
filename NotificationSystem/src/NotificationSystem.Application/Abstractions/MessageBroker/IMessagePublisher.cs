namespace NotificationSystem.Application.Abstractions.MessageBroker
{
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(
            T message,
            string routingKey,
            CancellationToken cancellationToken = default);

    }
}
