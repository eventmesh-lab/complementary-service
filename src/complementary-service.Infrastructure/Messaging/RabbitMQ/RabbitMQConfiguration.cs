namespace ComplementaryServices.Infrastructure.Messaging.RabbitMQ
{
    public class RabbitMQConfiguration
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";

        // Exchanges
        public string ServiceRequestExchange { get; set; } = "services.requests";
        public string ServiceResponseExchange { get; set; } = "services.responses";

        // Queues
        public string TransportRequestQueue { get; set; } = "transport.requests";
        public string CateringRequestQueue { get; set; } = "catering.requests";
        public string MerchandisingRequestQueue { get; set; } = "merchandising.requests";
        public string ServiceResponseQueue { get; set; } = "services.responses.platform";

        // Routing Keys
        public string TransportRoutingKey { get; set; } = "service.request.transport";
        public string CateringRoutingKey { get; set; } = "service.request.catering";
        public string MerchandisingRoutingKey { get; set; } = "service.request.merchandising";
        public string ResponseRoutingKey { get; set; } = "service.response";
    }
}
