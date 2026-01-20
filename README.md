# Complementary Services - Microservicio de Servicios Complementarios

## 📋 Descripción

Microservicio que gestiona la solicitud, confirmación y seguimiento de **servicios complementarios** (Transporte, Catering, Merchandising) para eventos en una plataforma de gestión de eventos. Cuando un usuario confirma una reserva para un evento, puede solicitar servicios adicionales que son procesados de forma asíncrona a través de proveedores externos.

### Problema de Negocio que Resuelve

- **Orquestación de servicios complementarios**: Centraliza las solicitudes de servicios adicionales y coordina con múltiples proveedores externos.
- **Comunicación asíncrona**: Utiliza mensajería (RabbitMQ) para enviar solicitudes a proveedores externos y recibir confirmaciones o rechazos.
- **Notificaciones en tiempo real**: Informa a los usuarios del estado de sus servicios mediante SignalR.
- **Trazabilidad completa**: Registra el ciclo de vida completo de cada solicitud (Requested → Pending → Confirmed/Rejected/Cancelled).

---

## 📚 Tabla de Contenidos

- **[Arquitectura](./docs/architecture.md)** - Flujo de datos, dependencias externas y modelo de dominio
- **[API Reference](./docs/api.md)** - Documentación de endpoints y ejemplos de uso
- **[Setup & Configuration](./docs/setup.md)** - Guía detallada de instalación y configuración

---

## 🛠 Stack Tecnológico

| Categoría | Tecnología |
|-----------|-----------|
| **Framework** | .NET 8.0 (ASP.NET Core) |
| **Arquitectura** | Hexagonal (Ports & Adapters) + DDD |
| **Base de Datos** | PostgreSQL 16 |
| **Mensajería** | RabbitMQ 3.12 |
| **Notificaciones** | SignalR (WebSockets) |
| **Logs** | MongoDB 7 (opcional) |
| **Autenticación** | Keycloak (JWT) |
| **Orquestación** | Docker Compose |
| **Patrones** | CQRS, Event Sourcing, MediatR |

---

## 🚀 Quick Start

### Prerequisitos
- Docker & Docker Compose
- .NET 8.0 SDK (solo para desarrollo local sin Docker)

### Iniciar el servicio completo

```bash
# Clonar el repositorio
git clone <repository-url>
cd complementary-service

# Copiar variables de entorno
cp .env.example .env

# Iniciar todos los servicios con Docker Compose
docker-compose up -d

# Verificar que los servicios estén corriendo
docker-compose ps
```

### Acceso a Servicios

| Servicio | URL | Credenciales |
|----------|-----|--------------|
| API (Swagger) | http://localhost:5050/swagger | N/A |
| API Health | http://localhost:5050/health | N/A |
| RabbitMQ Management | http://localhost:15675 | guest/guest |
| PostgreSQL | localhost:5436 | postgres/postgres |
| MongoDB | localhost:27018 | admin/admin |

### Comandos Útiles

```bash
# Ver logs de la API
docker-compose logs -f api

# Detener servicios
docker-compose down

# Reiniciar un servicio específico
docker-compose restart api

# Ejecutar scripts de utilidad
./scripts/docker_scripts.sh start    # Iniciar servicios
./scripts/docker_scripts.sh stop     # Detener servicios
./scripts/docker_scripts.sh logs api # Ver logs
./scripts/docker_scripts.sh health   # Check de salud
```

---

## 📖 Documentación Adicional

Para información más detallada, consulta:

- **[Arquitectura del Sistema](./docs/architecture.md)** - Cómo funciona internamente el servicio
- **[Referencia de API](./docs/api.md)** - Contratos de endpoints y ejemplos
- **[Guía de Configuración](./docs/setup.md)** - Variables de entorno, Docker y scripts

---

## 📝 Licencia

[Especificar licencia del proyecto]

---

## 🤝 Contribuciones

[Especificar pautas de contribución]

