using System;

namespace ComplementaryServices.Application.Exceptions
{
    public class ServiceNotFoundException : Exception
    {
        public ServiceNotFoundException(Guid serviceId)
            : base($"Service with ID {serviceId} was not found")
        {
        }
    }

    public class ReservationNotFoundException : Exception
    {
        public ReservationNotFoundException(Guid reservationId)
            : base($"Reservation with ID {reservationId} was not found")
        {
        }
    }

    public class InvalidServiceRequestException : Exception
    {
        public InvalidServiceRequestException(string message) : base(message)
        {
        }
    }
}
