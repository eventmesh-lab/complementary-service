using System;
using System.Collections.Generic;

namespace ComplementaryServices.Domain.Common
{
    public abstract class Entity
    {
        public Guid Id { get; protected set; }
        private List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected void AddDomainEvent(IDomainEvent eventItem)
        {
            _domainEvents.Add(eventItem);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
