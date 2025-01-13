using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ChatService.EventProcessing;

namespace ChatService.AsyncDataServices;

public class MessageBusSubscriber : BackgroundService
{
    private readonly IEventProcessor _eventProcessor;
    private IConnection? _connection;
    private IModel? _channel;
    private string? _queueName;

    public MessageBusSubscriber(IEventProcessor eventProcessor)
    {
        _eventProcessor = eventProcessor;
        InitiliazeRabbitMQ();
    }

    private void InitiliazeRabbitMQ()
    {
        var factory = new ConnectionFactory()
        {
            HostName = Environment.GetEnvironmentVariable("MQHOST"),
            Port = int.Parse(Environment.GetEnvironmentVariable("MQPORT")!),
            UserName = Environment.GetEnvironmentVariable("MQUSER"),
            Password = Environment.GetEnvironmentVariable("MQPASS"),
            ClientProvidedName = "ChatService",
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _queueName = "ChatServiceQueue";
        _channel.QueueDeclare(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );
        var routingKeys = new[]
        {
            "KK.EVENT.CLIENT.pridr.SUCCESS.#.REGISTER",
            "KK.EVENT.CLIENT.pridr.SUCCESS.#.LOGIN",
            "KK.EVENT.CLIENT.pridr.SUCCESS.#.LOGOUT",
            "KK.EVENT.CLIENT.pridr.SUCCESS.#.DELETE_ACCOUNT",
            "USERNAME_UPDATE"
        };
        foreach (var routingKey in routingKeys)
        {
            _channel.QueueBind(
                queue: _queueName,
                exchange: "amq.topic",
                routingKey: routingKey);
        }
        Console.WriteLine("--> Listening on the Message Bus. Waiting for messages...");
        _connection.ConnectionShutdown += RabbitMQ_ConectionShutdown!;
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (ModuleHandle, ea) =>
        {
            var body = ea.Body;
            var notificationMessage = Encoding.UTF8.GetString(body.ToArray());
            _eventProcessor.ProcessEvent(notificationMessage);
        };
        _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
        return Task.CompletedTask;
    }

    private static void RabbitMQ_ConectionShutdown(object sender, ShutdownEventArgs e)
    {
        Console.WriteLine("--> Connection Shutdown.");
    }

    public override void Dispose()
    {
        if (_channel!.IsOpen)
        {
            _channel.Close();
            _connection!.Close();
        }
        base.Dispose();
    }
}