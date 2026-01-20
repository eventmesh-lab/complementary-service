# Setup & Configuration Guide

## 📋 Prerequisitos

### Software Requerido

| Software | Versión Mínima | Propósito |
|----------|----------------|-----------|
| Docker | 20.10+ | Orquestación de contenedores |
| Docker Compose | 2.0+ | Multi-container orchestration |
| .NET SDK | 8.0 | Desarrollo local sin Docker |
| Git | 2.30+ | Control de versiones |

### Opcional (para desarrollo)
- Visual Studio 2022 / VS Code
- Azure Data Studio / pgAdmin (gestión de PostgreSQL)
- Postman / Insomnia (testing de API)

---

## 🚀 Instalación

### 1. Clonar el Repositorio

```bash
git clone https://github.com/eventmesh-lab/complementary-service.git
cd complementary-service
```

### 2. Configurar Variables de Entorno

```bash
# Copiar el archivo de ejemplo
cp .env.example .env

# Editar con tu editor favorito
nano .env  # o vim, code, etc.
```

### 3. Iniciar Servicios con Docker

```bash
# Construir imágenes
docker-compose build

# Iniciar todos los servicios
docker-compose up -d

# Verificar estado
docker-compose ps
```

### 4. Verificar Instalación

```bash
# Check de salud de la API
curl http://localhost:5050/health

# Abrir Swagger UI
open http://localhost:5050/swagger  # macOS
xdg-open http://localhost:5050/swagger  # Linux
start http://localhost:5050/swagger  # Windows
```

---

## ⚙️ Variables de Entorno

### Configuración Completa

| Variable | Descripción | Valor por Defecto | Requerido |
|----------|-------------|-------------------|-----------|
| **Application** |
| `ASPNETCORE_ENVIRONMENT` | Entorno de ejecución (`Development`, `Production`) | `Development` | ✅ |
| `ASPNETCORE_URLS` | URLs donde escucha la API | `http://+:8080` | ✅ |
| **PostgreSQL** |
| `POSTGRES_USER` | Usuario de PostgreSQL | `postgres` | ✅ |
| `POSTGRES_PASSWORD` | Contraseña de PostgreSQL | `postgres` | ✅ |
| `POSTGRES_DB` | Nombre de la base de datos | `ComplementaryServicesDB` | ✅ |
| `ConnectionStrings__DefaultConnection` | Connection string completo | Ver `.env.example` | ✅ |
| **RabbitMQ** |
| `RABBITMQ_USER` | Usuario de RabbitMQ | `guest` | ✅ |
| `RABBITMQ_PASSWORD` | Contraseña de RabbitMQ | `guest` | ✅ |
| `RABBITMQ_VHOST` | Virtual host de RabbitMQ | `/` | ✅ |
| `RabbitMQ__HostName` | Hostname del broker | `localhost` (dev) / `rabbitmq` (docker) | ✅ |
| `RabbitMQ__Port` | Puerto AMQP | `5672` | ✅ |
| **Keycloak (Autenticación)** |
| `KEYCLOAK_ADMIN` | Usuario admin de Keycloak | `admin` | ❌ |
| `KEYCLOAK_ADMIN_PASSWORD` | Contraseña admin | `AdminPassword123!` | ❌ |
| `KEYCLOAK_REALM` | Realm del proyecto | `eventplatform` | ❌ |
| `KEYCLOAK_CLIENT_ID` | Client ID del servicio | `complementary-services` | ❌ |
| **MongoDB (Logs)** |
| `MONGO_USER` | Usuario de MongoDB | `admin` | ❌ |
| `MONGO_PASSWORD` | Contraseña de MongoDB | `admin` | ❌ |
| `MongoDB__ConnectionString` | Connection string | Ver `.env.example` | ❌ |
| `MongoDB__DatabaseName` | Nombre de la base de datos | `complementary-services-logs` | ❌ |
| **CORS** |
| `CORS_ORIGIN_1` | Origen permitido 1 | `http://localhost:3000` | ✅ |
| `CORS_ORIGIN_2` | Origen permitido 2 | `http://localhost:5173` | ✅ |
| `AllowedOrigins__0` | Array de orígenes (alternativo) | Ver `appsettings.json` | ✅ |

