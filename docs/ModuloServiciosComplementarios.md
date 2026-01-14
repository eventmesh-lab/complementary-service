# Especificación de Requerimientos del Software (ERS)
## Módulo: Servicios Complementarios

**Proyecto:** Plataforma Integral para Gestión de Eventos
**Versión:** 1.0
**Fecha:** Enero 2026

---

## 1. Introducción

### 1.1 Propósito
Este documento especifica los requerimientos funcionales y no funcionales del módulo de Servicios Complementarios, que permite a los usuarios contratar servicios adicionales (transporte, catering, merchandising) asociados a sus reservas de eventos.

### 1.2 Alcance
El sistema gestionará:
- Solicitudes de servicios complementarios
- Integración asíncrona con proveedores externos vía RabbitMQ
- Notificaciones en tiempo real mediante SignalR
- Auditoría completa de transacciones

### 1.3 Definiciones y Acrónimos
- **DDD**: Domain-Driven Design
- **CQRS**: Command Query Responsibility Segregation
- **EDA**: Event-Driven Architecture
- **API**: Application Programming Interface
- **DTO**: Data Transfer Object

---

## 2. Descripción General

### 2.1 Perspectiva del Producto
El módulo de Servicios Complementarios es parte de una plataforma de gestión de eventos que integra:
- Gestión de Eventos
- Gestión de Reservas
- Sistema de Pagos
- Sistema de Notificaciones

### 2.2 Funciones del Producto
1. Contratación de servicios complementarios
2. Integración con proveedores externos
3. Notificaciones en tiempo real
4. Consulta de estado de servicios
5. Gestión administrativa de servicios

### 2.3 Características de los Usuarios

| Actor | Descripción | Responsabilidades |
|-------|-------------|-------------------|
| Usuario Final | Cliente que reserva entradas | Solicitar y consultar servicios |
| Organizador | Responsable del evento | Monitorear servicios contratados |
| Administrador | Operador de la plataforma | Supervisar y gestionar servicios |
| Sistema Externo | Proveedor de servicios | Confirmar/rechazar solicitudes |

---

## 3. Requerimientos Funcionales

### RF-001: Solicitud de Servicio Complementario
**Prioridad:** Alta  
**Caso de Uso:** TC-060

**Descripción:** El sistema debe permitir a un usuario autenticado solicitar un servicio complementario para una reserva confirmada.

**Precondiciones:**
- Usuario autenticado
- Reserva existente y confirmada
- Servicio solicitado disponible en el sistema

**Flujo Principal:**
1. Usuario selecciona una reserva confirmada
2. Usuario selecciona tipo de servicio (Transporte, Catering, Merchandising)
3. Usuario ingresa detalles específicos del servicio
4. Sistema valida la reserva y pertenencia al usuario
5. Sistema crea entidad ComplementaryService con estado "Requested"
6. Sistema persiste en base de datos
7. Sistema publica evento de dominio ServiceRequestedDomainEvent
8. Sistema retorna ID del servicio creado

**Postcondiciones:**
- Servicio creado en estado "Requested"
- Evento publicado para procesamiento asíncrono
- Usuario recibe confirmación de solicitud

**Excepciones:**
- E-001: Reserva no encontrada
- E-002: Reserva no pertenece al usuario
- E-003: Reserva no confirmada
- E-004: Tipo de servicio inválido

---

### RF-002: Integración con Proveedores vía RabbitMQ
**Prioridad:** Alta  
**Caso de Uso:** TC-061

**Descripción:** El sistema debe publicar solicitudes de servicios a RabbitMQ y consumir respuestas de proveedores externos.

**Componentes:**
1. **Publisher (ServiceRequestPublisher)**
   - Publica mensaje a exchange "services.requests"
   - Routing key según tipo de servicio
   - Mensaje incluye: ServiceId, ReservationId, EventId, ServiceType, Details

2. **Consumer (ServiceResponseConsumer)**
   - Consume mensajes de cola "services.responses.platform"
   - Procesa respuestas de proveedores
   - Actualiza estado del servicio

**Arquitectura de Mensajería:**
```
Exchange: services.requests (Type: Topic)
├── Queue: transport.requests (Routing: service.request.transport)
├── Queue: catering.requests (Routing: service.request.catering)
└── Queue: merchandising.requests (Routing: service.request.merchandising)

Exchange: services.responses (Type: Topic)
└── Queue: services.responses.platform (Routing: service.response)
```

**Formato de Mensaje Request:**
```json
{
  "serviceId": "uuid",
  "reservationId": "uuid",
  "eventId": "uuid",
  "serviceType": "Transport|Catering|Merchandising",
  "details": "string",
  "requestedAt": "datetime",
  "callbackQueue": "services.responses.platform"
}
```

