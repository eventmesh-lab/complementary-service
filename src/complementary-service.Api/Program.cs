using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ComplementaryServices.Application.Services;
using ComplementaryServices.Domain.Repositories;
using MediatR;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// DI: registramos implementaciones concretas
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MediatR para eventos de dominio
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IComplementaryServiceAppService).Assembly));

// Application Services
builder.Services.AddScoped<IComplementaryServiceAppService, ComplementaryServiceAppService>();

// Registrations (composition root)
// Aquí irían las implementaciones de Infrastructure (EF Core, etc.)
// builder.Services.AddScoped<IComplementaryServiceRepository, ComplementaryServiceRepository>();
// builder.Services.AddScoped<IReservationRepository, ReservationRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
