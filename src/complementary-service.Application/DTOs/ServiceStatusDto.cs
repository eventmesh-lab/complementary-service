using System;

namespace ComplementaryServices.Application.DTOs
{
    public class ServiceStatusDto
    {
        public Guid ServiceId { get; set; }
        public Guid ReservationId { get; set; }
        public string ServiceType { get; set; }
        public string Status { get; set; }
        public string ProviderId { get; set; }
        public decimal Price { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string RejectionReason { get; set; }
        public string Details { get; set; }
    }
}
