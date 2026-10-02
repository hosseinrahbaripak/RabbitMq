using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationSystem.Worker;

public class Worker : BackgroundService
{
    private readonly IConnection _connection;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IConnection connection,
        ILogger<Worker> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        await using var channel =
            await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: "notification.queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            try
            {
                var body = eventArgs.Body.ToArray();

                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation(
                    "Message received: {Message}",
                    message);

                //await channel.BasicAckAsync(
                //    deliveryTag: eventArgs.DeliveryTag,
                //    multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while processing message.");

                await channel.BasicNackAsync(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: true);
            }
        };

        await channel.BasicConsumeAsync(
            queue: "notification.queue",
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation(
            "Consumer started. Waiting for messages...");

        try
        {
            await Task.Delay(
                Timeout.Infinite,
                stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Application is shutting down.
        }
    }
}