using RabbitMQ.Client;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/publish", () =>
{
    var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
    var portValue = Environment.GetEnvironmentVariable("RABBITMQ_PORT");
    var port = int.TryParse(portValue, out var parsedPort) ? parsedPort : 5672;
    var username = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "guest";
    var password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest";
    var queue = Environment.GetEnvironmentVariable("RABBITMQ_QUEUE") ?? "orders";

    var factory = new ConnectionFactory
    {
        HostName = host,
        Port = port,
        UserName = username,
        Password = password
    };

    using var connection = factory.CreateConnection();
    using var channel = connection.CreateModel();

    channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);

    var message = $"Order created at {DateTime.UtcNow:O}";
    var body = Encoding.UTF8.GetBytes(message);
    var properties = channel.CreateBasicProperties();
    properties.Persistent = true;
    properties.ContentType = "text/plain";
    properties.Headers = new Dictionary<string, object>();

    channel.BasicPublish(exchange: "", routingKey: queue, basicProperties: properties, body: body);

    return Results.Ok(new { status = "published", message });
});

app.MapGet("/healthz", () => Results
.Ok(new { status = "ok" }));

app.Run();Ok(new { status = "ok" }));

app.Run();
