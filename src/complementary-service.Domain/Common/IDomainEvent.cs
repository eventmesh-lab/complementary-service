using System;
using MediatR;

namespace ComplementaryServices.Domain.Common
{
    public interface IDomainEvent : INotification
    {
        DateTime OccurredOn { get; }
    }
}
