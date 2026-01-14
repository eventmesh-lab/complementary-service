using System;

namespace ComplementaryServices.Domain.Common
{
    public interface IDomainEvent
    {
        DateTime OccurredOn { get; }
    }
}
