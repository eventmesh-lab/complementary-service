using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ComplementaryServices.Application.Services;
using ComplementaryServices.Application.Messaging.RabbitMQ;
using ComplementaryServices.Domain.Repositories;
using ComplementaryServices.Infrastructure.Messaging.RabbitMQ;
using ComplementaryServices.Infrastructure.Persistence;
using ComplementaryServices.Infrastructure.Notifications;
using ComplementaryServices.Application.Notifications;
using Microsoft.AspNetCore.SignalR;
using MediatR;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// DI: registramos implementaciones concretas
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// RabbitMQ Configuration
builder.Services.Configure<RabbitMQConfiguration>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<IServiceRequestPublisher, ServiceRequestPublisher>();
builder.Services.AddHostedService<ServiceResponseConsumer>();

// SignalR Configuration
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, SignalRUserIdProvider>();
builder.Services.AddScoped<IServiceNotifier, SignalRServiceNotifier>();

// MediatR para eventos de dominio
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(IComplementaryServiceAppService).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(ServiceRequestPublisher).Assembly);
});

// Application Services
builder.Services.AddScoped<IComplementaryServiceAppService, ComplementaryServiceAppService>();

// Registrations (composition root)
builder.Services.AddSingleton<IComplementaryServiceRepository, ComplementaryServiceRepository>();
builder.Services.AddSingleton<IReservationRepository, ReservationRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHub<ServiceNotificationHub>("/hubs/serviceNotifications");

app.Run();
