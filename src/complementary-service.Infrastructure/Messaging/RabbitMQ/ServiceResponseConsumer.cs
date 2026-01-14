using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using ComplementaryServices.Infrastructure.Messaging.Messages;
using ComplementaryServices.Application.Services;

namespace ComplementaryServices.Infrastructure.Messaging.RabbitMQ
{
    /// <summary>
    /// TC-061: Consume respuestas de proveedores desde RabbitMQ
    /// TC-062: Actualiza estado directo vía AppService (sin CQRS)
    /// </summary>
    public class ServiceResponseConsumer : BackgroundService
    {
        private readonly RabbitMQConfiguration _config;
        private readonly ILogger<ServiceResponseConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private IConnection _connection;
        private IModel _channel;

        public ServiceResponseConsumer(
            IOptions<RabbitMQConfiguration> config,
            ILogger<ServiceResponseConsumer> logger,
            IServiceProvider serviceProvider)
        {
            _config = config.Value;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() => _logger.LogInformation("ServiceResponseConsumer stopping"));

            InitializeRabbitMQ();
            StartConsuming();

            return Task.CompletedTask;
        }

        private void InitializeRabbitMQ()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _config.HostName,
                    Port = _config.Port,
                    UserName = _config.UserName,
                    Password = _config.Password,
                    VirtualHost = _config.VirtualHost,
                    DispatchConsumersAsync = true,
                    AutomaticRecoveryEnabled = true
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declarar exchange de respuestas
                _channel.ExchangeDeclare(
                    exchange: _config.ServiceResponseExchange,
                    type: ExchangeType.Topic,
                    durable: true);

                // Declarar cola de respuestas
                _channel.QueueDeclare(
                    queue: _config.ServiceResponseQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false);

                // Binding
                _channel.QueueBind(
                    _config.ServiceResponseQueue,
                    _config.ServiceResponseExchange,
                    _config.ResponseRoutingKey);

                _logger.LogInformation("RabbitMQ Consumer initialized for queue {Queue}", _config.ServiceResponseQueue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing RabbitMQ Consumer");
                throw;
            }
        }

        private void StartConsuming()
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var response = JsonSerializer.Deserialize<ServiceResponseMessage>(json);

                    if (response == null) return;

                    _logger.LogInformation(
                        "Received response for service {ServiceId} from provider {ProviderId}",
                        response.ServiceId,
                        response.ProviderId);

                    await ProcessResponseAsync(response);

                    // ACK solo si se procesó correctamente
                    _channel.BasicAck(ea.DeliveryTag, false);

                    _logger.LogInformation(
                        "Successfully processed response for service {ServiceId}",
                        response.ServiceId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing service response");
                    
                    // NACK con requeue si hay error
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(
                queue: _config.ServiceResponseQueue,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("Started consuming from queue {Queue}", _config.ServiceResponseQueue);
        }

        private async Task ProcessResponseAsync(ServiceResponseMessage response)
        {
            using var scope = _serviceProvider.CreateScope();
            var appService = scope.ServiceProvider.GetRequiredService<IComplementaryServiceAppService>();

            if (response.IsAvailable)
            {
                await appService.ConfirmServiceAsync(
                    response.ServiceId,
                    response.ProviderId,
                    response.Price,
                    response.Message,
                    response.EstimatedTime);
            }
            else
            {
                await appService.RejectServiceAsync(
                    response.ServiceId,
                    response.RejectionReason ?? "Service not available");
            }
        }

        public override void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