### Ejemplo de .env para Desarrollo

```bash
# ============================================
# DEVELOPMENT ENVIRONMENT
# ============================================

ASPNETCORE_ENVIRONMENT=Development

# PostgreSQL
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_DB=ComplementaryServicesDB

# RabbitMQ
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest
RABBITMQ_VHOST=/

# CORS (Frontend)
CORS_ORIGIN_1=http://localhost:3000
CORS_ORIGIN_2=http://localhost:5173
```

### Ejemplo de .env para Producción

```bash
# ============================================
# PRODUCTION ENVIRONMENT
# ============================================

ASPNETCORE_ENVIRONMENT=Production

# PostgreSQL (usar valores seguros)
POSTGRES_USER=complementary_svc_user
POSTGRES_PASSWORD=<STRONG_PASSWORD_HERE>
POSTGRES_DB=ComplementaryServicesDB

# RabbitMQ (usar valores seguros)
RABBITMQ_USER=complementary_svc_user
RABBITMQ_PASSWORD=<STRONG_PASSWORD_HERE>
RABBITMQ_VHOST=/eventplatform

# MongoDB
MONGO_USER=complementary_svc_user
MONGO_PASSWORD=<STRONG_PASSWORD_HERE>

# CORS (ajustar a dominio real)
CORS_ORIGIN_1=https://app.example.com
CORS_ORIGIN_2=https://dashboard.example.com

# Keycloak
KEYCLOAK_URL=https://auth.example.com
KEYCLOAK_REALM=eventplatform
KEYCLOAK_CLIENT_ID=complementary-services
```

---

## 🐳 Docker

### Dockerfile

El proyecto utiliza un **multi-stage build** optimizado para producción:

**Etapas**:

1. **Restore**: Restaura dependencias NuGet (layer caching)
2. **Build**: Compila el proyecto en modo Release
3. **Publish**: Publica los artefactos
4. **Final**: Imagen runtime mínima con ASP.NET Core Runtime

**Características de Seguridad**:
- Usuario no-root (`appuser`)
- Health check integrado
- Imagen base optimizada (`aspnet:8.0`)

### Construcción Manual de Imagen

```bash
# Construir imagen
docker build -t complementary-services-api:latest .

# Ejecutar contenedor
docker run -d \
  -p 5050:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ConnectionStrings__DefaultConnection="Host=postgres;..." \
  --name complementary-api \
  complementary-services-api:latest

# Ver logs
docker logs -f complementary-api
```

### docker-compose.yml

El archivo `docker-compose.yml` define 5 servicios:

| Servicio | Imagen | Puerto Expuesto | Descripción |
|----------|--------|-----------------|-------------|
| `postgres` | `postgres:16-alpine` | `5436:5432` | Base de datos principal |
| `rabbitmq` | `rabbitmq:3.12-management-alpine` | `5675:5672`, `15675:15672` | Message broker |
| `mongodb` | `mongo:7-jammy` | `27018:27017` | Logs (opcional) |
| `api` | Build local | `5050:8080` | API del microservicio |
| `mock-provider` | Build local | N/A | Proveedor mock para testing |

### Comandos Docker Compose

```bash
# Iniciar servicios en foreground (ver logs)
docker-compose up

# Iniciar en background
docker-compose up -d

# Ver logs de un servicio específico
docker-compose logs -f api

# Reiniciar un servicio
docker-compose restart api

# Detener todos los servicios
docker-compose down

# Detener y eliminar volúmenes (⚠️ DATOS SE PIERDEN)
docker-compose down -v

# Reconstruir imágenes
docker-compose build --no-cache
```

### Volúmenes Persistentes

Los siguientes volúmenes persisten datos:

```yaml
volumes:
  postgres_data:       # Datos de PostgreSQL
  rabbitmq_data:       # Datos de RabbitMQ
  mongodb_data:        # Logs de MongoDB
```

**Ubicación**: Docker gestiona estos volúmenes automáticamente.

**Backup Manual**:
```bash
# Exportar datos de PostgreSQL
docker-compose exec -T postgres pg_dump -U postgres ComplementaryServicesDB > backup.sql

# Importar datos
docker-compose exec -T postgres psql -U postgres ComplementaryServicesDB < backup.sql
```

---

## 📜 Scripts

