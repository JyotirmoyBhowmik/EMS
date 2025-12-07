#!/bin/bash
# Database Backup Script

BACKUP_DIR="./backups"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

mkdir -p $BACKUP_DIR

echo "🔄 Backing up SCADA databases..."

# Backup PostgreSQL
echo "📦 Backing up PostgreSQL..."
docker exec scada-postgres pg_dump -U scada scada | gzip > "$BACKUP_DIR/postgres_$TIMESTAMP.sql.gz"
echo "✅ PostgreSQL backup: $BACKUP_DIR/postgres_$TIMESTAMP.sql.gz"

# Backup InfluxDB
echo "📦 Backing up InfluxDB..."
docker exec scada-influxdb influx backup "/tmp/influx_backup_$TIMESTAMP" 2>/dev/null || true
docker cp scada-influxdb:/tmp/influx_backup_$TIMESTAMP "$BACKUP_DIR/influx_$TIMESTAMP"
echo "✅ InfluxDB backup: $BACKUP_DIR/influx_$TIMESTAMP"

# Backup configuration
echo "📦 Backing up configuration..."
tar -czf "$BACKUP_DIR/config_$TIMESTAMP.tar.gz" .env docker-compose.yml infrastructure/ 2>/dev/null
echo "✅ Configuration backup: $BACKUP_DIR/config_$TIMESTAMP.tar.gz"

# Clean old backups (keep last 7 days)
echo "🧹 Cleaning old backups..."
find $BACKUP_DIR -name "*.gz" -mtime +7 -delete
find $BACKUP_DIR -name "*.tar.gz" -mtime +7 -delete
find $BACKUP_DIR -type d -name "influx_*" -mtime +7 -exec rm -rf {} + 2>/dev/null || true

echo ""
echo "✅ Backup completed successfully!"
echo "Backup location: $BACKUP_DIR"
echo ""
