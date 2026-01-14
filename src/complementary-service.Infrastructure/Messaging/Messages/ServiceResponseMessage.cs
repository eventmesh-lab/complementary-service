using System;

namespace ComplementaryServices.Infrastructure.Messaging.Messages
{
    /// <summary>
    /// Mensaje recibido desde RabbitMQ con la respuesta del proveedor
    /// </summary>
    public class ServiceResponseMessage
    {
        public Guid ServiceId { get; set; }
        public bool IsAvailable { get; set; }
        public string ProviderId { get; set; }
        public string Message { get; set; }
        public decimal Price { get; set; }
        public DateTime? EstimatedTime { get; set; }
        public string RejectionReason { get; set; }
    }
}
