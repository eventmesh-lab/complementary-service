using System;

namespace ComplementaryServices.Infrastructure.Messaging.Messages
{
    /// <summary>
    /// Mensaje publicado a RabbitMQ cuando se solicita un servicio
    /// TC-061: Integración vía RabbitMQ
    /// </summary>
    public class ServiceRequestMessage
    {
        public Guid ServiceId { get; set; }
        public Guid ReservationId { get; set; }
        public Guid EventId { get; set; }
        public string ServiceType { get; set; }
        public string Details { get; set; }
        public DateTime RequestedAt { get; set; }
        public string CallbackQueue { get; set; } // Cola para recibir respuesta
    }
}
