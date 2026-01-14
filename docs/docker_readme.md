# 🐳 Complementary Services - Docker Setup Guide

## 📋 Tabla de Contenidos

1. [Requisitos Previos](#requisitos-previos)
2. [Inicio Rápido](#inicio-rápido)
3. [Estructura del Proyecto](#estructura-del-proyecto)
4. [Servicios Incluidos](#servicios-incluidos)
5. [Configuración](#configuración)
6. [Comandos Útiles](#comandos-útiles)
7. [Desarrollo Local](#desarrollo-local)
8. [Troubleshooting](#troubleshooting)
9. [Producción](#producción)

---

## 🔧 Requisitos Previos

### Software Necesario

- **Docker**: 24.0.0 o superior
- **Docker Compose**: 2.20.0 o superior
- **Git**: Para clonar el repositorio
- **curl/jq**: Para scripts de salud (opcional)

### Instalación de Docker

#### Windows/Mac
```bash
# Descargar Docker Desktop desde:
https://www.docker.com/products/docker-desktop
```

#### Linux (Ubuntu/Debian)
```bash
# Instalar Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Instalar Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# Agregar usuario al grupo docker
sudo usermod -aG docker $USER
```

### Verificar Instalación

```bash
docker --version
# Output: Docker version 24.0.0, build...

docker-compose --version
# Output: Docker Compose version v2.20.0
```

---

## 🚀 Inicio Rápido

### 1. Clonar el Repositorio

```bash
git clone https://github.com/your-org/complementary-services.git
cd complementary-services
```

### 2. Configurar Variables de Entorno

```bash
# Copiar el archivo de ejemplo
cp .env.example .env

# Editar valores según necesidad
nano .env  # o usar tu editor favorito
```

### 3. Hacer Scripts Ejecutables (Linux/Mac)

```bash
chmod +x scripts/*.sh
```

### 4. Iniciar Todos los Servicios

```bash
# Usando docker-compose directamente
docker-compose up -d

# O usando el script de utilidad
./scripts/start.sh
```

### 5. Verificar que Todo Esté Funcionando

```bash
# Verificar estado de contenedores
docker-compose ps

# Verificar health checks
./scripts/health.sh

# Ver logs
docker-compose logs -f api
```

### 6. Acceder a la Aplicación

Una vez que todos los servicios estén **healthy**:

| Servicio | URL | Credenciales |
|----------|-----|--------------|
| API REST | http://localhost:5000 | - |
| Swagger UI | http://localhost:5000/swagger | - |
| RabbitMQ Management | http://localhost:15672 | guest / guest |
| Keycloak Admin | http://localhost:8080 | admin / admin |
| Health Check | http://localhost:5000/health | - |

### 7. Ejecutar Migraciones

```bash
# Aplicar migraciones de base de datos
./scripts/migrate.sh

# Seed de datos iniciales (opcional)
./scripts/seed.sh
```

---

## 📁 Estructura del Proyecto

```
complementary-services/
├── src/                                  # Código fuente
│   ├── ComplementaryServices.API/
│   ├── ComplementaryServices.Application/
│   ├── ComplementaryServices.Domain/
│   └── ComplementaryServices.Infrastructure/
├── tests/                                # Pruebas
│   └── ComplementaryServices.Tests/
├── docker/                               # Configuración Docker
│   ├── postgres/
│   │   └── init.sql
│   ├── rabbitmq/
│   │   ├── definitions.json
│   │   └── rabbitmq.conf
│   ├── keycloak/
│   │   └── realm-export.json
│   └── mock-provider/
│       └── Dockerfile
├── scripts/                              # Scripts de utilidad
│   ├── build.sh
│   ├── start.sh
│   ├── stop.sh
│   ├── migrate.sh
│   ├── backup.sh
│   └── ...
├── logs/                                 # Logs de aplicación
├── backups/                              # Backups de DB
├── Dockerfile                            # Dockerfile multi-stage
├── docker-compose.yml                    # Configuración principal
├── docker-compose.override.yml           # Desarrollo local
├── docker-compose.prod.yml               # Producción
├── docker-compose.test.yml               # Testing
├── .env.example                          # Ejemplo de variables
├── .dockerignore                         # Exclusiones Docker
└── README.md                             # Este archivo
```

---

## 🛠️ Servicios Incluidos

### 1. PostgreSQL
- **Imagen**: `postgres:16-alpine`
- **Puerto**: 5432
- **Base de datos**: ComplementaryServicesDB
- **Usuario**: postgres (configurable en .env)

### 2. RabbitMQ
- **Imagen**: `rabbitmq:3.12-management-alpine`
- **Puertos**: 
  - 5672 (AMQP)
  - 15672 (Management UI)
- **Exchanges**: services.requests, services.responses, services.dlx
- **Queues**: transport.requests, catering.requests, etc.

### 3. Keycloak
- **Imagen**: `quay.io/keycloak/keycloak:22.0`
- **Puerto**: 8080
- **Realm**: eventplatform
- **Cliente**: complementary-services

### 4. MongoDB (Logs - Opcional)
- **Imagen**: `mongo:7-jammy`
- **Puerto**: 27017
- **Base de datos**: complementary-services-logs

### 5. API REST
- **Imagen**: Construida desde Dockerfile
- **Puerto**: 5000 (mapea a 8080 interno)
- **Framework**: .NET 8
- **Arquitectura**: Hexagonal + DDD

### 6. Mock External Provider
- **Imagen**: Custom (docker/mock-provider)
- **Función**: Simular proveedores externos para testing

---

## ⚙️ Configuración

### Variables de Entorno Principales

Editar `.env` con los siguientes valores:

```bash
# Ambiente
ASPNETCORE_ENVIRONMENT=Development

# PostgreSQL
POSTGRES_USER=postgres
POSTGRES_PASSWORD=YourSecurePassword123!
POSTGRES_DB=ComplementaryServicesDB

# RabbitMQ
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest

# Keycloak
KEYCLOAK_ADMIN=admin
KEYCLOAK_ADMIN_PASSWORD=AdminPassword123!
KEYCLOAK_REALM=eventplatform
KEYCLOAK_CLIENT_ID=complementary-services

# CORS (Frontend URLs)
CORS_ORIGIN_1=http://localhost:3000
CORS_ORIGIN_2=http://localhost:5173
```

### Configuración de Recursos

Editar `docker-compose.yml` para limitar recursos:

```yaml
api:
  deploy:
    resources:
      limits:
        cpus: '2.0'
        memory: 2G
      reservations:
        cpus: '1.0'
        memory: 1G
```

---

## 📝 Comandos Útiles

### Gestión de Servicios

```bash
# Iniciar todos los servicios
docker-compose up -d

# Detener todos los servicios
docker-compose down

# Reiniciar un servicio específico
docker-compose restart api

# Ver logs en tiempo real
docker-compose logs -f

# Ver logs de un servicio específico
docker-compose logs -f api

# Ver estado de servicios
docker-compose ps
```

### Build y Actualización

```bash
# Construir imágenes desde cero
docker-compose build --no-cache

# Actualizar imágenes
docker-compose pull

# Reconstruir solo el API
docker-compose build api
```

### Base de Datos

```bash
# Ejecutar migraciones
docker-compose exec api dotnet ef database update

# Crear nueva migración
docker-compose exec api dotnet ef migrations add MigrationName

# Backup de base de datos
docker-compose exec postgres pg_dump -U postgres ComplementaryServicesDB > backup.sql

# Restaurar base de datos
docker-compose exec -T postgres psql -U postgres ComplementaryServicesDB < backup.sql
```

### Debugging

```bash
# Abrir shell en contenedor API
docker-compose exec api /bin/sh

# Abrir shell en PostgreSQL
docker-compose exec postgres psql -U postgres ComplementaryServicesDB

# Ver estadísticas de recursos
docker stats

# Inspeccionar contenedor
docker inspect complementary-services-api

# Ver logs de un contenedor específico
docker logs complementary-services-api -f
```

### Limpieza

```bash
# Detener y eliminar contenedores
docker-compose down

# Detener y eliminar contenedores + volúmenes
docker-compose down -v

# Limpiar todo Docker (¡CUIDADO!)
docker system prune -a --volumes
```

---

## 💻 Desarrollo Local

### Workflow de Desarrollo

1. **Hacer cambios en el código**
   ```bash
   # Editar archivos en src/
   ```

2. **Reconstruir imagen de API**
   ```bash
   docker-compose build api
   ```

3. **Reiniciar servicio**
   ```bash
   docker-compose restart api
   ```

4. **Ver logs para verificar**
   ```bash
   docker-compose logs -f api
   ```

### Hot Reload (Desarrollo)

Para desarrollo con hot reload, puedes montar el código como volumen:

```yaml
# docker-compose.override.yml
services:
  api:
    volumes:
      - ./src:/app/src
    environment:
      ASPNETCORE_ENVIRONMENT: Development
```

### Ejecutar Pruebas

```bash
# Ejecutar pruebas unitarias
docker-compose exec api dotnet test

# Ejecutar pruebas con docker-compose.test.yml
docker-compose -f docker-compose.test.yml up --abort-on-container-exit
```

### Debugging con VS Code

Crear `.vscode/launch.json`:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Docker: Attach to API",
      "type": "coreclr",
      "request": "attach",
      "processId": "${command:pickRemoteProcess}",
      "pipeTransport": {
        "pipeProgram": "docker",
        "pipeArgs": ["exec", "-i", "complementary-services-api"],
        "debuggerPath": "/remote_debugger/vsdbg"
      }
    }
  ]
}
```

---

## 🔍 Troubleshooting

### Problema: Contenedores no inician

**Síntomas**: `docker-compose up` falla o contenedores se quedan en "starting"

**Soluciones**:
```bash
# Ver logs detallados
docker-compose logs

# Verificar puertos no estén en uso
sudo lsof -i :5432  # PostgreSQL
sudo lsof -i :5672  # RabbitMQ
sudo lsof -i :8080  # Keycloak

# Limpiar y reiniciar
docker-compose down -v
docker-compose up -d
```

### Problema: Health checks fallan

**Síntomas**: Contenedores en estado "unhealthy"

**Soluciones**:
```bash
# Verificar health check de cada servicio
docker inspect complementary-services-api | grep -A 20 Health

# Ver logs del servicio
docker-compose logs api

# Aumentar start_period en docker-compose.yml
healthcheck:
  start_period: 120s  # Dar más tiempo
```

### Problema: Conexión rechazada entre contenedores

**Síntomas**: API no puede conectarse a PostgreSQL/RabbitMQ

**Soluciones**:
```bash
# Verificar que estén en la misma red
docker network inspect eventplatform-network

# Usar nombres de servicio, NO localhost
# ✅ Correcto: Host=postgres;Port=5432
# ❌ Incorrecto: Host=localhost;Port=5432
```

### Problema: Migraciones fallan

**Síntomas**: Error al ejecutar `dotnet ef database update`

**Soluciones**:
```bash
# Verificar PostgreSQL esté corriendo
docker-compose ps postgres

# Verificar connection string
docker-compose exec api env | grep ConnectionStrings

# Resetear base de datos
docker-compose exec postgres psql -U postgres -c "DROP DATABASE ComplementaryServicesDB;"
docker-compose exec postgres psql -U postgres -c "CREATE DATABASE ComplementaryServicesDB;"
docker-compose exec api dotnet ef database update
```

### Problema: Volúmenes con permisos incorrectos

**Síntomas**: Errores de permisos al escribir logs/archivos

**Soluciones**:
```bash
# En Linux, dar permisos
sudo chown -R $USER:$USER ./logs
sudo chmod -R 755 ./logs

# O usar usuario específico en Dockerfile
USER 1000:1000
```

---

## 🚀 Producción

### Preparación para Producción

1. **Usar docker-compose.prod.yml**
   ```bash
   docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
   ```

2. **Variables de entorno de producción**
   ```bash
   cp .env.example .env.production
   # Editar con valores reales
   ASPNETCORE_ENVIRONMENT=Production
   POSTGRES_PASSWORD=StrongProductionPassword!
   ```

3. **Configurar HTTPS con reverse proxy (Nginx)**
   ```yaml
   # docker-compose.prod.yml
   nginx:
     image: nginx:alpine
     ports:
       - "80:80"
       - "443:443"
     volumes:
       - ./nginx/nginx.conf:/etc/nginx/nginx.conf
       - ./nginx/ssl:/etc/nginx/ssl
   ```

### Monitoreo

```bash
# Configurar Prometheus + Grafana (opcional)
# Ver docker-compose.monitoring.yml
```

### Backup Automático

```bash
# Agregar a crontab
0 2 * * * /path/to/scripts/backup.sh
```

### Deploy Continuo

Ver `.github/workflows/deploy-prod.yml` para CI/CD automático.

---

## 📚 Recursos Adicionales

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Reference](https://docs.docker.com/compose/compose-file/)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [PostgreSQL Docker](https://hub.docker.com/_/postgres)
- [RabbitMQ Docker](https://hub.docker.com/_/rabbitmq)

---

## 🤝 Contribuir

Para contribuir al proyecto:

1. Fork el repositorio
2. Crear branch: `git checkout -b feature/nueva-funcionalidad`
3. Commit cambios: `git commit -am 'Agregar nueva funcionalidad'`
4. Push: `git push origin feature/nueva-funcionalidad`
5. Crear Pull Request

---

## 📄 Licencia

Este proyecto es parte del curso de Desarrollo del Software - Universidad Católica Andrés Bello.

---

**¿Dudas?** Contactar al equipo de desarrollo o abrir un issue en GitHub.