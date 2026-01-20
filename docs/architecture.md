# Arquitectura del Sistema - Complementary Services

## 🏗️ Visión General

Este microservicio implementa una **Arquitectura Hexagonal (Ports & Adapters)** combinada con principios de **Domain-Driven Design (DDD)**. La arquitectura permite aislar la lógica de negocio del dominio de los detalles de infraestructura, facilitando la mantenibilidad y pruebas.

### Estructura de Capas

```
┌─────────────────────────────────────────────────────────┐
│                    API Layer (Ports)                     │
│              ComplementaryServicesController             │
│                (HTTP REST + SignalR)                     │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│              Application Layer (Use Cases)               │
│        ComplementaryServiceAppService (Orchestrator)     │
│              Event Handlers (MediatR)                    │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│                  Domain Layer (Core)                     │
│  Entities: ComplementaryService, Reservation             │
│  Value Objects: ServiceType, ServiceStatus               │
│  Domain Events: ServiceRequested, Confirmed, Rejected    │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│           Infrastructure Layer (Adapters)                │
│  Persistence: PostgreSQL + EF Core                       │
│  Messaging: RabbitMQ Publisher/Consumer                  │
│  Notifications: SignalR Hub                              │
└──────────────────────────────────────────────────────────┘
```

---

## 🔄 Flujo de Datos Completo

### 1. Solicitud de Servicio (Happy Path)

```
Usuario → API Controller → Application Service → Domain Entity → Repository → Database
                                    │                   │
                                    ↓                   ↓
                              MediatR Events      Domain Events
                                    │                   │
                                    ↓                   ↓
                             Event Handler → RabbitMQ Publisher
                                                    │
                                                    ↓
                                          Provider External Queue
```

#### Paso a Paso

1. **Cliente HTTP** envía `POST /api/v1/ComplementaryServices/request`
   - Body: `{ reservationId, eventId, serviceType: "Transport", details: "..." }`
   - Header: `Authorization: Bearer <JWT>` o `X-User-Id: <guid>` (testing)

2. **Controller** (`ComplementaryServicesController`)
   - Extrae el `userId` del JWT (claim `NameIdentifier`)
   - Valida autenticación
   - Llama a `_appService.RequestServiceAsync(request, userId)`

3. **Application Service** (`ComplementaryServiceAppService`)
   - Valida que la reserva existe (`IReservationRepository.GetByIdAsync`)
   - Valida que el usuario es dueño de la reserva
   - Valida que la reserva está confirmada (`reservation.IsConfirmed()`)
   - Crea una nueva entidad `ComplementaryService` del dominio
   - Persiste en base de datos (`_repository.AddAsync()`)

4. **Domain Entity** (`ComplementaryService`)
   - Se inicializa con estado `Requested`
   - Genera un `ServiceRequestedDomainEvent`

5. **Event Handler** (`ServiceRequestedEventHandler`)
   - Recibe el evento vía **MediatR**
   - Invoca `IServiceRequestPublisher.PublishServiceRequestAsync()`

6. **RabbitMQ Publisher** (`ServiceRequestPublisher`)
   - Publica mensaje a exchange `services.requests` (tipo Topic)
   - Routing Key: `service.request.transport | catering | merchandising`
   - El mensaje llega a la cola del proveedor correspondiente

7. **Proveedor Externo** (Mock o real)
   - Consume mensaje de su cola
   - Procesa la solicitud
   - Publica respuesta a `services.responses.platform`

8. **RabbitMQ Consumer** (`ServiceResponseConsumer`)
   - Escucha `services.responses.platform`
   - Recibe `ServiceResponseMessage` con `{ serviceId, isAvailable, price, ... }`
   - Invoca `_appService.ConfirmServiceAsync()` o `_appService.RejectServiceAsync()`

9. **Domain Entity** actualizado
   - Estado cambia a `Confirmed` o `Rejected`
   - Genera evento `ServiceConfirmedDomainEvent` o `ServiceRejectedDomainEvent`

10. **Event Handler de Confirmación/Rechazo**
    - Envía notificación SignalR al usuario
    - Hub: `/hubs/service-notifications`
    - Método: `ReceiveServiceUpdate(serviceId, status, details)`

