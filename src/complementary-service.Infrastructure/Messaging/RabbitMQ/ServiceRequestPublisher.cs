using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ComplementaryServices.Application.Messaging.RabbitMQ;
using ComplementaryServices.Domain.ValueObjects;
using ComplementaryServices.Infrastructure.Messaging.Messages;

namespace ComplementaryServices.Infrastructure.Messaging.RabbitMQ
{
    /// <summary>
    /// TC-061: Publica solicitudes de servicios a RabbitMQ
    /// </summary>
    public class ServiceRequestPublisher : IServiceRequestPublisher, IDisposable
    {
        private readonly RabbitMQConfiguration _config;
        private readonly ILogger<ServiceRequestPublisher> _logger;
        private IConnection _connection;
        private IModel _channel;

        public ServiceRequestPublisher(
            IOptions<RabbitMQConfiguration> config,
            ILogger<ServiceRequestPublisher> logger)
        {
            _config = config.Value;
            _logger = logger;
            InitializeRabbitMQ();
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
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declarar exchange tipo topic
                _channel.ExchangeDeclare(
                    exchange: _config.ServiceRequestExchange,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false);

                // Declarar colas para cada tipo de servicio
                _channel.QueueDeclare(
                    queue: _config.TransportRequestQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false);

                _channel.QueueDeclare(
                    queue: _config.CateringRequestQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false);

                _channel.QueueDeclare(
                    queue: _config.MerchandisingRequestQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false);

                // Bindings
                _channel.QueueBind(_config.TransportRequestQueue, _config.ServiceRequestExchange, _config.TransportRoutingKey);
                _channel.QueueBind(_config.CateringRequestQueue, _config.ServiceRequestExchange, _config.CateringRoutingKey);
                _channel.QueueBind(_config.MerchandisingRequestQueue, _config.ServiceRequestExchange, _config.MerchandisingRoutingKey);

                _logger.LogInformation("RabbitMQ Publisher initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing RabbitMQ Publisher");
                throw;
            }
        }

        public Task PublishServiceRequestAsync(
            Guid serviceId,
            Guid reservationId,
            Guid eventId,
            ServiceType serviceType,
            string details,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var message = new ServiceRequestMessage
                {
                    ServiceId = serviceId,
                    ReservationId = reservationId,
                    EventId = eventId,
                    ServiceType = serviceType.Value,
                    Details = details,
                    RequestedAt = DateTime.UtcNow,
                    CallbackQueue = _config.ServiceResponseQueue
                };

                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                var routingKey = GetRoutingKey(serviceType);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.ContentType = "application/json";
                properties.MessageId = serviceId.ToString();
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                _channel.BasicPublish(
                    exchange: _config.ServiceRequestExchange,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation(
                    "Published service request {ServiceId} to exchange {Exchange} with routing key {RoutingKey}",
                    serviceId,
                    _config.ServiceRequestExchange,
                    routingKey);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing service request {ServiceId}", serviceId);
                throw;
            }
        }

        private string GetRoutingKey(ServiceType serviceType)
        {
            return serviceType.Value.ToLower() switch
            {
                "transport" => _config.TransportRoutingKey,
                "catering" => _config.CateringRoutingKey,
                "merchandising" => _config.MerchandisingRoutingKey,
                _ => throw new ArgumentException($"Unknown service type: {serviceType.Value}")
            };
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
