# API Reference - Complementary Services

## 📡 Base URL

```
Local Development: http://localhost:5050
Production: [TBD]
```

**API Version**: `v1`  
**Base Path**: `/api/v1/ComplementaryServices`

---

## 🔐 Autenticación

Todos los endpoints requieren autenticación mediante **JWT Bearer Token** o header `X-User-Id` (solo desarrollo).

### Headers Requeridos

```http
Authorization: Bearer <JWT_TOKEN>
Content-Type: application/json
```

### Testing sin Keycloak (Solo Development)

```http
X-User-Id: <user-guid>
Content-Type: application/json
```

---

## 📋 Endpoints

### 1. Solicitar Servicio Complementario

Crea una nueva solicitud de servicio (Transporte, Catering o Merchandising).

**Endpoint**:
```http
POST /api/v1/ComplementaryServices/request
```

**Request Body**:
```json
{
  "reservationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "eventId": "7b8c9d12-3456-7890-abcd-ef1234567890",
  "serviceType": "Transport",
  "details": "Necesito transporte para 15 personas desde el hotel al evento a las 18:00"
}
```

**Campos**:
- `reservationId` (UUID, required): ID de la reserva confirmada
- `eventId` (UUID, required): ID del evento
- `serviceType` (string, required): Uno de `Transport`, `Catering`, `Merchandising`
- `details` (string, required): Detalles específicos del servicio

**Response**: `202 Accepted`
```json
{
  "serviceId": "9c7d6e5f-1234-5678-90ab-cdef12345678"
}
```

**Códigos de Estado**:
- `202 Accepted`: Solicitud aceptada y en proceso
- `400 Bad Request`: Datos inválidos o reserva no confirmada
- `401 Unauthorized`: Token inválido o faltante
- `404 Not Found`: Reserva no existe

**Location Header**: 
```
Location: /api/v1/ComplementaryServices/9c7d6e5f-1234-5678-90ab-cdef12345678
```

---

### 2. Obtener Estado de un Servicio

Consulta el estado actual de una solicitud de servicio específica.

**Endpoint**:
```http
GET /api/v1/ComplementaryServices/{serviceId}
```

**Path Parameters**:
- `serviceId` (UUID): ID del servicio a consultar

**Response**: `200 OK`
```json
{
  "serviceId": "9c7d6e5f-1234-5678-90ab-cdef12345678",
  "reservationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "serviceType": "Transport",
  "status": "Confirmed",
  "providerId": "TransportProvider_001",
  "price": 450.00,
  "requestedAt": "2024-01-15T10:30:00Z",
  "confirmedAt": "2024-01-15T10:32:15Z",
  "rejectedAt": null,
  "rejectionReason": null,
  "details": "Necesito transporte para 15 personas desde el hotel al evento a las 18:00"
}
```

**Códigos de Estado**:
- `200 OK`: Servicio encontrado
- `404 Not Found`: Servicio no existe o no pertenece al usuario
- `401 Unauthorized`: Token inválido

**Status Posibles**:
- `Requested`: Creado, no enviado a proveedor aún
- `Pending`: Enviado a proveedor, esperando respuesta
- `Confirmed`: Proveedor confirmó disponibilidad y precio
- `Rejected`: Proveedor rechazó (ver `rejectionReason`)
- `Cancelled`: Usuario canceló

---

### 3. Listar Mis Servicios

Obtiene todos los servicios del usuario autenticado, opcionalmente filtrados por reserva.

**Endpoint**:
```http
GET /api/v1/ComplementaryServices/my-services
GET /api/v1/ComplementaryServices/my-services?reservationId={uuid}
```

**Query Parameters**:
- `reservationId` (UUID, optional): Filtrar por reserva específica

**Response**: `200 OK`
```json
[
  {
    "serviceId": "9c7d6e5f-1234-5678-90ab-cdef12345678",
    "reservationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "serviceType": "Transport",
    "status": "Confirmed",
    "providerId": "TransportProvider_001",
    "price": 450.00,
    "requestedAt": "2024-01-15T10:30:00Z",
    "confirmedAt": "2024-01-15T10:32:15Z",
    "rejectedAt": null,
    "rejectionReason": null,
    "details": "Transporte para 15 personas"
  },
  {
    "serviceId": "a1b2c3d4-5678-90ab-cdef-123456789012",
    "reservationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "serviceType": "Catering",
    "status": "Rejected",
    "providerId": null,
    "price": 0.0,
    "requestedAt": "2024-01-15T11:00:00Z",
    "confirmedAt": null,
    "rejectedAt": "2024-01-15T11:05:30Z",
    "rejectionReason": "No disponible para la fecha solicitada",
    "details": "Catering vegetariano para 20 personas"
  }
]
```

**Códigos de Estado**:
- `200 OK`: Siempre (puede ser lista vacía)
- `401 Unauthorized`: Token inválido

---

### 4. Cancelar un Servicio

Cancela un servicio pendiente o solicitado (no se pueden cancelar servicios confirmados).

**Endpoint**:
```http
POST /api/v1/ComplementaryServices/{serviceId}/cancel
```