11. **Cliente Frontend (React/Vue/etc.)**
    - Recibe notificación en tiempo real vía WebSocket
    - Actualiza UI sin necesidad de polling

---

### 2. Consulta de Estado

```
Usuario → Controller.GetServiceStatus(serviceId)
                ↓
         AppService.GetServiceByIdAsync()
                ↓
         Repository → PostgreSQL
                ↓
         ServiceStatusDto → Response 200 OK
```

---

## 🌐 Dependencias Externas

### 1. **PostgreSQL** (Base de Datos Principal)
- **Propósito**: Persistencia de solicitudes de servicios
- **Tablas principales**:
  - `ComplementaryServices`: Solicitudes de servicios
  - (EF Core migrations en `Infrastructure/Persistence/Migrations`)
- **Conexión**: Configurada vía `ConnectionStrings:DefaultConnection` en `appsettings.json`

### 2. **RabbitMQ** (Message Broker)
- **Propósito**: Comunicación asíncrona con proveedores externos
- **Exchanges**:
  - `services.requests` (Topic): Solicitudes salientes
  - `services.responses` (Direct): Respuestas entrantes
- **Colas**:
  - `transport.requests`: Proveedores de transporte
  - `catering.requests`: Proveedores de catering
  - `merchandising.requests`: Proveedores de merchandising
  - `services.responses.platform`: Respuestas de todos los proveedores
- **Routing Keys**:
  - `service.request.transport`
  - `service.request.catering`
  - `service.request.merchandising`
  - `service.response`

### 3. **SignalR Hub** (Notificaciones en Tiempo Real)
- **Propósito**: Notificar cambios de estado a clientes conectados
- **Hub Endpoint**: `/hubs/service-notifications`
- **Métodos**:
  - `ReceiveServiceUpdate(serviceId, status, details)`: Cliente escucha este método
- **UserIdProvider**: Extrae el `userId` del JWT para enviar notificaciones a usuarios específicos

### 4. **MongoDB** (Opcional - Logs)
- **Propósito**: Almacenamiento de logs estructurados
- **Configuración**: `MongoDB:ConnectionString` en `appsettings.json`
- **Uso**: Actualmente configurado pero no implementado en código visible

### 5. **Keycloak** (Autenticación)
- **Propósito**: Gestión de identidades y autenticación JWT
- **Integración**: 
  - Controller valida JWT con claim `NameIdentifier`
  - Fallback: Header `X-User-Id` para testing sin JWT

### 6. **Proveedores Externos** (Mock o Servicios Reales)
- **Mock Provider**: Implementado en `docker/mock-provider/`
- **Protocolo**: Mensajes RabbitMQ
- **Contrato**:
  - **Request**: `{ serviceId, reservationId, eventId, serviceType, details }`
  - **Response**: `{ serviceId, isAvailable, providerId, price, message, estimatedTime }`

---

## 📊 Modelo de Datos (Domain Entities)

### 1. **ComplementaryService** (Aggregate Root)

```csharp
public class ComplementaryService : Entity, IAggregateRoot
{
    public Guid Id { get; }                    // PK
    public Guid ReservationId { get; }         // FK a Reserva (otro microservicio)
    public Guid UserId { get; }                // Propietario del servicio
    public Guid EventId { get; }               // FK a Evento
    public ServiceType ServiceType { get; }     // Transport | Catering | Merchandising
    public ServiceStatus Status { get; }        // Requested | Pending | Confirmed | Rejected | Cancelled
    public string ProviderId { get; }           // ID del proveedor que acepta
    public decimal Price { get; }               // Precio confirmado
    public string Details { get; }              // Detalles específicos
    public DateTime RequestedAt { get; }
    public DateTime? ConfirmedAt { get; }
    public DateTime? RejectedAt { get; }
    public string RejectionReason { get; }
    public ProviderResponse ProviderResponse { get; } // Value Object
}
```

**Estados del Ciclo de Vida**:
- `Requested`: Solicitud creada, pendiente de enviar
- `Pending`: Enviada a proveedor, esperando respuesta
- `Confirmed`: Proveedor aceptó (tiene precio y providerId)
- `Rejected`: Proveedor rechazó (tiene rejectionReason)
- `Cancelled`: Usuario canceló antes de confirmar

