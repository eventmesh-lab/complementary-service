using System;

namespace ComplementaryServices.Domain.Entities
{
    public class Reservation
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string Status { get; private set; }

        public Reservation(Guid id, Guid userId, string status)
        {
            Id = id;
            UserId = userId;
            Status = status;
        }

        public bool IsConfirmed() => Status == "Confirmed";
    }
}
