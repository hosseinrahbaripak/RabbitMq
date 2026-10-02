using NotificationSystem.Application.Abstractions.MessageBroker;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace NotificationSystem.Infrastructure.Messaging.RabbitMQ
{
    public class RabbitMqPublisher(IConnection connection) : IMessagePublisher
    {

        public async Task PublishAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default)
        {


            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType ="applcation/json"
            };


            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: properties ,
                body: body);

        }
    }
}
