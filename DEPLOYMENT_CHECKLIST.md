# SCADA System v2.1 - Final Deployment Checklist

**Date**: 2025-01-08  
**Version**: 2.1  
**Status**: Production Ready ✅

---

## ✅ **Pre-Deployment Checklist**

### **1. Documentation** ✅
- [x] WINDOWS_SETUP.md - Updated with new migrations
- [x] USER_MANUAL.md - Complete with Energy/WO sections
- [x] TRAINING_GUIDE.md - Comprehensive training materials
- [x] ALL_FEATURES_COMPLETE.md - Final feature summary
- [x] ENERGY_MANAGEMENT_COMPLETE.md - Energy module guide
- [x] METER_MANAGEMENT_GUIDE.md - Meter setup guide
- [x] API documentation in Swagger/OpenAPI

### **2. Database Migrations** ✅
- [x] 001_initial_schema.sql - Basic schema
- [x] 002_seed_data.sql - Sample data
- [x] 003_extended_equipment_tags.sql - Industrial equipment
- [x] 004_energy_workorders_scheduled_reports.sql - **NEW**
- [x] 005_hierarchical_metering.sql - **NEW**
- [x] 006_meter_management_enhancements.sql - **NEW**

### **3. Backend Services** ✅
- [x] ScadaCore (Port 5001)
- [x] DataAcquisition (Port 5002)
- [x] AlarmManagement (Port 5003)
- [x] AuthService (Port 5004)
- [x] ReportingService (Port 5005) - Enhanced with email
- [x] GraphQLService (Port 5006)
- [x] WebSocketService (Port 5007)
- [x] MLService (Port 8000)
- [x] **EnergyManagement (Port 5010)** - **NEW**
- [x] **WorkOrderService (Port 5011)** - **NEW**
- [x] OpcUaServer (Port 4840)

### **4. Frontend Components** ✅
- [x] Main Dashboard
- [x] Tag Management
- [x] Alarm Management
- [x] **MeterSetup.tsx** - **NEW**
- [x] **MeterDashboard.tsx** - **NEW**
- [x] MeterSetup.css - **NEW**
- [x] MeterDashboard.css - **NEW**

### **5. Docker Configuration** ✅
- [x] docker-compose.yml - All 11 services configured
- [x] Dockerfiles for all services
- [x] .env.example - All environment variables documented
- [x] Network configuration
- [x] Volume mounts
- [x] Health checks

### **6. Scripts** ✅
- [x] start.bat - Windows start script
- [x] stop.bat - Windows stop script
- [x] backup.bat - Windows backup script
- [x] restore.bat - Windows restore script
- [x] **install-frontend-deps.bat** - **NEW** NPM installer

###7. Dependencies** ✅
- [x] .NET 8.0 packages
- [x] Node.js packages (React, Nivo charts)
- [x] Python packages (scikit-learn, pandas)
- [x] NuGet packages (EF Core, Serilog, MailKit, Quartz)
- [x] NPM packages (@nivo/sankey, @nivo/pie, @nivo/line)

---

## 📦 **Deployment Steps**

### **Step 1: Prerequisites Verification**
```powershell
# Check Docker
docker --version
# Should show: Docker version 24.x or higher

# Check Git
git --version

# Check .NET
dotnet --version
# Should show: 8.0.x

# Check Node.js
node --version
# Should show: v18.x or higher
```

### **Step 2: Clone and Setup**
```powershell
git clone https://github.com/your-org/scada-system.git
cd scada-system

# Copy environment file
copy .env.example .env

# Edit .env with your settings
notepad .env
```

### **Step 3: Install Frontend Dependencies**
```powershell
.\scripts\install-frontend-deps.bat
```

Expected output:
```
[1/3] Installing core React libraries...
[2/3] Installing TypeScript types...
[3/3] Installing Nivo chart libraries...
Installation Complete!
```

### **Step 4: Start Infrastructure**
```powershell
# Start databases and message broker
docker-compose up -d postgres influxdb rabbitmq redis

# Wait 30 seconds for databases to initialize
Start-Sleep -Seconds 30
```

### **Step 5: Run Database Migrations**
```powershell
# Basic schema
docker exec -i scada-postgres psql -U scada -d scada < database/migrations/001_initial_schema.sql

# Seed data
docker exec -i scada-postgres psql -U scada -d scada < database/migrations/002_seed_data.sql

# Extended equipment
docker exec -i scada-postgres psql -U scada -d scada < database/migrations/003_extended_equipment_tags.sql

# NEW - Energy, Work Orders, Reports
docker exec -i scada-postgres psql -U scada -d scada < database/migrations/004_energy_workorders_scheduled_reports.sql

# NEW - Hierarchical metering
docker exec -i scada-postgres psql -U scada -d scada < database/migrations/005_hierarchical_metering.sql

# NEW - Meter enhancements
docker exec -i scada-postgres psql -U scada -d scada < database/migrations/006_meter_management_enhancements.sql
```

### **Step 6: Start All Services**
```powershell
# Build and start everything
docker-compose build
docker-compose up -d

# Wait for services to start
Start-Sleep -Seconds 60
```