### docker_scripts.sh

El archivo `scripts/docker_scripts.sh` proporciona utilidades para gestionar el proyecto.

#### Comandos Disponibles

| Comando | Descripción | Ejemplo |
|---------|-------------|---------|
| `build` | Construye todas las imágenes Docker | `./scripts/docker_scripts.sh build` |
| `start` | Inicia todos los servicios | `./scripts/docker_scripts.sh start` |
| `stop` | Detiene todos los servicios | `./scripts/docker_scripts.sh stop` |
| `restart <service>` | Reinicia un servicio específico | `./scripts/docker_scripts.sh restart api` |
| `logs [service]` | Muestra logs (todos o uno específico) | `./scripts/docker_scripts.sh logs api` |
| `clean` | Limpia contenedores y volúmenes (⚠️) | `./scripts/docker_scripts.sh clean` |
| `migrate` | Ejecuta migraciones de base de datos | `./scripts/docker_scripts.sh migrate` |
| `seed` | Carga datos iniciales | `./scripts/docker_scripts.sh seed` |
| `backup` | Backup de PostgreSQL | `./scripts/docker_scripts.sh backup` |
| `restore <file>` | Restaura backup | `./scripts/docker_scripts.sh restore backup.sql.gz` |
| `health` | Verifica salud de servicios | `./scripts/docker_scripts.sh health` |
| `shell [service]` | Abre shell en contenedor | `./scripts/docker_scripts.sh shell api` |
| `test` | Ejecuta tests en Docker | `./scripts/docker_scripts.sh test` |
| `stats` | Muestra uso de recursos | `./scripts/docker_scripts.sh stats` |
| `update` | Actualiza imágenes base | `./scripts/docker_scripts.sh update` |

#### Ejemplos de Uso

```bash
# Hacer script ejecutable (primera vez)
chmod +x scripts/docker_scripts.sh

# Iniciar todo
./scripts/docker_scripts.sh start

# Ver logs de la API en tiempo real
./scripts/docker_scripts.sh logs api

# Backup antes de despliegue
./scripts/docker_scripts.sh backup

# Abrir shell para debugging
./scripts/docker_scripts.sh shell api

# Ver consumo de recursos
./scripts/docker_scripts.sh stats
```

---

## 🛠️ Desarrollo Local (sin Docker)

### 1. Instalar Dependencias

```bash
# Restaurar paquetes NuGet
dotnet restore

# Verificar instalación
dotnet --version  # Debe ser 8.0+
```

### 2. Configurar appsettings.Development.json

Crear `src/complementary-service.Api/appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "ComplementaryServices": "Trace"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5436;Database=ComplementaryServicesDB;Username=postgres;Password=postgres"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5675
  }
}
```

### 3. Iniciar Infraestructura (solo bases de datos)

```bash
# Iniciar solo PostgreSQL y RabbitMQ
docker-compose up -d postgres rabbitmq
```

### 4. Ejecutar Migraciones

```bash
cd src/complementary-service.Api

# Aplicar migraciones
dotnet ef database update
```

### 5. Ejecutar la API

```bash
cd src/complementary-service.Api

# Ejecutar en modo watch (hot reload)
dotnet watch run

# O ejecutar normal
dotnet run
```

**API disponible en**: http://localhost:5000 (o el puerto configurado)

### 6. Ejecutar Tests

```bash
# Todos los tests
dotnet test

# Tests de un proyecto específico
dotnet test tests/complementary-service.Domain.Tests/

# Con cobertura
dotnet test /p:CollectCoverage=true /p:CoverageReportsDirectory=./coverage
```

---

## 🗄️ Migraciones de Base de Datos

### Comandos Entity Framework Core

```bash
cd src/complementary-service.Api

# Ver migraciones aplicadas
dotnet ef migrations list

# Crear nueva migración
dotnet ef migrations add <MigrationName>

# Aplicar migraciones
dotnet ef database update

# Rollback a migración específica
dotnet ef database update <MigrationName>

# Generar script SQL
dotnet ef migrations script -o migration.sql
```

### Aplicar Migraciones en Docker

```bash
# Opción 1: Script de utilidad
./scripts/docker_scripts.sh migrate

# Opción 2: Comando directo
docker-compose exec api dotnet ef database update
```

