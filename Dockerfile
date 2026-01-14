# Stage 1: Restore dependencies
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src

# Copy only csproj files for better layer caching
COPY ["src/complementary-service.Api/complementary-service.Api.csproj", "complementary-service.Api/"]
COPY ["src/complementary-service.Application/complementary-service.Application.csproj", "complementary-service.Application/"]
COPY ["src/complementary-service.Domain/complementary-service.Domain.csproj", "complementary-service.Domain/"]
COPY ["src/complementary-service.Infrastructure/complementary-service.Infrastructure.csproj", "complementary-service.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "complementary-service.Api/complementary-service.Api.csproj"

# Stage 2: Build application
FROM restore AS build
WORKDIR /src

# Copy all source code
COPY src/ .

# Build the application
WORKDIR "/src/complementary-service.Api"
RUN dotnet build "complementary-service.Api.csproj" -c Release -o /app/build --no-restore

# Stage 3: Publish
FROM build AS publish
RUN dotnet publish "complementary-service.Api.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

# Stage 4: Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published application
COPY --from=publish /app/publish .

# Change ownership to non-root user
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Environment variables (can be overridden by docker-compose)
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=60s --retries=3 \
  CMD curl --fail http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "complementary_service.Api.dll"]