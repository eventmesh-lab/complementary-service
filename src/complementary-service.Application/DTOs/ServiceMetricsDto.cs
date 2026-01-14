using System;
using System.Collections.Generic;

namespace ComplementaryServices.Application.DTOs
{
    public class ServiceMetricsDto
    {
        public int TotalRequests { get; set; }
        public int Confirmed { get; set; }
        public int Rejected { get; set; }
        public int Pending { get; set; }
        public decimal AveragePrice { get; set; }
        public Dictionary<string, int> ByServiceType { get; set; } = new();
    }
}