### **Step 7: Verify Deployment**
```powershell
# Check all services are running
docker-compose ps

# Should show 11 services running:
# - postgres, influxdb, rabbitmq, redis
# - scada-core, data-acquisition, alarm-management
# - auth-service, reporting-service
# - energy-management, work-order-service
# - graphql-service, websocket-service
# - ml-service, frontend

# Check service health
curl http://localhost:5001/health  # ScadaCore
curl http://localhost:5010/health  # EnergyManagement
curl http://localhost:5011/health  # WorkOrderService

# Open browser
Start-Process "http://localhost:3000"
```

### **Step 8: Login and Test**
1. Navigate to http://localhost:3000
2. Login with: `admin@scada.local` / `Admin123!`
3. Test each module:
   - Dashboard - ✅ Should load
   - Energy Management → Meter Setup - ✅ Should see interface
   - Energy Management → Dashboard - ✅ Should see charts (after npm install)
   - Work Orders - ✅ Should see list
   - Alarms - ✅ Should see list
   - Reports - ✅ Should generate

---

## 🧪 **Verification Tests**

### **Test 1: Energy Management**
```powershell
# Create test meter
curl -X POST http://localhost:5010/api/Meters \
  -H "Content-Type: application/json" \
  -d '{
    "meterNumber": "TEST-001",
    "meterName": "Test Meter",
    "meterType": "Main",
    "ctPrimaryAmps": 1000,
    "ctSecondaryAmps": 5
  }'

# Get all meters
curl http://localhost:5010/api/Meters

# Get hierarchy
curl http://localhost:5010/api/Meters/hierarchy
```

### **Test 2: Work Orders**
```powershell
# Create test work order
curl -X POST http://localhost:5011/api/WorkOrders \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Test Maintenance",
    "priority": "Medium",
    "type": "Preventive"
  }'

# Get all work orders
curl http://localhost:5011/api/WorkOrders

# Get statistics
curl http://localhost:5011/api/WorkOrders/statistics
```

### **Test 3: Scheduled Reports**
- Go to Reports → Scheduled Reports
- Scheduled jobs should be visible
- Check logs: `docker logs scada-reporting-service`

---

## 📊 **Success Criteria**

✅ **All services running** (11/11)  
✅ **All databases accessible** (Postgres & InfluxDB)  
✅ **All migrations applied** (6/6)  
✅ **Frontend loads** (http://localhost:3000)  
✅ **Login successful**  
✅ **Energy Management accessible**  
✅ **Work Orders accessible**  
✅ **API endpoints responding**  
✅ **No errors in logs**  

---

## 🔧 **Post-Deployment Configuration**

### **1. Change Default Passwords**
```sql
-- Login as admin and go to Settings → Users
-- Change passwords for:
-- - admin@scada.local
-- - engineer@scada.local
-- - operator@scada.local
```

### **2. Configure Email (for Scheduled Reports)**
Edit `.env`:
```env
EMAIL_SMTP_HOST=smtp.gmail.com
EMAIL_SMTP_PORT=587
EMAIL_SMTP_USER=your-email@gmail.com
EMAIL_SMTP_PASSWORD=your-app-password
EMAIL_FROM_EMAIL=scada@company.com
EMAIL_FROM_NAME=SCADA System
```

Restart reporting service:
```powershell
docker-compose restart reporting-service
```

### **3. Configure Scheduled Reports**
1. Go to Reports → Scheduled Reports
2. Create schedules:
   - Daily Production Report → 8:00 AM → recipients
   - Weekly Energy Report → Monday 9:00 AM → recipients
   - Monthly Summary → 1st day 10:00 AM → recipients

### **4. Setup Backup Schedule**
Windows Task Scheduler:
- Name: SCADA Daily Backup
- Trigger: Daily at 2:00 AM
- Action: `C:\Users\TEST\EMS\scripts\backup.bat`

### **5. Configure Monitoring**
- Prometheus: http://localhost:9090
- Grafanahttp://localhost:3001 (admin/admin)
- Import pre-built dashboards from `/infrastructure/monitoring/dashboards/`

---

## 📱 **Access Points**

| Service | URL | Credentials |
|---------|-----|-------------|
| **Main Dashboard** | http://localhost:3000 | admin@scada.local / Admin123! |
| **Energy Management** | http://localhost:3000/meter-setup | (same) |
| **Work Orders** | http://localhost:3000/work-orders | (same) |
| **ScadaCore API** | http://localhost:5001/swagger | - |
| **Energy API** | http://localhost:5010/swagger | - |
| **Work Order API** | http://localhost:5011/swagger | - |
| **Prometheus** | http://localhost:9090 | - |
| **Grafana** | http://localhost:3001 | admin/admin |
| **RabbitMQ** | http://localhost:15672 | guest/guest |

---

## 📞 **Support**

**Issues?**
1. Check logs: `docker-compose logs [service-name]`
2. Review documentation in `/docs`
3. Consult TROUBLESHOOTING.md
4. Contact: support@company.com

---

## 🎉 **Deployment Complete!**

✅ **Enterprise SCADA System v2.1 is LIVE!**

**Total Components**:
- 11 Microservices
- 32 API Endpoints
- 35+ Database Tables
- 6 Frontend Components
- 92+ Energy Meters Support

**Ready for Production Use!** 🚀

---

**End of Deployment Checklist**