**Métodos del Dominio**:
- `MarkAsPending(providerId)`: Cambia a Pending
- `Confirm(ProviderResponse)`: Valida disponibilidad y confirma
- `Reject(reason)`: Marca como rechazada
- `Cancel()`: Usuario cancela

### 2. **Reservation** (External Entity)

```csharp
public class Reservation
{
    public Guid Id { get; }
    public Guid UserId { get; }
    public string Status { get; }
    
    public bool IsConfirmed() => Status == "Confirmed";
}
```

**Nota**: Esta entidad representa datos de otro microservicio. El repositorio actual es in-memory (mock), en producción debería consultar una API o evento de sincronización.

### 3. **Value Objects**

#### ServiceType
```csharp
public class ServiceType : ValueObject
{
    public string Value { get; }
    public static ServiceType Transport = new("Transport");
    public static ServiceType Catering = new("Catering");
    public static ServiceType Merchandising = new("Merchandising");
}
```

#### ServiceStatus
```csharp
public class ServiceStatus : ValueObject
{
    public string Value { get; }
    public static ServiceStatus Requested = new("Requested");
    public static ServiceStatus Pending = new("Pending");
    public static ServiceStatus Confirmed = new("Confirmed");
    public static ServiceStatus Rejected = new("Rejected");
    public static ServiceStatus Cancelled = new("Cancelled");
}
```

#### ProviderResponse
```csharp
public class ProviderResponse : ValueObject
{
    public bool IsAvailable { get; }
    public string ProviderId { get; }
    public string Message { get; }
    public decimal Price { get; }
    public DateTime? EstimatedTime { get; }
}
```

---

## 🎯 Patrones Implementados

### 1. **Hexagonal Architecture (Ports & Adapters)**
- **Ports**: Interfaces definidas en `Domain.Repositories` y `Application.Messaging`
- **Adapters**: Implementaciones en `Infrastructure`

### 2. **Domain-Driven Design (DDD)**
- **Aggregates**: `ComplementaryService` es la raíz
- **Value Objects**: Inmutables (`ServiceType`, `ServiceStatus`, `ProviderResponse`)
- **Domain Events**: Generados por el agregado (`ServiceRequestedDomainEvent`, etc.)

### 3. **CQRS (Command Query Responsibility Segregation)**
- **Commands**: `RequestServiceAsync`, `ConfirmServiceAsync`, `CancelServiceAsync`
- **Queries**: `GetServiceByIdAsync`, `GetUserServicesAsync`, `GetMetricsAsync`

### 4. **MediatR (Mediator Pattern)**
- Desacopla Event Handlers de la lógica de negocio
- Publicación de Domain Events como Notifications

### 5. **Unit of Work**
- Implementado vía `IUnitOfWork` en el repositorio
- `SaveChangesAsync()` garantiza transaccionalidad

---

## ⚠️ Deuda Técnica Detectada

### 1. **Repositorio de Reservas In-Memory**
**Ubicación**: `Infrastructure/Persistence/ReservationRepository.cs`

**Problema**: El `IReservationRepository` está implementado como `Singleton` con datos en memoria. Esto significa:
- Los datos no persisten entre reinicios del servicio
- No refleja el estado real del sistema de reservas
- Datos hardcodeados en el constructor

**Impacto**: En producción, no se puede validar correctamente si una reserva existe o está confirmada.

**Solución Recomendada**: 
- Integrar con API de Reservas vía HTTP Client
- O consumir eventos de dominio del microservicio de Reservas vía RabbitMQ
- O implementar sincronización de Read Model con Event Sourcing

---

### 2. **GetMetricsAsync - Query Ineficiente**
**Ubicación**: `Application/Services/ComplementaryServiceAppService.cs:205-219`

```csharp
public async Task<ServiceMetricsDto> GetMetricsAsync(...)
{
    var allServices = await _repository.GetAllAsync(cancellationToken); 
    // ⚠️ Carga TODOS los servicios en memoria
    
    return new ServiceMetricsDto {
        TotalRequests = allServices.Count,
        Confirmed = allServices.Count(s => s.Status == ServiceStatus.Confirmed),
        // ...
    };
}
```

