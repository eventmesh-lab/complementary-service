using System;
using System.Collections.Generic;

namespace ComplementaryServices.Domain.ValueObjects
{
    public class ServiceType : ValueObject
    {
        public string Value { get; private set; }

        public static readonly ServiceType Transport = new ServiceType("Transport");
        public static readonly ServiceType Catering = new ServiceType("Catering");
        public static readonly ServiceType Merchandising = new ServiceType("Merchandising");

        private ServiceType(string value)
        {
            Value = value;
        }

        public static ServiceType FromString(string value)
        {
            return value?.ToLower() switch
            {
                "transport" => Transport,
                "catering" => Catering,
                "merchandising" => Merchandising,
                _ => throw new ArgumentException($"Invalid service type: {value}")
            };
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
