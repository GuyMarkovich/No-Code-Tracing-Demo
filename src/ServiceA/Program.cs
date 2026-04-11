using RabbitMQ.Client;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/publish", () =>
{
    var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
    var port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672");
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

    channel.BasicPublish(exchange: "", routingKey: queue, body: body);

    return Results.Ok(new { status = "published", message });
});

app.Run();
