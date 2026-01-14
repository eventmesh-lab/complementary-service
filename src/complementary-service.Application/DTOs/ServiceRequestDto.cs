using System;

namespace ComplementaryServices.Application.DTOs
{
    public class ServiceRequestDto
    {
        public Guid ReservationId { get; set; }
        public string ServiceType { get; set; }
        public string Details { get; set; }
    }
}