**Path Parameters**:
- `serviceId` (UUID): ID del servicio a cancelar

**Response**: `204 No Content`

**Códigos de Estado**:
- `204 No Content`: Servicio cancelado exitosamente
- `400 Bad Request`: No se puede cancelar (ya confirmado o ya cancelado)
- `401 Unauthorized`: Token inválido
- `404 Not Found`: Servicio no existe o no pertenece al usuario

---

### 5. Listar Servicios por Evento (Admin)

Obtiene todos los servicios asociados a un evento específico. Endpoint para administradores o gestores.

**Endpoint**:
```http
GET /api/v1/ComplementaryServices/by-event/{eventId}
```

**Path Parameters**:
- `eventId` (UUID): ID del evento

**Response**: `200 OK`
```json
[
  {
    "serviceId": "9c7d6e5f-1234-5678-90ab-cdef12345678",
    "reservationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "serviceType": "Transport",
    "status": "Confirmed",
    "providerId": "TransportProvider_001",
    "price": 450.00,
    "requestedAt": "2024-01-15T10:30:00Z",
    "confirmedAt": "2024-01-15T10:32:15Z",
    "rejectedAt": null,
    "rejectionReason": null,
    "details": "Transporte para 15 personas"
  }
]
```

**Códigos de Estado**:
- `200 OK`: Siempre (puede ser lista vacía)
- `401 Unauthorized`: Token inválido

**Nota**: Actualmente no hay control de permisos de admin. Todos los usuarios autenticados pueden acceder.

---

### 6. Obtener Métricas del Sistema (Dashboard)

Devuelve estadísticas agregadas de todos los servicios solicitados. Útil para dashboards administrativos.

**Endpoint**:
```http
GET /api/v1/ComplementaryServices/metrics
```

**Response**: `200 OK`
```json
{
  "totalRequests": 247,
  "confirmed": 189,
  "rejected": 35,
  "pending": 23,
  "averagePrice": 385.50,
  "byServiceType": {
    "Transport": 120,
    "Catering": 87,
    "Merchandising": 40
  }
}
```

**Campos**:
- `totalRequests`: Total de solicitudes creadas
- `confirmed`: Servicios confirmados por proveedores
- `rejected`: Servicios rechazados
- `pending`: Servicios en estado `Requested` o `Pending`
- `averagePrice`: Precio promedio de servicios confirmados
- `byServiceType`: Conteo por tipo de servicio

**Códigos de Estado**:
- `200 OK`: Siempre
- `401 Unauthorized`: Token inválido

