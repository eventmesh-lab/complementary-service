using System;
using System.Collections.Generic;
using ComplementaryServices.Domain.Common;

namespace ComplementaryServices.Domain.ValueObjects
{
    public class ServiceStatus : ValueObject
    {
        public string Value { get; private set; }

        public static readonly ServiceStatus Requested = new ServiceStatus("Requested");
        public static readonly ServiceStatus Pending = new ServiceStatus("Pending");
        public static readonly ServiceStatus Confirmed = new ServiceStatus("Confirmed");
        public static readonly ServiceStatus Rejected = new ServiceStatus("Rejected");
        public static readonly ServiceStatus Cancelled = new ServiceStatus("Cancelled");

        private ServiceStatus(string value)
        {
            Value = value;
        }

        public static ServiceStatus FromString(string value)
        {
            return value?.ToLower() switch
            {
                "requested" => Requested,
                "pending" => Pending,
                "confirmed" => Confirmed,
                "rejected" => Rejected,
                "cancelled" => Cancelled,
                _ => throw new ArgumentException($"Invalid status: {value}")
            };
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}