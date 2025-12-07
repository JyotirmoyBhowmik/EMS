# Enterprise SCADA System - Quick Start Guide

## 🚀 Get Started in 5 Minutes

### Step 1: Prerequisites

**Required Software:**
- Docker Desktop 24.0+ ([Download](https://www.docker.com/products/docker-desktop))
- Git ([Download](https://git-scm.com/downloads))

**Optional (for local development):**
- .NET SDK 8.0+ ([Download](https://dotnet.microsoft.com/download))
- Node.js 18.x+ ([Download](https://nodejs.org/))

### Step 2: Clone & Configure

```bash
# Clone the repository
git clone <repository-url>
cd EMS

# Create configuration file
cp .env.example .env

# Edit .env with your settings (optional for local testing)
# Default settings work for development
```

### Step 3: Start the System

**Windows:**
```cmd
scripts\start.bat
```

**Linux/Mac:**
```bash
chmod +x scripts/start.sh
./scripts/start.sh
```

**Or use Docker Compose directly:**
```bash
docker-compose up -d
```

### Step 4: Access the System

Wait 30-60 seconds for all services to initialize, then access:

| Service | URL | Credentials |
|---------|-----|-------------|
| **🖥️ Dashboard** | http://localhost:3000 | admin@scada.local / Admin123! |
| **📖 API Docs** | http://localhost:5001/swagger | - |
| **🔌 API Gateway** | http://localhost:5000 | - |
| **🐰 RabbitMQ** | http://localhost:15672 | guest / guest |
| **📊 Grafana** | http://localhost:3001 | admin / admin |
| **🌊 Node-RED** | http://localhost:1880 | - |

### Step 5: Explore

1. **Login to Dashboard** at http://localhost:3000
   - Username: `admin@scada.local`
   - Password: `Admin123!`

2. **View Live Data**
   - See real-time KPIs on the dashboard
   - Check tag values for wind turbines and solar panels
   - Monitor system status

3. **Test the API**
   - Import Postman collection: `docs/api/SCADA-API.postman_collection.json`
   - Or browse Swagger UI: http://localhost:5001/swagger

4. **Configure Protocol Drivers**
   - Open Node-RED: http://localhost:1880
   - Modify flows for your PLCs/devices
   - Deploy changes

## 📚 What's Included

✅ **6 Microservices**: ScadaCore, DataAcquisition, AlarmManagement, AuthService, ReportingService, ApiGateway  
✅ **Modern Frontend**: React 18 with real-time updates  
✅ **Protocol Support**: Modbus, MQTT, OPC UA, BACnet  
✅ **High Performance**: 100k+ tags/sec capability  
✅ **Complete Monitoring**: Prometheus + Grafana  
✅ **Security**: JWT authentication + MFA ready  

## 🛠️ Common Tasks

### View Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f scada-core
docker-compose logs -f data-acquisition
```

### Restart a Service
```bash
docker-compose restart scada-core
```

### Stop the System
```bash
# Windows
scripts\stop.bat

# Linux/Mac
./scripts/stop.sh

# Or directly
docker-compose down
```

### Backup Data
```bash
./scripts/backup.sh
```

### Check Service Health
```bash
curl http://localhost:5000/health        # API Gateway
curl http://localhost:5001/health        # ScadaCore
curl http://localhost:5002/health        # DataAcquisition
```

## 🎯 Next Steps

### For Development

1. **Backend Development:**
   ```bash
   cd backend/ScadaCore
   dotnet restore
   dotnet run
   ```

2. **Frontend Development:**
   ```bash
   cd frontend/scada-dashboard
   npm install
   npm run dev
   ```

3. **Run Tests:**
   ```bash
   cd backend/ScadaCore.Tests
   dotnet test
   ```

### For Production

1. **Review Configuration**
   - Update `.env` with production settings
   - Set strong JWT secrets
   - Configure SMTP for email alerts
   - Set up Twilio for SMS notifications

2. **Deploy to Kubernetes:**
   ```bash
   kubectl apply -f infrastructure/kubernetes/scada-deployment.yaml
   ```

3. **Enable Security Features**
   - Configure TLS/SSL certificates
   - Enable MFA for all users
   - Set up firewall rules
   - Review security best practices in `docs/security-best-practices.md`

## 📖 Documentation

- **README**: `README.md` - System overview
- **Administrator Guide**: `docs/administrator-guide.md` - Detailed configuration
- **Security Guide**: `docs/security-best-practices.md` - Security hardening
- **Walkthrough**: See artifacts for implementation details
- **API Reference**: http://localhost:5001/swagger

## 🐛 Troubleshooting

### Services Won't Start
```bash
# Check Docker is running
docker ps

# Check for port conflicts
netstat -an | grep 3000
netstat -an | grep 5000

# View service logs
docker-compose logs
```

### Database Connection Errors
```bash
# Restart databases
docker-compose restart postgres influxdb

# Check if they're ready
docker exec scada-postgres pg_isready -U scada
docker exec scada-influxdb influx ping
```

### Frontend Can't Connect to API
- Ensure API Gateway is running: http://localhost:5000/health
- Check CORS settings in `.env`
- Clear browser cache

### High Memory Usage
- Reduce number of replicas in `docker-compose.yml`
- Adjust InfluxDB retention policies
- Clear Redis cache if needed

## 💡 Tips

- **Sample Data**: System includes pre-configured tags for wind turbines, solar panels, and batteries
- **Real-time Updates**: Dashboard updates every 2 seconds via WebSocket
- **Scalability**: Adjust `docker-compose.yml` replicas for DataAcquisition service
- **Monitoring**: Check Grafana dashboards for system performance metrics
- **Protocol Testing**: Use Node-RED to simulate device data

## 🆘 Getting Help

- **Documentation**: Check `docs/` folder
- **Logs**: Use `docker-compose logs -f` for debugging
- **Health Checks**: All services expose `/health` endpoint
- **Issues**: Check GitHub issues or create a new one

## 🎉 Success!

You now have a fully functional Enterprise SCADA system running!

**Next**: Explore the dashboard, configure your devices, and customize for your needs.

---

**Author**: Jyotirmoy Bhowmik (jyotirmoy.bhowmik@gmail.com)  
**Version**: 1.0.0  
**Last Updated**: 2025-01-01
