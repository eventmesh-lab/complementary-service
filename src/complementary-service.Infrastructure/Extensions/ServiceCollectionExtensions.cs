using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SignalR;
using ComplementaryServices.Infrastructure.Messaging.RabbitMQ;
using ComplementaryServices.Infrastructure.Notifications;
using ComplementaryServices.Domain.Repositories;
using ComplementaryServices.Infrastructure.Persistence;
using ComplementaryServices.Application.Messaging.RabbitMQ;
using ComplementaryServices.Application.Notifications;

namespace ComplementaryServices.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMQServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configuración
            services.Configure<RabbitMQConfiguration>(
                configuration.GetSection("RabbitMQ"));

            // Publisher
            services.AddSingleton<IServiceRequestPublisher, ServiceRequestPublisher>();

            // Consumer como HostedService
            services.AddHostedService<ServiceResponseConsumer>();

            return services;
        }

        public static IServiceCollection AddSignalRServices(this IServiceCollection services)
        {
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            });

            // Custom UserIdProvider para Keycloak
            services.AddSingleton<IUserIdProvider, SignalRUserIdProvider>();

            // Notifier
            services.AddScoped<IServiceNotifier, SignalRServiceNotifier>();

            return services;
        }
    }
}
