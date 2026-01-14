#!/bin/bash
# ============================================
# DOCKER UTILITY SCRIPTS
# ============================================
# Collection of useful scripts for managing
# the Complementary Services application
# ============================================

# ------------------------------
# scripts/build.sh
# Build Docker images
# ------------------------------
build() {
    echo "🔨 Building Docker images..."
    docker-compose build --no-cache
    echo "✅ Build completed!"
}

# ------------------------------
# scripts/start.sh
# Start all services
# ------------------------------
start() {
    echo "🚀 Starting all services..."
    docker-compose up -d
    echo "⏳ Waiting for services to be healthy..."
    sleep 10
    docker-compose ps
    echo "✅ Services started!"
    echo ""
    echo "📍 Access points:"
    echo "   - API: http://localhost:5000"
    echo "   - Swagger: http://localhost:5000/swagger"
    echo "   - RabbitMQ Management: http://localhost:15672 (guest/guest)"
    echo "   - Keycloak: http://localhost:8080"
}

# ------------------------------
# scripts/stop.sh
# Stop all services
# ------------------------------
stop() {
    echo "🛑 Stopping all services..."
    docker-compose down
    echo "✅ Services stopped!"
}

# ------------------------------
# scripts/restart.sh
# Restart specific service
# ------------------------------
restart() {
    SERVICE=$1
    if [ -z "$SERVICE" ]; then
        echo "❌ Usage: ./restart.sh <service-name>"
        echo "   Services: api, postgres, rabbitmq, keycloak, mongodb"
        exit 1
    fi
    
    echo "🔄 Restarting $SERVICE..."
    docker-compose restart $SERVICE
    echo "✅ $SERVICE restarted!"
}

# ------------------------------
# scripts/logs.sh
# View logs of services
# ------------------------------
logs() {
    SERVICE=${1:-}
    
    if [ -z "$SERVICE" ]; then
        echo "📋 Showing logs for all services..."
        docker-compose logs -f
    else
        echo "📋 Showing logs for $SERVICE..."
        docker-compose logs -f $SERVICE
    fi
}

# ------------------------------
# scripts/clean.sh
# Clean up containers, volumes, and networks
# ------------------------------
clean() {
    echo "⚠️  WARNING: This will remove all containers, volumes, and data!"
    read -p "Are you sure? (yes/no): " -r
    echo
    
    if [[ $REPLY =~ ^[Yy][Ee][Ss]$ ]]; then
        echo "🧹 Cleaning up..."
        docker-compose down -v
        docker system prune -f
        echo "✅ Cleanup completed!"
    else
        echo "❌ Cleanup cancelled"
    fi
}

# ------------------------------
# scripts/migrate.sh
# Run database migrations
# ------------------------------
migrate() {
    echo "🔄 Running database migrations..."
    docker-compose exec api dotnet ef database update
    echo "✅ Migrations completed!"
}

# ------------------------------
# scripts/seed.sh
# Seed initial data
# ------------------------------
seed() {
    echo "🌱 Seeding initial data..."
    docker-compose exec api dotnet run --project complementary-service.Api seed
    echo "✅ Seeding completed!"
}

# ------------------------------
# scripts/backup.sh
# Backup PostgreSQL database
# ------------------------------
backup() {
    BACKUP_DIR="./backups"
    BACKUP_FILE="$BACKUP_DIR/backup_$(date +%Y%m%d_%H%M%S).sql"
    
    mkdir -p $BACKUP_DIR
    
    echo "💾 Creating database backup..."
    docker-compose exec -T postgres pg_dump -U postgres ComplementaryServicesDB > $BACKUP_FILE
    
    if [ $? -eq 0 ]; then
        echo "✅ Backup created: $BACKUP_FILE"
        
        # Compress backup
        gzip $BACKUP_FILE
        echo "✅ Backup compressed: $BACKUP_FILE.gz"
    else
        echo "❌ Backup failed!"
        exit 1
    fi
}

