# Guía de Integración: Frontend React con Complementary Service API

Esta guía explica cómo un frontend en React puede consumir el microservicio de Servicios Complementarios, incluyendo llamadas HTTP (axios/fetch) y notificaciones en tiempo real con SignalR.

## Base URL y CORS
- Base de la API: `http://localhost:5000` (ajusta al puerto donde corre la API)
- Prefijo de rutas: `/api/v1/ComplementaryServices`
- CORS: la API viene configurada para permitir `http://localhost:3000` y `http://localhost:5173`. Si tu frontend usa otro origen, agrega ese dominio en AllowedOrigins en [src/complementary-service.Api/appsettings.json](src/complementary-service.Api/appsettings.json).

## Autenticación y encabezados
- Producción: usa `Authorization: Bearer <token JWT>` (Keycloak/tu IdP).
- Desarrollo local: si no tienes JWT, puedes pasar `X-User-Id: <GUID>` como encabezado. La API lo acepta solo para pruebas.

## DTOs principales
- `ServiceRequestDto`:
  - `reservationId: string (GUID)`
  - `eventId: string (GUID)`
  - `serviceType: string` (ej. `transport`, `catering`, `merchandising`)
  - `details: string`
- `ServiceStatusDto`:
  - `serviceId, reservationId: GUID`
  - `serviceType: string`
  - `status: string` (ej. `Pending`, `Confirmed`, `Rejected`)
  - `providerId: string`
  - `price: number`
  - `requestedAt: string (ISO)`
  - `confirmedAt?: string (ISO)`
  - `rejectedAt?: string (ISO)`
  - `rejectionReason?: string`
  - `details?: string`
- `ServiceMetricsDto`:
  - `totalRequests, confirmed, rejected, pending: number`
  - `averagePrice: number`
  - `byServiceType: Record<string, number>`

## Endpoints disponibles
- POST `/{base}/request`: crear una solicitud de servicio. Responde `202 Accepted` y `{ ServiceId: <GUID> }`.
- GET `/{base}/{serviceId}`: obtener estado de un servicio (del usuario actual).
- GET `/{base}/my-services?reservationId=<GUID>`: lista de servicios del usuario (filtrable por reserva).
- POST `/{base}/{serviceId}/cancel`: cancelar un servicio pendiente.
- GET `/{base}/by-event/{eventId}`: lista por evento (rol de gestor/administrador).
- GET `/{base}/metrics`: métricas globales.

Reemplaza `/{base}` por `/api/v1/ComplementaryServices`.

## Configuración en React
Define variables de entorno para la API y token.

```env
REACT_APP_API_URL=http://localhost:5000
REACT_APP_SIGNALR_URL=http://localhost:5000/hubs/service-notifications
REACT_APP_DEV_USER_ID=00000000-0000-0000-0000-000000000001
```

### Cliente axios
```ts
// src/api/client.ts
import axios from 'axios';

const api = axios.create({
  baseURL: process.env.REACT_APP_API_URL + '/api/v1/ComplementaryServices',
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('access_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  } else if (process.env.REACT_APP_DEV_USER_ID) {
    config.headers['X-User-Id'] = process.env.REACT_APP_DEV_USER_ID;
  }
  return config;
});

export default api;
```

### Crear solicitud de servicio
```ts
// src/api/services.ts
import api from './client';

export type ServiceRequestDto = {
  reservationId: string;
  eventId: string;
  serviceType: 'transport' | 'catering' | 'merchandising' | string;
  details: string;
};

export async function requestService(payload: ServiceRequestDto) {
  const { data, status } = await api.post('/request', payload);
  if (status !== 202) throw new Error('Solicitud no aceptada');
  return data as { ServiceId: string };
}

export async function getServiceStatus(serviceId: string) {
  const { data } = await api.get(`/${serviceId}`);
  return data;
}

export async function getMyServices(reservationId?: string) {
  const { data } = await api.get('/my-services', { params: { reservationId } });
  return data;
}

export async function cancelService(serviceId: string) {
  const { status } = await api.post(`/${serviceId}/cancel`);
  if (status !== 204) throw new Error('No se pudo cancelar');
}

export async function getByEvent(eventId: string) {
  const { data } = await api.get(`/by-event/${eventId}`);
  return data;
}

export async function getMetrics() {
  const { data } = await api.get('/metrics');
  return data;
}
```