**⚠️ Nota de Rendimiento**: Este endpoint carga todos los servicios en memoria. Ver [Deuda Técnica](./architecture.md#⚠️-deuda-técnica-detectada) para detalles.

---

## 🔔 SignalR Hub - Notificaciones en Tiempo Real

### Conexión

**Hub URL**: `/hubs/service-notifications`

**Ejemplo de Conexión (JavaScript)**:
```javascript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5050/hubs/service-notifications", {
    accessTokenFactory: () => localStorage.getItem("jwtToken")
  })
  .withAutomaticReconnect()
  .build();

// Escuchar actualizaciones de servicios
connection.on("ReceiveServiceUpdate", (serviceId, status, details) => {
  console.log(`Service ${serviceId} updated to ${status}`);
  console.log(`Details:`, details);
  
  // Actualizar UI
  updateServiceStatus(serviceId, status, details);
});

await connection.start();
console.log("Connected to SignalR hub");
```

### Eventos Recibidos

#### `ReceiveServiceUpdate`

**Parámetros**:
- `serviceId` (string): UUID del servicio
- `status` (string): Nuevo estado (`Confirmed`, `Rejected`)
- `details` (object): Detalles del servicio actualizado

**Ejemplo de Payload**:
```javascript
{
  serviceId: "9c7d6e5f-1234-5678-90ab-cdef12345678",
  status: "Confirmed",
  details: {
    providerId: "TransportProvider_001",
    price: 450.00,
    confirmedAt: "2024-01-15T10:32:15Z",
    message: "Confirmado con éxito. Conductor: Juan Pérez"
  }
}
```

**Cuándo se Envía**:
- Cuando un proveedor confirma el servicio
- Cuando un proveedor rechaza el servicio

**Destinatarios**: Solo el usuario dueño del servicio recibe la notificación (filtrado por `userId` del JWT).

---

## 📝 Modelos de Datos

### ServiceRequestDto (Request)

```json
{
  "reservationId": "uuid",
  "eventId": "uuid",
  "serviceType": "Transport | Catering | Merchandising",
  "details": "string"
}
```

### ServiceStatusDto (Response)

```json
{
  "serviceId": "uuid",
  "reservationId": "uuid",
  "serviceType": "string",
  "status": "Requested | Pending | Confirmed | Rejected | Cancelled",
  "providerId": "string | null",
  "price": 0.0,
  "requestedAt": "2024-01-15T10:30:00Z",
  "confirmedAt": "2024-01-15T10:32:15Z | null",
  "rejectedAt": "2024-01-15T11:05:30Z | null",
  "rejectionReason": "string | null",
  "details": "string"
}
```

### ServiceMetricsDto (Response)

```json
{
  "totalRequests": 0,
  "confirmed": 0,
  "rejected": 0,
  "pending": 0,
  "averagePrice": 0.0,
  "byServiceType": {
    "Transport": 0,
    "Catering": 0,
    "Merchandising": 0
  }
}
```

---

## 🧪 Ejemplos de Uso Completo

### Escenario: Usuario Solicita Transporte

#### 1. Solicitar Servicio

```bash
curl -X POST http://localhost:5050/api/v1/ComplementaryServices/request \
  -H "X-User-Id: 12345678-1234-1234-1234-123456789012" \
  -H "Content-Type: application/json" \
  -d '{
    "reservationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "eventId": "7b8c9d12-3456-7890-abcd-ef1234567890",
    "serviceType": "Transport",
    "details": "Transporte para 15 personas desde Hotel Plaza a Centro de Convenciones, salida 17:30"
  }'
```

**Response**:
```json
HTTP/1.1 202 Accepted
Location: /api/v1/ComplementaryServices/9c7d6e5f-1234-5678-90ab-cdef12345678
Content-Type: application/json

{
  "serviceId": "9c7d6e5f-1234-5678-90ab-cdef12345678"
}
```

#### 2. Consultar Estado (Polling)

```bash
curl -X GET http://localhost:5050/api/v1/ComplementaryServices/9c7d6e5f-1234-5678-90ab-cdef12345678 \
  -H "X-User-Id: 12345678-1234-1234-1234-123456789012"
```

**Response** (inmediatamente después de crear):
```json
{
  "serviceId": "9c7d6e5f-1234-5678-90ab-cdef12345678",
  "reservationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "serviceType": "Transport",
  "status": "Requested",
  "providerId": null,
  "price": 0.0,
  "requestedAt": "2024-01-15T10:30:00Z",
  "confirmedAt": null,
  "rejectedAt": null,
  "rejectionReason": null,
  "details": "Transporte para 15 personas..."
}
```

**Response** (después de confirmación por proveedor):
```json
{
  "serviceId": "9c7d6e5f-1234-5678-90ab-cdef12345678",
  "reservationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "serviceType": "Transport",
  "status": "Confirmed",
  "providerId": "TransportProvider_001",
  "price": 450.00,
  "requestedAt": "2024-01-15T10:30:00Z",
  "confirmedAt": "2024-01-15T10:32:15Z",
  "rejectedAt": null,
  "rejectionReason": null,
  "details": "Transporte para 15 personas..."
}
```

#### 3. Notificación SignalR (Recibida Automáticamente)

```javascript
// Cliente recibe este evento cuando proveedor confirma:
connection.on("ReceiveServiceUpdate", (serviceId, status, details) => {
  // serviceId: "9c7d6e5f-1234-5678-90ab-cdef12345678"
  // status: "Confirmed"
  // details: { providerId: "TransportProvider_001", price: 450.00, ... }
});
```

---

### Escenario: Listar Servicios de una Reserva

```bash
curl -X GET "http://localhost:5050/api/v1/ComplementaryServices/my-services?reservationId=3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "X-User-Id: 12345678-1234-1234-1234-123456789012"
```

**Response**:
```json
[
  {
    "serviceId": "9c7d6e5f-1234-5678-90ab-cdef12345678",
    "serviceType": "Transport",
    "status": "Confirmed",
    "price": 450.00
  },
  {
    "serviceId": "a1b2c3d4-5678-90ab-cdef-123456789012",
    "serviceType": "Catering",
    "status": "Pending",
    "price": 0.0
  }
]
```

---

### Escenario: Cancelar Servicio Pendiente

```bash
curl -X POST http://localhost:5050/api/v1/ComplementaryServices/a1b2c3d4-5678-90ab-cdef-123456789012/cancel \
  -H "X-User-Id: 12345678-1234-1234-1234-123456789012"
```

**Response**:
```
HTTP/1.1 204 No Content
```

---

## 🚨 Manejo de Errores

### Formato de Errores

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Cannot request services for unconfirmed reservation",
  "traceId": "00-abc123..."
}
```

### Códigos de Error Comunes

| Código | Descripción | Solución |
|--------|-------------|----------|
| `400 Bad Request` | Datos inválidos o regla de negocio violada | Verificar datos enviados y estado de reserva |
| `401 Unauthorized` | Token faltante o inválido | Incluir header Authorization con JWT válido |
| `404 Not Found` | Recurso no existe o no pertenece al usuario | Verificar ID y permisos |
| `500 Internal Server Error` | Error del servidor | Contactar soporte, revisar logs |

---

## 📊 Rate Limiting

**Nota**: Actualmente no implementado. En producción se recomienda limitar a:
- 100 requests/minuto por usuario
- 10 solicitudes de servicio/minuto por usuario

---

## 🔗 Enlaces Relacionados

- [Swagger UI](http://localhost:5050/swagger) - Documentación interactiva
- [Health Check](http://localhost:5050/health) - Estado del servicio
- [Arquitectura](./architecture.md) - Detalles internos del sistema