# ------------------------------
# scripts/restore.sh
# Restore PostgreSQL database
# ------------------------------
restore() {
    BACKUP_FILE=$1
    
    if [ -z "$BACKUP_FILE" ]; then
        echo "❌ Usage: ./restore.sh <backup-file.sql.gz>"
        exit 1
    fi
    
    if [ ! -f "$BACKUP_FILE" ]; then
        echo "❌ Backup file not found: $BACKUP_FILE"
        exit 1
    fi
    
    echo "⚠️  WARNING: This will overwrite the current database!"
    read -p "Are you sure? (yes/no): " -r
    echo
    
    if [[ $REPLY =~ ^[Yy][Ee][Ss]$ ]]; then
        echo "🔄 Restoring database..."
        
        # Decompress if needed
        if [[ $BACKUP_FILE == *.gz ]]; then
            gunzip -c $BACKUP_FILE | docker-compose exec -T postgres psql -U postgres ComplementaryServicesDB
        else
            docker-compose exec -T postgres psql -U postgres ComplementaryServicesDB < $BACKUP_FILE
        fi
        
        echo "✅ Database restored!"
    else
        echo "❌ Restore cancelled"
    fi
}

# ------------------------------
# scripts/health.sh
# Check health of all services
# ------------------------------
health() {
    echo "🏥 Checking service health..."
    echo ""
    
    # API Health
    echo "API:"
    curl -s http://localhost:5000/health | jq . || echo "❌ API not responding"
    echo ""
    
    # PostgreSQL
    echo "PostgreSQL:"
    docker-compose exec postgres pg_isready -U postgres || echo "❌ PostgreSQL not ready"
    echo ""
    
    # RabbitMQ
    echo "RabbitMQ:"
    docker-compose exec rabbitmq rabbitmq-diagnostics ping || echo "❌ RabbitMQ not responding"
    echo ""
    
    # Keycloak
    echo "Keycloak:"
    curl -s http://localhost:8080/health/ready || echo "❌ Keycloak not ready"
    echo ""
    
    # Show container status
    echo "Container Status:"
    docker-compose ps
}

# ------------------------------
# scripts/shell.sh
# Open shell in container
# ------------------------------
shell() {
    SERVICE=${1:-api}
    
    echo "🐚 Opening shell in $SERVICE..."
    docker-compose exec $SERVICE /bin/sh
}

# ------------------------------
# scripts/test.sh
# Run tests in Docker
# ------------------------------
test() {
    echo "🧪 Running tests..."
    docker-compose -f docker-compose.test.yml up --build --abort-on-container-exit
    docker-compose -f docker-compose.test.yml down -v
    echo "✅ Tests completed!"
}

# ------------------------------
# scripts/stats.sh
# Show resource usage statistics
# ------------------------------
stats() {
    echo "📊 Resource usage statistics:"
    docker stats --no-stream $(docker-compose ps -q)
}

# ------------------------------
# scripts/update.sh
# Update all images
# ------------------------------
update() {
    echo "🔄 Updating images..."
    docker-compose pull
    echo "✅ Images updated!"
}

# ------------------------------
# Main script router
# ------------------------------
case "${1:-help}" in
    build)
        build
        ;;
    start)
        start
        ;;
    stop)
        stop
        ;;
    restart)
        restart $2
        ;;
    logs)
        logs $2
        ;;
    clean)
        clean
        ;;
    migrate)
        migrate
        ;;
    seed)
        seed
        ;;
    backup)
        backup
        ;;
    restore)
        restore $2
        ;;
    health)
        health
        ;;
    shell)
        shell $2
        ;;
    test)
        test
        ;;
    stats)
        stats
        ;;
    update)
        update
        ;;
    help|*)
        echo "🐳 Docker Utility Scripts"
        echo ""
        echo "Usage: ./docker.sh <command> [options]"
        echo ""
        echo "Commands:"
        echo "  build              Build Docker images"
        echo "  start              Start all services"
        echo "  stop               Stop all services"
        echo "  restart <service>  Restart specific service"
        echo "  logs [service]     View logs (all or specific service)"
        echo "  clean              Clean up containers and volumes"
        echo "  migrate            Run database migrations"
        echo "  seed               Seed initial data"
        echo "  backup             Backup PostgreSQL database"
        echo "  restore <file>     Restore database from backup"
        echo "  health             Check health of all services"
        echo "  shell [service]    Open shell in container (default: api)"
        echo "  test               Run tests"
        echo "  stats              Show resource usage"
        echo "  update             Update all images"
        echo "  help               Show this help message"
        echo ""
        echo "Examples:"
        echo "  ./docker.sh start"
        echo "  ./docker.sh logs api"
        echo "  ./docker.sh restart rabbitmq"
        echo "  ./docker.sh backup"
        echo "  ./docker.sh restore backups/backup_20240115_120000.sql.gz"
        ;;
esac