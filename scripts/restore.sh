#!/bin/bash
# Database Restore Script for Linux/Mac
# Usage: ./restore.sh <backup-file.tar.gz>

if [ -z "$1" ]; then
    echo "Error: Please provide backup file path"
    echo "Usage: ./restore.sh <backup-file.tar.gz>"
    exit 1
fi

BACKUP_FILE="$1"

if [ ! -f "$BACKUP_FILE" ]; then
    echo "Error: Backup file not found: $BACKUP_FILE"
    exit 1
fi

echo "========================================"
echo "SCADA System Restore Script"
echo "========================================"
echo ""
echo "WARNING: This will replace all existing data!"
read -p "Are you sure you want to continue? (yes/no): " confirm

if [ "$confirm" != "yes" ]; then
    echo "Restore cancelled."
    exit 0
fi

RESTORE_DIR="restore_temp"
mkdir -p "$RESTORE_DIR"

echo ""
echo "[1/4] Extracting backup..."
tar -xzf "$BACKUP_FILE" -C "$RESTORE_DIR"
echo "✓ Extraction completed"

echo ""
echo "[2/4] Restoring PostgreSQL database..."
docker exec -i scada-postgres psql -U scada -d postgres -c "DROP DATABASE IF EXISTS scada;"
docker exec -i scada-postgres psql -U scada -d postgres -c "CREATE DATABASE scada;"
docker exec -i scada-postgres psql -U scada -d scada < "$RESTORE_DIR/postgres_backup.sql"
echo "✓ PostgreSQL restore completed"

echo ""
echo "[3/4] Restoring InfluxDB database..."
docker cp "$RESTORE_DIR/influxdb-backup" scada-influxdb:/tmp/
docker exec scada-influxdb influx restore /tmp/influxdb-backup -t scada-token-change-in-production
echo "✓ InfluxDB restore completed"

echo ""
echo "[4/4] Restoring configuration files..."
cp "$RESTORE_DIR/.env" .env 2>/dev/null
cp -r "$RESTORE_DIR/protocols" protocols 2>/dev/null
echo "✓ Configuration restore completed"

echo ""
echo "Cleaning up..."
rm -rf "$RESTORE_DIR"
echo "✓ Cleanup completed"

echo ""
echo "========================================"
echo "Restore completed successfully!"
echo "Please restart SCADA system:"
echo "  docker-compose restart"
echo "========================================"
