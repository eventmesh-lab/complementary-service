using System;
using System.Collections.Generic;
using ComplementaryServices.Domain.Common;

namespace ComplementaryServices.Domain.ValueObjects
{
    public class ProviderResponse : ValueObject
    {
        public bool IsAvailable { get; private set; }
        public string ProviderId { get; private set; }
        public string Message { get; private set; }
        public decimal Price { get; private set; }
        public DateTime? EstimatedTime { get; private set; }

        public ProviderResponse(
            bool isAvailable,
            string providerId,
            string message,
            decimal price,
            DateTime? estimatedTime = null)
        {
            IsAvailable = isAvailable;
            ProviderId = providerId ?? throw new ArgumentNullException(nameof(providerId));
            Message = message;
            Price = price;
            EstimatedTime = estimatedTime;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return IsAvailable;
            yield return ProviderId;
            yield return Price;
        }
    }
}
