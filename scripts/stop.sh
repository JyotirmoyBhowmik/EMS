#!/bin/bash
# SCADA System Shutdown Script

echo "🛑 Stopping Enterprise SCADA System..."

# Stop all services gracefully
docker-compose down

echo "✅ All services stopped"
echo ""
echo "To start again: ./scripts/start.sh"
echo "To remove all data: docker-compose down -v"
echo ""
