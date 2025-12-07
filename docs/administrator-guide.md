# Enterprise SCADA System - Administrator Guide

## Table of Contents
1. [System Overview](#system-overview)
2. [Installation](#installation)
3. [Configuration](#configuration)
4. [User Management](#user-management)
5. [Security](#security)
6. [Monitoring](#monitoring)
7. [Backup & Recovery](#backup--recovery)
8. [Troubleshooting](#troubleshooting)

## System Overview

The Enterprise SCADA system consists of 6 microservices:
- **ScadaCore**: Tag management and metadata
- **DataAcquisition**: High-speed data collection (100k+ tags/sec)
- **AlarmManagement**: Alarm processing and notifications
- **AuthService**: Authentication and RBAC
- **ReportingService**: Scheduled reports
- **ApiGateway**: Unified API access point

## Installation

### Prerequisites
- Docker 24.0+ or Kubernetes 1.28+
- PostgreSQL 15+
- InfluxDB 2.7+
- RabbitMQ 3.12+
- Redis 7.0+

### Docker Compose Installation

```bash
# Clone repository
git clone <repository-url>
cd EMS

# Configure environment
cp .env.example .env
# Edit .env with your settings

# Start all services
docker-compose up -d

# Check status
docker-compose ps

# View logs
docker-compose logs -f scada-core
```

### Kubernetes Installation

```bash
# Create namespace
kubectl create namespace scada-system

# Create secrets
kubectl create secret generic scada-secrets -n scada-system \
  --from-literal=jwt-secret=your-secret-key \
  --from-literal=postgres-password=your-password \
  --from-literal=influxdb-token=your-token

# Deploy
kubectl apply -f infrastructure/kubernetes/scada-deployment.yaml

# Check pods
kubectl get pods -n scada-system

# Check services
kubectl get svc -n scada-system
```

## Configuration

### Environment Variables

Critical configuration in `.env`:

```env
# Database Connections
POSTGRES_CONNECTION=Host=postgres;Database=scada;Username=scada;Password=<strong-password>
INFLUXDB_URL=http://influxdb:8086
INFLUXDB_TOKEN=<generate-secure-token>

# Security
JWT_SECRET=<generate-256-bit-secret>
JWT_EXPIRY_MINUTES=60

# SMTP (Alarms)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USER=your-email@domain.com
SMTP_PASS=<app-specific-password>

# Twilio (SMS Alarms)
TWILIO_ACCOUNT_SID=<your-sid>
TWILIO_AUTH_TOKEN=<your-token>
TWILIO_FROM_NUMBER=+1234567890
```

### Generating Secrets

```bash
# Generate JWT secret (256-bit)
openssl rand -base64 32

# Generate InfluxDB token
influx auth create --all-access --org scada-org
```

### Tag Configuration

Tags are configured via API or database:

```sql
INSERT INTO tags (name, description, data_type, units, scan_rate, site, device)
VALUES ('SITE01.TURBINE01.Power', 'Turbine power output', 'Float', 'kW', 1000, 'SITE01', 'TURBINE01');
```

### Alarm Rules

Configure alarm thresholds:

```sql
INSERT INTO alarm_rules (tag_id, alarm_type, threshold, priority, message)
VALUES (1, 'High', 5000.0, 'Medium', 'Power output exceeded threshold');
```

## User Management

### Default Users

- **Username**: admin
- **Password**: Admin123!
- **Role**: Administrator

### Creating Users

Via AuthService API:

```bash
curl -X POST http://localhost:5004/api/users \
  -H "Content-Type: application/json" \
  -d '{
    "email": "operator@company.com",
    "username": "operator1",
    "password": "SecurePassword123!",
    "role": "Operator"
  }'
```

### Roles & Permissions

| Role | Permissions |
|------|-------------|
| **Administrator** | Full system access, user management |
| **Engineer** | Configure tags, alarms, reports |
| **Operator** | Monitor, control, acknowledge alarms |
| **Viewer** | Read-only access to dashboards |

### Enabling MFA

1. Login as user
2. Navigate to Settings > Security
3. Click "Enable Two-Factor Authentication"
4. Scan QR code with authenticator app (Google Authenticator, Authy)
5. Enter verification code
6. Save recovery codes

## Security

### Network Security

**Firewall Rules**:

```bash
# Allow only necessary ports
sudo ufw allow 3000/tcp   # Frontend
sudo ufw allow 5000/tcp   # API Gateway
sudo ufw deny 5432/tcp   # PostgreSQL (internal only)
sudo ufw deny 8086/tcp   # InfluxDB (internal only)
sudo ufw enable
```

**TLS/SSL Configuration**:

```bash
# Generate self-signed certificate (development)
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout /etc/ssl/private/scada.key \
  -out /etc/ssl/certs/scada.crt

# For production, use Let's Encrypt
certbot certonly --standalone -d scada.yourdomain.com
```

### Database Security

```sql
-- Create read-only user for reporting
CREATE USER scada_readonly WITH PASSWORD 'readonly_pass';
GRANT SELECT ON ALL TABLES IN SCHEMA public TO scada_readonly;

-- Revoke unnecessary permissions
REVOKE CREATE ON SCHEMA public FROM PUBLIC;
```

### Audit Logging

All administrative actions are logged:

```sql
SELECT * FROM audit_logs 
WHERE action = 'USER_CREATED' 
ORDER BY timestamp DESC 
LIMIT 100;
```

## Monitoring

### Prometheus Metrics

Access: http://localhost:9090

Key metrics to monitor:
- `scada_tag_scan_rate` - Current processing rate
- `scada_alarms_active` - Active alarm count
- `scada_influxdb_points_written_total` - Data points stored
- `scada_messages_processed_total` - Messages processed

### Grafana Dashboards

Access: http://localhost:3001 (admin/admin)

Pre-configured dashboards:
1. **System Overview** - KPIs and health status
2. **Performance Metrics** - CPU, memory, latency
3. **Data Acquisition** - Tag rates, buffer depth
4. **Alarms** - Active alarms, trends

### Health Checks

```bash
# Check all services
curl http://localhost:5000/health  # API Gateway
curl http://localhost:5001/health  # ScadaCore
curl http://localhost:5002/health  # DataAcquisition
curl http://localhost:5003/health  # AlarmManagement
curl http://localhost:5004/health  # AuthService
curl http://localhost:5005/health  # ReportingService
```

## Backup & Recovery

### Database Backups

**PostgreSQL**:

```bash
# Manual backup
docker exec scada-postgres pg_dump -U scada scada > backup_$(date +%Y%m%d).sql

#Automated daily backup (cron)
0 2 * * * docker exec scada-postgres pg_dump -U scada scada | gzip > /backups/scada_$(date +\%Y\%m\%d).sql.gz

# Restore
docker exec -i scada-postgres psql -U scada scada < backup_20250101.sql
```

**InfluxDB**:

```bash
# Backup
influx backup /backups/influxdb_$(date +%Y%m%d)

# Restore
influx restore /backups/influxdb_20250101
```

### Configuration Backups

```bash
# Backup all configs
tar -czf scada_config_$(date +%Y%m%d).tar.gz .env docker-compose.yml infrastructure/
```

### Disaster Recovery Plan

1. **RTO** (Recovery Time Objective): 4 hours
2. **RPO** (Recovery Point Objective): 15 minutes

**Recovery Steps**:

1. Restore database from latest backup
2. Deploy services from container registry
3. Restore configuration files
4. Verify health checks
5. Validate data flow
6. Resume operations

## Troubleshooting

### High Memory Usage

**InfluxDB**:

```bash
# Check cardinality
influx query 'SHOW SERIES CARDINALITY'

# Reduce retention
influx bucket update --name scada-data --retention 7d
```

**Redis**:

```bash
# Check memory usage
redis-cli INFO memory

# Clear cache if needed
redis-cli FLUSHALL
```

### RabbitMQ Queue Buildup

```bash
# Check queue depth
rabbitmqctl list_queues name messages consumers

# Purge queue (CAUTION!)
rabbitmqctl purge_queue data-acquisition

# Add more consumers (scale DataAcquisition)
kubectl scale deployment data-acquisition --replicas=10 -n scada-system
```

### Slow Database Queries

```sql
-- PostgreSQL slow queries
SELECT query, calls, total_time, mean_time 
FROM pg_stat_statements 
ORDER BY mean_time DESC 
LIMIT 10;

-- Add missing indexes
CREATE INDEX idx_tags_site_device ON tags(site, device);
CREATE INDEX idx_audit_logs_timestamp ON audit_logs(timestamp DESC);
```

### WebSocket Disconnections

**NGINX Config** (`/etc/nginx/sites-available/scada`):

```nginx
server {
    listen 443 ssl;
    server_name scada.yourdomain.com;

    location / {
        proxy_pass http://localhost:3000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_read_timeout 300s;
        proxy_send_timeout 300s;
    }
}
```

### Certificate Errors

```bash
# Verify certificate
openssl x509 -in /etc/ssl/certs/scada.crt -text -noout

# Check expiration
openssl x509 -in /etc/ssl/certs/scada.crt -noout -dates

# Renew Let's Encrypt
certbot renew
```

## Performance Tuning

### PostgreSQL

```sql
-- Increase connection pool
ALTER SYSTEM SET max_connections = 200;
ALTER SYSTEM SET shared_buffers = '4GB';
ALTER SYSTEM SET effective_cache_size = '12GB';
SELECT pg_reload_conf();
```

### RabbitMQ

```conf
# /etc/rabbitmq/rabbitmq.conf
vm_memory_high_watermark.relative = 0.6
disk_free_limit.absolute = 5GB
channel_max = 4096
```

### InfluxDB

```yaml
# config.yml
storage-cache-max-memory-size: 1073741824  # 1GB
storage-wal-max-concurrent-writes: 128
```

## Support

For technical support:
- **Email**: support@scada-system.com
- **Documentation**: https://docs.scada-system.com
- **Issue Tracker**: https://github.com/your-org/scada/issues

## Version History

- **v1.0.0** (2025-01-01) - Initial release
  - Core SCADA functionality
  - 100k+ tags/sec processing
  - Full microservices architecture
