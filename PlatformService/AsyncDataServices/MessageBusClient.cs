using System.Text;
using System.Text.Json;
using PlatformService.DTOs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration _config;
        private IConnection _connection;
        private IChannel _channel;

        public MessageBusClient(IConfiguration config)
        {
            _config = config;
            Console.WriteLine("MessageBusClient constructor called!");

            // Ensure RabbitMQ is initialized before publishing
            InitializeRabbitMQ();
        }

        private async Task InitializeRabbitMQ()
        {
            if (_channel != null && _channel.IsOpen)
            {
                Console.WriteLine("RabbitMQ already initialized and open.");
                return;
            }

            var factory = new ConnectionFactory()
            {
                HostName = _config["RabbitMQHost"],
                Port = int.Parse(_config["RabbitMQPort"])
            };

            try
            {
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.ExchangeDeclareAsync(exchange: "trigger", type: ExchangeType.Fanout);

                _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdown;

                Console.WriteLine("Connected to MessageBus!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection to Message Bus Failed: {ex.Message}");
                _connection = null;
                _channel = null;
            }
        }

        public async Task PublishNewPlatform(PlatformPublishedDto publishedDto)
        {

            if (_channel != null && _channel.IsOpen)
            {
                var message = JsonSerializer.Serialize(publishedDto);
                Console.WriteLine("RabbitMQ Connection Open, Sending message...");
                await SendMessage(message);
            }
            else
            {
                Console.WriteLine("RabbitMQ Connection is closed or not initialized...");
            }
        }

        private async Task SendMessage(string message)
        {
            try
            {
                var body = Encoding.UTF8.GetBytes(message);
                var properties = new BasicProperties();

                await _channel.BasicPublishAsync(
                    exchange: "trigger",
                    routingKey: "",
                    mandatory: false,
                    basicProperties: properties,
                    body: body);

                Console.WriteLine("Message was sent!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Message failed: {ex.Message}");
            }
        }

        private async Task RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("RabbitMQ connection shutdown.");
            await Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            Console.WriteLine("MessageBus Disposed");
            if (_channel != null && _channel.IsOpen)
            {
                await _channel.CloseAsync();
                _connection?.Dispose();
            }
        }
    }
}