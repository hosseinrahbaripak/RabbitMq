using NotificationSystem.Infrastructure.Messaging;
using NotificationSystem.Worker;


var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