**Formato de Mensaje Response:**
```json
{
  "serviceId": "uuid",
  "isAvailable": true|false,
  "providerId": "string",
  "message": "string",
  "price": decimal,
  "estimatedTime": "datetime?",
  "rejectionReason": "string?"
}
```

**Manejo de Errores:**
- Reintentos automáticos: 3 intentos con backoff exponencial
- Dead Letter Queue para mensajes fallidos
- Logging completo de errores
- Notificación a administradores en caso de fallos críticos

---

### RF-003: Notificaciones en Tiempo Real
**Prioridad:** Alta  
**Caso de Uso:** TC-062

**Descripción:** El sistema debe notificar al usuario en tiempo real sobre cambios en el estado de sus servicios mediante SignalR.

**Eventos de Notificación:**
1. **ServiceConfirmed**
   - Dispara: ServiceConfirmedDomainEvent
   - Contenido: ServiceId, ServiceType, ProviderId, Price
   - Hub Method: "ServiceNotification"

2. **ServiceRejected**
   - Dispara: ServiceRejectedDomainEvent
   - Contenido: ServiceId, ServiceType, RejectionReason
   - Hub Method: "ServiceNotification"

3. **ServiceUpdated**
   - Dispara: Cambios manuales de estado
   - Contenido: ServiceId, Status, Message
   - Hub Method: "ServiceNotification"

**Configuración SignalR:**
- Endpoint: /hubs/service-notifications
- Autenticación: JWT Bearer Token (Keycloak)
- KeepAlive: 15 segundos
- Timeout: 30 segundos
- Transporte: WebSockets (fallback: Server-Sent Events, Long Polling)

**Conexión del Cliente:**
```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/service-notifications", {
    accessTokenFactory: () => getAccessToken()
  })
  .withAutomaticReconnect()
  .build();

connection.on("ServiceNotification", (notification) => {
  console.log(notification);
  // Actualizar UI
});
```

---

### RF-004: Consulta de Estado de Servicios
**Prioridad:** Media

**Descripción:** Usuarios pueden consultar el estado de sus servicios contratados.

**Endpoints:**
- `GET /api/v1/complementaryservices/{id}` - Estado de un servicio específico
- `GET /api/v1/complementaryservices/my-services?reservationId={id}` - Servicios del usuario

**Respuesta:**
```json
{
  "serviceId": "uuid",
  "reservationId": "uuid",
  "serviceType": "Transport",
  "status": "Confirmed|Rejected|Pending|Requested",
  "providerId": "string",
  "price": 50.00,
  "requestedAt": "datetime",
  "confirmedAt": "datetime?",
  "rejectedAt": "datetime?",
  "rejectionReason": "string?",
  "details": "string"
}
```

---

### RF-005: Cancelación de Servicios
**Prioridad:** Media

**Descripción:** Usuarios pueden cancelar servicios solicitados que aún no han sido confirmados.

**Reglas:**
- Solo se pueden cancelar servicios en estado "Requested" o "Pending"
- Servicios confirmados no se pueden cancelar vía API (contacto con soporte)
- Se publica evento ServiceCancelledDomainEvent

---

### RF-006: Panel Administrativo
**Prioridad:** Media

**Descripción:** Administradores y organizadores pueden consultar servicios por evento y obtener métricas.

**Endpoints:**
- `GET /api/v1/complementaryservices/by-event/{eventId}` - [Roles: Admin, Organizer]
- `GET /api/v1/complementaryservices/metrics` - [Roles: Admin, Organizer]

**Métricas:**
- Total de solicitudes
- Servicios confirmados/rechazados/pendientes
- Distribución por tipo de servicio
- Precio promedio
- Tasa de confirmación por proveedor

---

## 4. Requerimientos No Funcionales

### RNF-001: Rendimiento
- Tiempo de respuesta API: < 200ms (p95)
- Latencia SignalR: < 100ms
- Procesamiento mensajes RabbitMQ: < 500ms
- Throughput: 1000 solicitudes/minuto

### RNF-002: Disponibilidad
- Uptime: 99.5% mensual
- RabbitMQ: Alta disponibilidad con clustering
- SignalR: Reconnection automático

### RNF-003: Seguridad
- Autenticación: Keycloak JWT
- Autorización: Role-Based Access Control (RBAC)
- Comunicación: HTTPS/WSS obligatorio en producción
- Validación: Input validation en todos los endpoints
- Encriptación: Datos sensibles en tránsito y reposo

### RNF-004: Escalabilidad
- Arquitectura stateless
- Horizontal scaling ready
- Database connection pooling
- RabbitMQ con múltiples consumers