---

## 🔒 Seguridad

### Configuración de Keycloak (Autenticación)

**Nota**: Actualmente el servicio está configurado para JWT de Keycloak pero no valida tokens (modo desarrollo).

#### Habilitar Validación de JWT (Producción)

Editar `src/complementary-service.Api/Program.cs`:

```csharp
// Descomentar y configurar
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.Audience = builder.Configuration["Keycloak:Audience"];
        options.RequireHttpsMetadata = false; // Solo para desarrollo
    });
```

Agregar a `appsettings.json`:

```json
{
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/eventplatform",
    "Audience": "complementary-services"
  }
}
```

### Recomendaciones de Seguridad

1. **Producción**:
   - Usar contraseñas fuertes (no usar valores por defecto)
   - Habilitar HTTPS (TLS/SSL)
   - Validar JWT obligatoriamente
   - Eliminar header `X-User-Id` en controller

2. **Secrets**:
   - NO commitear `.env` (incluido en `.gitignore`)
   - Usar Azure Key Vault / AWS Secrets Manager en cloud
   - Rotar credenciales periódicamente

3. **Base de Datos**:
   - Crear usuarios específicos con permisos mínimos
   - Habilitar SSL en conexión PostgreSQL
   - Backups automáticos cifrados

4. **RabbitMQ**:
   - Cambiar usuario/contraseña por defecto
   - Deshabilitar guest remoto
   - TLS en producción

---

## 🐛 Troubleshooting

### Problema: "Cannot connect to PostgreSQL"

**Síntomas**: Error al iniciar API

```
Npgsql.NpgsqlException: Connection refused
```

**Soluciones**:
1. Verificar que PostgreSQL esté corriendo:
   ```bash
   docker-compose ps postgres
   ```

2. Verificar connection string en `.env`

3. Probar conexión manual:
   ```bash
   docker-compose exec postgres psql -U postgres -d ComplementaryServicesDB
   ```

---

### Problema: "RabbitMQ connection failed"

**Síntomas**: API no publica mensajes

**Soluciones**:
1. Verificar que RabbitMQ esté corriendo:
   ```bash
   docker-compose ps rabbitmq
   ```

2. Verificar configuración en `appsettings.json`

3. Acceder a RabbitMQ Management UI:
   - URL: http://localhost:15675
   - User: guest / guest

4. Verificar que existan exchanges y queues:
   - Exchange: `services.requests`
   - Queues: `transport.requests`, `catering.requests`, etc.

---

### Problema: "Port already in use"

**Síntomas**:
```
Error starting userland proxy: listen tcp4 0.0.0.0:5050: bind: address already in use
```

**Soluciones**:
1. Cambiar puerto en `docker-compose.yml`:
   ```yaml
   ports:
     - "5051:8080"  # Cambiado de 5050 a 5051
   ```

2. O detener servicio que usa el puerto:
   ```bash
   # Linux/macOS
   lsof -i :5050
   kill -9 <PID>
   
   # Windows
   netstat -ano | findstr :5050
   taskkill /PID <PID> /F
   ```

---

### Problema: Migraciones no se aplican

**Síntomas**: Tablas no existen en base de datos

**Soluciones**:
1. Aplicar migraciones manualmente:
   ```bash
   ./scripts/docker_scripts.sh migrate
   ```

2. Verificar que el assembly de migraciones sea correcto en `Program.cs`:
   ```csharp
   npgsqlOptions => npgsqlOptions.MigrationsAssembly("complementary_service.Infrastructure")
   ```

3. Recrear base de datos (⚠️ PIERDE DATOS):
   ```bash
   docker-compose down -v
   docker-compose up -d postgres
   ./scripts/docker_scripts.sh migrate
   ```

---

## 📚 Referencias

- [.NET Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)
- [SignalR Documentation](https://learn.microsoft.com/en-us/aspnet/core/signalr/)
- [Docker Documentation](https://docs.docker.com/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)

---

## 🤝 Soporte

Para problemas no cubiertos en esta guía:
1. Revisar logs: `./scripts/docker_scripts.sh logs api`
2. Verificar health: `./scripts/docker_scripts.sh health`
3. Abrir issue en GitHub
4. Contactar al equipo de desarrollo