**Problema**: 
- Carga todas las entidades en memoria para hacer agregaciones
- No escala con volumen de datos creciente
- Query O(n) ejecutada en Application Layer

**Impacto**: Con miles de servicios, este endpoint será lento y consumirá mucha memoria.

**Solución Recomendada**:
- Mover agregaciones a SQL: `SELECT COUNT(*) FROM ... WHERE Status = 'Confirmed'`
- O implementar un Read Model separado (CQRS) actualizado vía eventos
- O usar una tabla de métricas pre-calculadas

---

### 3. **Hardcoded Testing Fallback**
**Ubicación**: `Api/Controllers/ComplementaryServicesController.cs:30-32`

```csharp
var headerId = Request.Headers["X-User-Id"].ToString();
if (!string.IsNullOrEmpty(headerId)) return Guid.Parse(headerId);
```

**Problema**: Permite bypass de autenticación en cualquier entorno si se envía el header `X-User-Id`.

**Impacto**: Riesgo de seguridad si este código llega a producción sin protección.

**Solución Recomendada**:
- Condicionar este código solo para `Development`: 
  ```csharp
  if (app.Environment.IsDevelopment()) { /* allow X-User-Id */ }
  ```
- O eliminar completamente y usar siempre JWT

---

### 4. **Falta de Retry Policy en RabbitMQ Publisher**
**Ubicación**: `Infrastructure/Messaging/RabbitMQ/ServiceRequestPublisher.cs`

**Problema**: 
- Si RabbitMQ no está disponible al iniciar, la conexión falla sin reintentos
- `AutomaticRecoveryEnabled = true` solo recupera conexiones perdidas, no conexiones iniciales fallidas

**Impacto**: El servicio no se inicia si RabbitMQ tarda en levantar (race condition en Docker Compose).

**Solución Recomendada**:
- Implementar Polly Retry Policy con backoff exponencial
- O inicializar la conexión bajo demanda (lazy)

---

### 5. **Vulnerabilidad: Exception Swallowing**
**Ubicación**: `Api/Controllers/ComplementaryServicesController.cs:65-73`

```csharp
catch (Exception)
{
    return NotFound();
}
```

**Problema**: Cualquier excepción (incluso errores de base de datos o timeout) se traduce en `404 Not Found`, ocultando el problema real.

**Impacto**: 
- Dificulta debugging
- No se logean errores críticos
- Cliente no puede distinguir entre "no encontrado" y "error del servidor"

**Solución Recomendada**:
- Capturar excepciones específicas (`ServiceNotFoundException`)
- Dejar que otros errores se propaguen a un middleware global de error handling
- Loguear excepciones antes de devolver respuesta

---

### 6. **Sin Configuración de Dead Letter Queue (DLQ)**
**Ubicación**: RabbitMQ configuration

**Problema**: Si un mensaje no puede ser procesado (error de parseo, validación, etc.), se pierde o queda en loop infinito de reintentos.

**Impacto**: Mensajes "envenenados" pueden bloquear el consumidor.

**Solución Recomendada**:
- Configurar DLQ para cada cola
- Reenviar mensajes fallidos después de N reintentos
- Implementar monitoring de DLQ

---

## 📈 Posibles Mejoras Futuras

1. **Health Checks Detallados**: Incluir checks de RabbitMQ, PostgreSQL y SignalR en `/health`
2. **Observabilidad**: Integrar OpenTelemetry para tracing distribuido
3. **Rate Limiting**: Proteger endpoints de uso excesivo
4. **Idempotencia**: Garantizar que mensajes duplicados no creen servicios duplicados
5. **Saga Pattern**: Si el proceso de confirmación involucra múltiples servicios, usar Saga para rollback
6. **Eventual Consistency**: Implementar Outbox Pattern para garantizar publicación de eventos

---

## 🧪 Testing

La solución incluye proyectos de pruebas en `tests/`:
- **Domain.Tests**: Pruebas unitarias de entidades y lógica de dominio
- **Application.Tests**: Pruebas de Application Services con mocks
- **Infrastructure.IntegrationTests**: Pruebas de integración con PostgreSQL y RabbitMQ

**Comando para ejecutar tests**:
```bash
dotnet test
```
