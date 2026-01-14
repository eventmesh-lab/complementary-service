using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ComplementaryServices.Application.Services;
using ComplementaryServices.Domain.Repositories;
using ComplementaryServices.Infrastructure.Persistence;
using ComplementaryServices.Infrastructure.Extensions;
using ComplementaryServices.Infrastructure.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using MediatR;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// Database
builder.Services.AddDbContext<ComplementaryServiceDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsAssembly("complementary_service.Infrastructure")));

// MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(IComplementaryServiceAppService).Assembly);
});

// Repositories
builder.Services.AddScoped<IComplementaryServiceRepository, ComplementaryServiceRepository>();
builder.Services.AddSingleton<IReservationRepository, ReservationRepository>();

// RabbitMQ
builder.Services.AddRabbitMQServices(builder.Configuration);

// SignalR
builder.Services.AddSignalRServices();

// Keycloak Authentication
builder.Services.AddKeycloakAuthentication(builder.Configuration);

// Application Service
builder.Services.AddScoped<IComplementaryServiceAppService, ComplementaryServiceAppService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000", "http://localhost:5173" })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Para SignalR
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowWebApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// SignalR Hub
app.MapHub<ServiceNotificationHub>("/hubs/service-notifications");

app.Run();