### Ejemplo de uso en un componente React
```tsx
// src/components/RequestServiceForm.tsx
import React, { useState } from 'react';
import { requestService, getServiceStatus } from '../api/services';

export default function RequestServiceForm() {
  const [reservationId, setReservationId] = useState('');
  const [eventId, setEventId] = useState('');
  const [serviceType, setServiceType] = useState('transport');
  const [details, setDetails] = useState('');
  const [serviceId, setServiceId] = useState<string | null>(null);
  const [status, setStatus] = useState<any>(null);

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const res = await requestService({ reservationId, eventId, serviceType, details });
    setServiceId(res.ServiceId);
    const st = await getServiceStatus(res.ServiceId);
    setStatus(st);
  };

  return (
    <div>
      <form onSubmit={onSubmit}>
        <input value={reservationId} onChange={(e) => setReservationId(e.target.value)} placeholder="ReservationId (GUID)" />
        <input value={eventId} onChange={(e) => setEventId(e.target.value)} placeholder="EventId (GUID)" />
        <select value={serviceType} onChange={(e) => setServiceType(e.target.value)}>
          <option value="transport">Transport</option>
          <option value="catering">Catering</option>
          <option value="merchandising">Merchandising</option>
        </select>
        <textarea value={details} onChange={(e) => setDetails(e.target.value)} placeholder="Detalles" />
        <button type="submit">Solicitar</button>
      </form>
      {serviceId && <p>ServiceId: {serviceId}</p>}
      {status && <pre>{JSON.stringify(status, null, 2)}</pre>}
    </div>
  );
}
```

## Notificaciones en tiempo real (SignalR)
El backend expone un hub en `/hubs/service-notifications`. Envía eventos con `SendAsync("ServiceNotification", payload)`, donde `payload.Type` puede ser:
- `ServiceConfirmed`: confirmado con `providerId` y `price`.
- `ServiceRejected`: rechazado con `reason`.
- `ServiceUpdated`: actualización genérica de estado.

### Cliente con `@microsoft/signalr`
```ts
// src/realtime/signalr.ts
import * as signalR from '@microsoft/signalr';

export function createServiceHubConnection() {
  const token = localStorage.getItem('access_token');
  const hubUrl = process.env.REACT_APP_SIGNALR_URL!;

  const connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl, {
      accessTokenFactory: token ? () => token! : undefined,
      withCredentials: true,
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

  return connection;
}
```

### Uso en React
```tsx
// src/components/ServiceNotifications.tsx
import React, { useEffect, useState } from 'react';
import { createServiceHubConnection } from '../realtime/signalr';

type Notification = {
  Type: 'ServiceConfirmed' | 'ServiceRejected' | 'ServiceUpdated';
  ServiceId: string;
  ServiceType?: string;
  ProviderId?: string;
  Price?: number;
  Reason?: string;
  Status?: string;
  Message: string;
  Timestamp: string;
};

export default function ServiceNotifications() {
  const [notifications, setNotifications] = useState<Notification[]>([]);

  useEffect(() => {
    const connection = createServiceHubConnection();

    connection.on('ServiceNotification', (payload: Notification) => {
      setNotifications((prev) => [payload, ...prev]);
    });

    connection.start().catch(console.error);
    return () => {
      connection.stop();
    };
  }, []);

  return (
    <ul>
      {notifications.map((n, i) => (
        <li key={i}>
          [{n.Type}] {n.Message} (Servicio: {n.ServiceId})
        </li>
      ))}
    </ul>
  );
}
```

## Manejo de errores
- `401 Unauthorized`: faltan credenciales. Usa Bearer token o `X-User-Id` en desarrollo.
- `404 Not Found`: `serviceId` inexistente o no pertenece al usuario.
- `400 Bad Request`: datos inválidos o no se puede cancelar (ya confirmado/rechazado).

## Consejos de integración
- Persistir el `serviceId` tras `POST /request` para consultar estado y escuchar notificaciones.
- Mostrar feedback inmediato usando el `202 Accepted` y luego consultar `GET /{serviceId}` hasta recibir notificación.
- Para dashboards, usar `GET /metrics` y `GET /by-event/{eventId}`.

## Comprobación rápida
1. Levanta la API y base de datos (ver [docs/docker_readme.md](docs/docker_readme.md)).
2. Desde React, prueba `POST /request` y confirma que recibes `ServiceId`.
3. Abre el componente de notificaciones y verifica recepción de eventos en tiempo real.

---

Referencias del backend:
- Controlador: [src/complementary-service.Api/Controllers/ComplementaryServicesController.cs](src/complementary-service.Api/Controllers/ComplementaryServicesController.cs)
- Hub de SignalR: [src/complementary-service.Infrastructure/Notifications/ServiceNotificationHub.cs](src/complementary-service.Infrastructure/Notifications/ServiceNotificationHub.cs)
- Notificador: [src/complementary-service.Infrastructure/Notifications/SignalRServiceNotifier.cs](src/complementary-service.Infrastructure/Notifications/SignalRServiceNotifier.cs)