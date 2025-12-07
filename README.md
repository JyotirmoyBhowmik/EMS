# Enterprise SCADA System

## System Overview

Enterprise-grade SCADA (Supervisory Control and Data Acquisition) system designed for mission-critical energy operations with 24/7 uptime, handling 100,000+ tags per second.

**Author**: Jyotirmoy Bhowmik (jyotirmoy.bhowmik@gmail.com)

## Architecture

- **Backend**: Microservices architecture with .NET Core 8.0
- **Frontend**: React 18 with TypeScript
- **Time-Series Database**: InfluxDB 2.x
- **Message Broker**: RabbitMQ
- **Protocol Middleware**: Node-RED
- **Deployment**: Docker & Kubernetes

## Features

✅ **High Performance**: 100,000+ tags/second data acquisition  
✅ **Universal Connectivity**: Modbus, OPC UA, MQTT, BACnet, SNMP, Ethernet/IP  
✅ **Store-and-Forward**: Data buffering during network outages  
✅ **Real-Time Monitoring**: Live dashboards with WebSocket updates  
✅ **Advanced Alarms**: Predictive analytics and root cause analysis  
✅ **Enterprise Security**: MFA, RBAC, encryption, audit logging  
✅ **Scalability**: Kubernetes auto-scaling from hundreds to millions of tags  
✅ **Mobile Access**: Responsive web interface for remote monitoring  

## Project Structure

```
EMS/
├── backend/                    # .NET Core microservices
│   ├── ScadaCore/             # Tag management & core engine
│   ├── DataAcquisition/       # High-speed data collection
│   ├── AlarmManagement/       # Alarm processing & notifications
│   ├── AuthService/           # Authentication & authorization
│   ├── ReportingService/      # Report generation
│   └── ApiGateway/            # Unified API gateway
├── frontend/                   # React web application
│   └── scada-dashboard/       # Main dashboard
├── protocols/                  # Protocol drivers
│   └── node-red/              # Node-RED flows
├── database/                   # Database configurations
│   ├── influxdb/              # Time-series setup
│   └── postgres/              # Relational database
├── infrastructure/             # Deployment & monitoring
│   ├── docker/                # Docker configurations
│   ├── kubernetes/            # K8s manifests
│   └── monitoring/            # Prometheus/Grafana
└── docs/                       # Documentation
    ├── api/                   # API documentation
    ├── deployment/            # Deployment guides
    └── user-guides/           # User manuals
```

## Prerequisites

- **.NET SDK** 8.0 or higher
- **Node.js** 18.x or higher
- **Docker** & **Docker Compose**
- **Kubernetes** cluster (for production)

## Quick Start

### 1. Install Dependencies

**Backend:**
```bash
cd backend
dotnet restore
```

**Frontend:**
```bash
cd frontend/scada-dashboard
npm install
```

### 2. Run with Docker Compose

```bash
docker-compose up -d
```

This starts:
- All backend microservices
- Frontend development server
- InfluxDB (port 8086)
- PostgreSQL (port 5432)
- RabbitMQ (port 5672, management: 15672)
- Redis (port 6379)
- Node-RED (port 1880)

### 3. Access the System

- **Dashboard**: http://localhost:3000
- **API Gateway**: http://localhost:5000
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)
- **Node-RED**: http://localhost:1880
- **InfluxDB UI**: http://localhost:8086

### 4. Default Credentials

- **Admin User**: admin@scada.local
- **Password**: Admin123!
- **MFA**: Disabled by default (enable in settings)

## Development

### Backend Services

Each microservice can be run independently:

```bash
cd backend/ScadaCore
dotnet run
```

### Frontend

```bash
cd frontend/scada-dashboard
npm run dev
```

### Protocol Testing

Access Node-RED at http://localhost:1880 to configure protocol flows.

## Deployment

### Kubernetes

```bash
# Apply configurations
kubectl apply -f infrastructure/kubernetes/

# Check status
kubectl get pods -n scada-system
```

### Scaling

```bash
# Scale data acquisition service
kubectl scale deployment data-acquisition --replicas=10 -n scada-system
```

## Configuration

