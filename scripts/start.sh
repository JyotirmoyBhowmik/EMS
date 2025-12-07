#!/bin/bash
# SCADA System Startup Script
# This script starts all SCADA services in the correct order

set -e

echo "🚀 Starting Enterprise SCADA System..."

# Check prerequisites
echo "Checking prerequisites..."
command -v docker >/dev/null 2>&1 || { echo "❌ Docker is required but not installed. Aborting." >&2; exit 1; }
command -v docker-compose >/dev/null 2>&1 || { echo "❌ Docker Compose is required but not installed. Aborting." >&2; exit 1; }

# Check if .env file exists
if [ ! -f .env ]; then
    echo "⚠️  .env file not found. Creating from template..."
    cp .env.example .env
    echo "✅ Created .env file. Please update with your configuration."
    echo "   Edit .env and run this script again."
    exit 1
fi

# Start infrastructure services first
echo "📦 Starting infrastructure services..."
docker-compose up -d influxdb postgres rabbitmq redis node-red

# Wait for databases to be ready
echo "⏳ Waiting for databases to initialize..."
sleep 15

# Check database health
echo "🏥 Checking database health..."
until docker exec scada-postgres pg_isready -U scada; do
    echo "   PostgreSQL is unavailable - sleeping"
    sleep 2
done
echo "✅ PostgreSQL is ready"

until docker exec scada-influxdb influx ping; do
    echo "   InfluxDB is unavailable - sleeping"
    sleep 2
done
echo "✅ InfluxDB is ready"

# Start backend services
echo "🔧 Starting backend services..."
docker-compose up -d scada-core data-acquisition alarm-management auth-service reporting-service api-gateway

# Wait for backend services
echo "⏳ Waiting for backend services..."
sleep 10

# Check backend health
echo "🏥 Checking backend service health..."
for service in scada-core:5001 data-acquisition:5002 alarm-management:5003 auth-service:5004 api-gateway:5000; do
    IFS=':' read -r name port <<< "$service"
    until curl -f http://localhost:$port/health >/dev/null 2>&1; do
        echo "   $name is unavailable - sleeping"
        sleep 2
    done
    echo "✅ $name is ready"
done

# Start frontend
echo "🎨 Starting frontend..."
docker-compose up -d frontend

# Start monitoring
echo "📊 Starting monitoring services..."
docker-compose up -d prometheus grafana

echo ""
echo "✅ ✅ ✅ SCADA System Started Successfully! ✅ ✅ ✅"
echo ""
echo "Access Points:"
echo "  🖥️  Frontend Dashboard:    http://localhost:3000"
echo "  🔌 API Gateway:            http://localhost:5000"
echo "  📖 API Documentation:      http://localhost:5001/swagger"
echo "  🐰 RabbitMQ Management:    http://localhost:15672 (guest/guest)"
echo "  🌊 Node-RED:               http://localhost:1880"
echo "  📈 Grafana:                http://localhost:3001 (admin/admin)"
echo "  🎯 Prometheus:             http://localhost:9090"
echo ""
echo "Default Login: admin@scada.local / Admin123!"
echo ""
echo "To view logs: docker-compose logs -f"
echo "To stop: ./scripts/stop.sh"
echo ""
