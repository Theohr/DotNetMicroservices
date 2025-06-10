
using System.Text;
using AutoMapper;
using CommandsService.Enums;
using CommandsService.EventProcessing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommandsService.AsyncDataServices
{
    public class MessageBusSubscriber : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly IEventProcessor _eventProcessor;
        private IConnection _connection;
        private IChannel _channel;
        private string _queueName;

        public MessageBusSubscriber(IConfiguration config, IEventProcessor eventProcessor)
        {
            _config = config;
            _eventProcessor = eventProcessor;

        }

        private async Task InitializeRabbitMQ(CancellationToken cancellationToken)
        {
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

                _queueName = _channel.QueueDeclareAsync().Result.QueueName;

                _channel.QueueBindAsync(queue: _queueName, exchange: "trigger", routingKey: "");

                _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdown;

                Console.WriteLine("Listening on the message bus...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection to Message Bus Failed: {ex.Message}");
                _connection = null;
                _channel = null;
            }

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            await InitializeRabbitMQ(stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += (ModuleHandle, ea) =>
            {
                Console.WriteLine("Event Received!");

                var body = ea.Body;

                var notificationMessage = Encoding.UTF8.GetString(body.ToArray());

                _eventProcessor.ProcessEvent(notificationMessage);

                return Task.CompletedTask;
            };

            await _channel.BasicConsumeAsync(queue: _queueName, autoAck: true, consumer: consumer);
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