### Environment Variables

Copy `.env.example` to `.env` and configure:

```env
# Database
INFLUXDB_URL=http://influxdb:8086
INFLUXDB_TOKEN=your-token-here
POSTGRES_CONNECTION=Host=postgres;Database=scada;Username=scada;Password=your-password

# RabbitMQ
RABBITMQ_HOST=rabbitmq
RABBITMQ_PORT=5672
RABBITMQ_USER=guest
RABBITMQ_PASS=guest

# Security
JWT_SECRET=your-secret-key-change-in-production
JWT_EXPIRY=3600

# SMTP (for alerts)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USER=your-email@gmail.com
SMTP_PASS=your-app-password
```

## Performance Tuning

### Tag Throughput

Configure in `backend/DataAcquisition/appsettings.json`:

```json
{
  "DataAcquisition": {
    "MaxConcurrentScans": 100,
    "BatchSize": 1000,
    "ScanInterval": 100
  }
}
```

### Database Retention

InfluxDB retention policies:
- **Raw data**: 30 days
- **1-minute aggregates**: 1 year
- **1-hour aggregates**: 5 years

## Security

### Enable MFA

1. Login as admin
2. Navigate to Settings > Security
3. Enable Two-Factor Authentication
4. Scan QR code with authenticator app

### RBAC Roles

- **Administrator**: Full system access
- **Operator**: Monitor and control
- **Engineer**: Configure tags and alarms
- **Viewer**: Read-only access

### Network Security

Recommended firewall rules:
```bash
# Allow only necessary ports
ufw allow 3000/tcp  # Frontend
ufw allow 5000/tcp  # API Gateway
ufw deny 5432/tcp   # PostgreSQL (internal only)
ufw deny 8086/tcp   # InfluxDB (internal only)
```

## Monitoring

### Grafana Dashboards

Pre-configured dashboards:
1. **System Overview**: KPIs, tag rates, alarms
2. **Performance Metrics**: CPU, memory, latency
3. **Data Acquisition**: Tag scan rates, error rates
4. **Alarms**: Active alarms, acknowledgment times

Access: http://localhost:3001 (admin/admin)

### Prometheus Metrics

Custom metrics exposed:
- `scada_tags_scanned_total`: Total tags scanned
- `scada_tag_scan_duration`: Scan duration histogram
- `scada_alarms_active`: Current active alarms
- `scada_messages_processed`: RabbitMQ message throughput

## Troubleshooting

### High Memory Usage

Check InfluxDB cardinality:
```bash
influx query 'SHOW SERIES CARDINALITY'
```

### RabbitMQ Queue Buildup

Check consumer lag:
```bash
rabbitmqctl list_queues name messages consumers
```

### WebSocket Disconnections

Increase nginx timeout:
```nginx
proxy_read_timeout 300s;
proxy_send_timeout 300s;
```

## API Documentation

Interactive API documentation available at:
- **Swagger UI**: http://localhost:5000/swagger
- **ReDoc**: http://localhost:5000/redoc

## Testing

### Unit Tests
```bash
dotnet test
```

### Integration Tests
```bash
dotnet test --filter Category=Integration
```

### Load Testing
```bash
k6 run scripts/performance/load-test.js
```

### E2E Tests
```bash
cd frontend/scada-dashboard
npm run test:e2e
```

## Compliance

Designed to support:
- **ISA/IEC 62443**: Industrial cybersecurity
- **NERC CIP**: Critical infrastructure protection
- **FDA 21 CFR Part 11**: Electronic records (if applicable)

## Support & Documentation

- **User Guide**: `docs/user-guides/user-manual.pdf`
- **Admin Guide**: `docs/user-guides/admin-manual.pdf`
- **API Reference**: `docs/api/api-reference.md`
- **Deployment Guide**: `docs/deployment/production-deployment.md`

## License

Proprietary - Enterprise License  
Copyright © 2025 - All rights reserved

## Contact

**Author**: Jyotirmoy Bhowmik  
**Email**: jyotirmoy.bhowmik@gmail.com

## Version History

- **v1.0.0** (Current) - Initial release with core SCADA functionality