### RNF-005: Mantenibilidad
- Cobertura de pruebas: ≥ 90%
- Documentación: XML docs en código
- Logs estructurados (JSON)
- Métricas: Health checks en /health

### RNF-006: Observabilidad
- Logging: Serilog con sinks a ElasticSearch
- Métricas: Prometheus compatible
- Tracing: OpenTelemetry
- Dashboard: Grafana

---

## 5. Modelo de Datos

### Entidad: ComplementaryService

| Campo | Tipo | Descripción | Restricciones |
|-------|------|-------------|---------------|
| Id | Guid | Identificador único | PK |
| ReservationId | Guid | Reserva asociada | FK, NOT NULL, Index |
| UserId | Guid | Usuario solicitante | NOT NULL, Index |
| EventId | Guid | Evento asociado | NOT NULL, Index |
| ServiceType | String | Tipo de servicio | NOT NULL, MAX(50) |
| Status | String | Estado actual | NOT NULL, MAX(50) |
| ProviderId | String | ID del proveedor | MAX(100) |
| Price | Decimal | Precio final | Precision(18,2) |
| Details | String | Detalles solicitud | MAX(1000) |
| RequestedAt | DateTime | Fecha solicitud | NOT NULL |
| ConfirmedAt | DateTime? | Fecha confirmación | Nullable |
| RejectedAt | DateTime? | Fecha rechazo | Nullable |
| RejectionReason | String | Motivo rechazo | MAX(500) |

**Índices:**
- IX_ComplementaryServices_UserId
- IX_ComplementaryServices_ReservationId
- IX_ComplementaryServices_EventId
- IX_ComplementaryServices_UserId_ReservationId (compuesto)

---

## 6. Casos de Uso Detallados

### CU-060: Contratación de Servicio

**Actor Principal:** Usuario Final

**Flujo Normal:**
1. Usuario navega a detalles de reserva confirmada
2. Usuario selecciona "Agregar Servicio Complementario"
3. Sistema muestra tipos de servicios disponibles
4. Usuario selecciona tipo (ej: Transporte)
5. Usuario completa formulario con detalles
6. Usuario confirma solicitud
7. Sistema valida datos
8. Sistema crea servicio con estado "Requested"
9. Sistema envía mensaje a RabbitMQ
10. Sistema muestra confirmación "Solicitud enviada"
11. Sistema envía notificación en tiempo real cuando hay respuesta

**Flujos Alternativos:**
- 7a. Datos inválidos → Sistema muestra errores
- 8a. Error de persistencia → Sistema muestra error genérico, logea detalles
- 9a. RabbitMQ no disponible → Sistema encola para reintento

---

### CU-061: Procesamiento Asíncrono

**Actor Principal:** Sistema (Background)

**Flujo Normal:**
1. ServiceRequestedEventHandler detecta evento
2. Handler obtiene datos del servicio
3. ServiceRequestPublisher publica a RabbitMQ
4. Proveedor externo consume mensaje
5. Proveedor procesa solicitud
6. Proveedor publica respuesta
7. ServiceResponseConsumer consume respuesta
8. Consumer invoca ConfirmServiceCommand o RejectServiceCommand
9. Sistema actualiza estado del servicio
10. Sistema publica ServiceConfirmedDomainEvent o ServiceRejectedDomainEvent

**Monitoreo:**
- Dead Letter Queue para mensajes fallidos
- Alertas si tiempo de procesamiento > 30 segundos
- Dashboard con métricas de cola

---

### CU-062: Notificación en Tiempo Real

**Actor Principal:** Sistema (Event Handler)

**Flujo Normal:**
1. ServiceConfirmedEventHandler detecta evento
2. Handler obtiene UserId del servicio
3. SignalRServiceNotifier construye notificación
4. Notifier envía a HubContext
5. HubContext identifica conexiones del usuario
6. SignalR envía mensaje a cliente
7. Cliente recibe y muestra notificación
8. Usuario ve actualización en UI

**Reconexión:**
- Cliente desconectado → SignalR reintenta cada 5s
- Máximo 10 reintentos
- Si falla persistentemente → Usuario debe refrescar página

---

## 7. Arquitectura Técnica

### 7.1 Capas del Sistema

```
┌─────────────────────────────────────┐
│         API Layer (REST)            │
│   - ComplementaryServicesController │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│      Application Layer (CQRS)       │
│   - Commands & Handlers             │
│   - Queries & Handlers              │
│   - Event Handlers                  │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│         Domain Layer (DDD)          │
│   - Entities & Aggregates           │
│   - Value Objects                   │
│   - Domain Events                   │
│   - Repository Interfaces           │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│     Infrastructure Layer            │
│   - EF Core Repositories            │
│   - RabbitMQ Integration            │
│   - SignalR Hubs                    │
│   - External Provider Adapters      │
└─────────────────────────────────────┘
```

