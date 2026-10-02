namespace NotificationSystem.Infrastructure.Messaging.RabbitMQ
{
    public sealed class RabbitMqOptions
    {
        public const string SectionName = "RabbitMQ";

        public string HostName { get; set; } = "localhost";

        public int Port { get; set; } = 5672;

        public string UserName { get; set; } = "notification_admin";

        public string Password { get; set; } = string.Empty;

        public string VirtualHost { get; set; } = "/";
    }
}
