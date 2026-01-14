using System;
using ComplementaryServices.Domain.ValueObjects;

namespace ComplementaryServices.Domain.Entities
{
    public class ServiceProvider : Entity
    {
        public string Name { get; private set; }
        public ServiceType ServiceType { get; private set; }
        public string ApiEndpoint { get; private set; }
        public string QueueName { get; private set; }
        public bool IsActive { get; private set; }
        public int Priority { get; private set; }

        private ServiceProvider() { }

        public ServiceProvider(
            string name,
            ServiceType serviceType,
            string apiEndpoint,
            string queueName,
            int priority = 1)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            ApiEndpoint = apiEndpoint;
            QueueName = queueName;
            IsActive = true;
            Priority = priority;
        }

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
    }
}
