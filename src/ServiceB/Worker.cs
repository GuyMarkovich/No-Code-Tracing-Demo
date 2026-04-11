using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace ServiceB;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
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

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation("Received: {Message}", message);
            _channel.BasicAck(ea.DeliveryTag, multiple: false);
        };

        _channel.BasicConsume(queue, autoAck: false, consumer);

        _logger.LogInformation("Listening on queue '{Queue}'", queue);

        stoppingToken.Register(() =>
        {
            _channel?.Dispose();
            _connection?.Dispose();
        });

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
