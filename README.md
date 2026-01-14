# Plantilla complementary_service Hexagonal (dotnet)

Plantilla para generar un microservicio con arquitectura Hexagonal (Ports & Adapters) en .NET.

Estructura creada:

- src/
  - complementary_service.Domain/
  - complementary_service.Application/
  - complementary_service.Infrastructure/
  - complementary_service.Api/
- tests/
  - complementary_service.Domain.Tests/
  - complementary_service.Application.Tests/
  - complementary_service.Infrastructure.IntegrationTests/
- .template.config/template.json

Cómo usar este repositorio como template

Hay dos formas comunes de usar este repo como plantilla:

1) Usar el repositorio como "GitHub Template" (recomendado si publicas en GitHub):
   - En GitHub configura el repositorio como "Template repository" (Settings → Template repository) o usa el botón "Use this template" para crear un nuevo repo basado en esta plantilla.
   - Clona el repo resultante localmente y sigue la sección "Instalación local" abajo para instalar el template en tu máquina.

2) Instalación local directa (desarrollo / pruebas):
   - Clona este repositorio y luego instala la plantilla desde la carpeta del repo:

```bash
git clone https://github.com/<owner>/complementary_service-hexagonal-template-.git
cd complementary_service-hexagonal-template-
# Instalar la plantilla localmente (SDK moderno):
dotnet new install .
# Si ya la tienes instalada y quieres forzar la actualización:
dotnet new install . --force
```

Ver las plantillas instaladas:

```bash
dotnet new list
```

Generar un nuevo microservicio desde la plantilla

```bash
# Crea el microservicio (reemplaza "Orders" por el nombre que desees):
dotnet new complementary_service-hex -n Orders -o ./Orders --framework net8.0

cd Orders
dotnet restore
dotnet build
```

Notas importantes
- El template usa `complementary_service` como `sourceName`; al generar el proyecto ese token se sustituye por el nombre que pases con `-n`.
- Los .csproj contienen el token `net8.0` que se sustituye por el valor del parámetro `--framework` (por defecto `net8.0`).
- Si la plantilla está instalada globalmente y haces cambios locales, reinstálala con `--force`.

Desinstalar la plantilla (opcional)

```bash
# Si la instalaste desde una carpeta local, puedes desinstalar usando la misma ruta o el identificador usado al instalar.
dotnet new uninstall /path/to/complementary_service-hexagonal-template-
# (o) desinstalar por paquete si lo subiste a un feed: dotnet new uninstall <package-or-feed>
```

Problemas comunes
- Si ves errores al compilar la API relacionados con Swagger, asegúrate de restaurar paquetes; la plantilla incluye `Swashbuckle.AspNetCore` por defecto.
- Si el comando `dotnet new complementary_service-hex` no aparece tras instalar, ejecuta `dotnet new install . --force` y verifica con `dotnet new list`.

¿Qué sigue?
- Puedes solicitar que añada un `.sln` a la plantilla, workflows de CI (GitHub Actions) que verifiquen la generación y build, o parámetros adicionales para incluir EF Core / mensajería a la carta.

