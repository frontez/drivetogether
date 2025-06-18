using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace TripService;

public class MessagePublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string QueueName = "service-a-to-b";

    public MessagePublisher(IConfiguration configuration)
    {
        var factory = new ConnectionFactory()
        {
            HostName = configuration["RabbitMQ:Host"],
            Port = int.Parse(configuration["RabbitMQ:Port"]),
            UserName = configuration["RabbitMQ:Username"],
            Password = configuration["RabbitMQ:Password"]
        };
        
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false);
    }

    public void Publish<T>(T message)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);
        
        _channel.BasicPublish(exchange: "", 
                            routingKey: QueueName, 
                            basicProperties: null, 
                            body: body);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