### 7.2 Tecnologías

- **Framework:** .NET 8
- **ORM:** Entity Framework Core
- **Base de Datos:** PostgreSQL
- **Mensajería:** RabbitMQ
- **Tiempo Real:** SignalR
- **Autenticación:** Keycloak
- **Mediator:** MediatR
- **Testing:** xUnit, Moq, FluentAssertions
- **Documentación:** Swagger/OpenAPI

---

## 8. Pruebas

### 8.1 Estrategia de Pruebas

**Cobertura Objetivo:** 90%

**Tipos de Pruebas:**
1. **Unitarias** (70% del total)
   - Entities y Value Objects
   - Command Handlers
   - Domain Event Handlers
   - Services

2. **Integración** (20% del total)
   - Repositorios con PostgreSQL (TestContainers)
   - RabbitMQ Publisher/Consumer
   - API Endpoints

3. **E2E** (10% del total)
   - Flujo completo: Solicitud → RabbitMQ → Confirmación → Notificación

### 8.2 Casos de Prueba Críticos

| ID | Descripción | Tipo | Prioridad |
|----|-------------|------|-----------|
| UT-001 | Crear servicio con datos válidos | Unit | Alta |
| UT-002 | Confirmar servicio cambia estado | Unit | Alta |
| UT-003 | Rechazar servicio con razón | Unit | Alta |
| IT-001 | Publicar mensaje a RabbitMQ | Integration | Alta |
| IT-002 | Consumir respuesta de proveedor | Integration | Alta |
| IT-003 | Enviar notificación SignalR | Integration | Alta |
| E2E-001 | Flujo completo solicitud-confirmación | E2E | Crítica |

---

## 9. Despliegue

### 9.1 Ambientes

| Ambiente | Propósito | URL Base |
|----------|-----------|----------|
| Desarrollo | Testing local | http://localhost:5000 |
| Staging | Pre-producción | https://staging-api.eventplatform.com |
| Producción | Operación real | https://api.eventplatform.com |

### 9.2 Requisitos de Infraestructura

**Por Instancia:**
- CPU: 2 cores
- RAM: 4 GB
- Disco: 20 GB SSD

**Servicios Externos:**
- PostgreSQL: 11+
- RabbitMQ: 3.12+
- Keycloak: 22+

### 9.3 Configuración

Variables de entorno requeridas:
- `ConnectionStrings__DefaultConnection`
- `Keycloak__Authority`
- `Keycloak__Audience`
- `RabbitMQ__HostName`
- `RabbitMQ__UserName`
- `RabbitMQ__Password`

---

## 10. Auditoría y Logs

### 10.1 Eventos Auditables

| Evento | Nivel | Información Registrada |
|--------|-------|------------------------|
| Servicio solicitado | Info | UserId, ServiceId, ServiceType, Timestamp |
| Servicio confirmado | Info | ServiceId, ProviderId, Price |
| Servicio rechazado | Warning | ServiceId, Reason |
| Error RabbitMQ | Error | ServiceId, Exception, StackTrace |
| Notificación fallida | Warning | UserId, ServiceId, Reason |

### 10.2 Almacenamiento de Logs

- **Destino:** MongoDB (vía Serilog sink)
- **Retención:** 90 días
- **Formato:** JSON estructurado
- **Consulta:** ElasticSearch + Kibana

---

## 11. Mantenimiento y Soporte

### 11.1 Procedimientos Operacionales

**Monitoreo Diario:**
- Revisar métricas de RabbitMQ (profundidad de colas)
- Verificar tasa de éxito de notificaciones
- Analizar logs de errores

**Mantenimiento Semanal:**
- Limpieza de Dead Letter Queues
- Revisión de performance de queries
- Actualización de dependencias críticas

### 11.2 SLA

- **Tiempo de Respuesta a Incidentes Críticos:** 1 hora
- **Tiempo de Resolución P1:** 4 horas
- **Tiempo de Resolución P2:** 24 horas
- **Tiempo de Resolución P3:** 72 horas

---

## 12. Apéndices

### A. Diagrama de Estados

```
[Requested] → [Pending] → [Confirmed]
     ↓            ↓
[Cancelled]  [Rejected]
```

### B. Glosario

- **Servicio Complementario:** Servicio adicional contratado junto con una reserva
- **Proveedor:** Entidad externa que suministra servicios
- **Hub (SignalR):** Servidor de comunicación en tiempo real
- **Exchange (RabbitMQ):** Router de mensajes
- **Queue:** Cola de mensajes FIFO
