# Enterprise SCADA System v2.0 - Complete Training Guide

**Version**: 1.0  
**Date**: 2025-01-01  
**For**: Administrators, Operators, Developers  
**Duration**: 2-3 Days (varies by role)

---

## Training Overview

This comprehensive guide covers:
- ✅ **Administrator Training** (Day 1-2)
- ✅ **Operator Training** (Day 1)
- ✅ **Developer Training** (Day 2-3)
- ✅ **Maintenance Personnel** (Day 1)

---

## 📋 Table of Contents

1. [Getting Started](#getting-started)
2. [Administrator Training](#administrator-training)
3. [Operator Training](#operator-training)
4. [Developer Training](#developer-training)
5. [Maintenance Training](#maintenance-training)
6. [Troubleshooting](#troubleshooting)
7. [Best Practices](#best-practices)
8. [Certification](#certification)

---

## 1. Getting Started

### Prerequisites

**All Users**:
- Basic computer skills
- Windows/Linux familiarity
- Web browser knowledge

**Administrators**:
- Networking basics
- Command line experience
- Docker/container concepts

**Developers**:
- Programming experience (C# or JavaScript)
- API/REST concepts
- Git/version control

### Training Materials

- 📖 This guide
- 💻 Hands-on lab environment
- 🎥 Video tutorials (optional)
- 📝 Cheat sheets
- ✅ Practice exercises

---

## 2. Administrator Training

**Duration**: 2 days  
**Level**: Intermediate  
**Goal**: Full system operation and maintenance

### Day 1: System Administration

#### Module 1.1: System Architecture (2 hours)

**Topics**:
- Understanding microservices architecture
- 10 services overview
- Data flow diagram
- Network architecture

**Hands-On**:
```powershell
# View all running services
docker ps

# Check service health
curl http://localhost:5001/health  # ScadaCore
curl http://localhost:5002/health  # DataAcquisition
curl http://localhost:8000/health  # ML Service

# View logs
docker logs scada-core
docker logs scada-data-acquisition
```

**Exercise 1**: Draw the data flow from PLC to Dashboard

---

#### Module 1.2: Starting & Stopping System (1 hour)

**Starting the System**:

```powershell
# Windows
cd C:\Users\TEST\EMS
.\scripts\start.bat

# Or manually
docker-compose up -d

# Check status
docker-compose ps
```

**Stopping the System**:

```powershell
# Graceful shutdown
docker-compose down

# Force stop (if needed)
docker-compose down --remove-orphans

# Stop specific service
docker-compose stop scada-core
```

**Restarting Services**:

```powershell
# Restart single service
docker-compose restart scada-core

# Restart all
docker-compose restart

# Restart with rebuild
docker-compose up -d --build scada-core
```

**Exercise 2**: Practice starting, stopping, and restarting services

---

#### Module 1.3: User Management (2 hours)

**Creating Users**:

1. Open dashboard: http://localhost:3000
2. Login as admin (admin@scada.local / Admin123!)
3. Navigate to Settings → Users
4. Click "Add User"
5. Fill details:
   - Email
   - Password
   - Role (Administrator, Operator, Engineer, Viewer)
   - Site access

**User Roles**:

| Role | Permissions |
|------|-------------|
| **Administrator** | Full access, user management, system config |
| **Engineer** | Tag management, alarm config, reports |
| **Operator** | View dashboards, acknowledge alarms |
| **Viewer** | Read-only access |

**Managing Passwords**:

```powershell
# Reset user password via API
curl -X POST http://localhost:5000/api/auth/reset-password \
  -H "Content-Type: application/json" \
  -d '{"email":"user@company.com","newPassword":"NewPass123!"}'
```

**Exercise 3**: Create 3 users with different roles

---

#### Module 1.4: Tag Management (3 hours)

**Understanding Tags**:
- Tag = A data point from a device (e.g., temperature sensor)
- Tag Name Format: `SITE.DEVICE.PARAMETER` (e.g., SITE01.TURBINE01.Power)

**Creating Tags**:

1. Go to Tags → Add Tag
2. Fill form:
   - Name: SITE01.TANK01.Level
   - Description: Water Tank Level
   - Data Type: Float
   - Unit: meters
   - Min/Max: 0-10
   - Device: Select PLC

**Importing Bulk Tags** (CSV):

```csv
name,description,dataType,unit,min,max
SITE01.TANK01.Level,Water Level,Float,meters,0,10
SITE01.TANK01.Temperature,Water Temp,Float,celsius,0,100
SITE01.PUMP01.Status,Pump Running,Boolean,,0,1
```

Upload via: Tags → Import → Select CSV

**Exercise 4**: Create 10 tags for a mock site

---

#### Module 1.5: Alarm Configuration (2 hours)

**Creating Alarm Rules**:

1. Go to Alarms → Rules → Add Rule
2. Configure:
   - Tag: Select tag
   - Condition: `value > 90` (high level alarm)
   - Priority: High
   - Message: "Tank level critically high"
   - Actions: Email + SMS

**Alarm Priorities**:
- 🔴 **Critical**: Immediate action required
- 🟠 **High**: Urgent attention
- 🟡 **Medium**: Monitor closely
- 🟢 **Low**: Informational

**Testing Alarms**:

```powershell
# Manually trigger test alarm
curl -X POST http://localhost:5000/api/alarms/test \
  -H "Content-Type: application/json" \
  -d '{"tagName":"SITE01.TANK01.Level","value":95}'
```

**Exercise 5**: Create alarm rules for high/low limits

---

### Day 2: Advanced Administration

#### Module 2.1: Database Management (2 hours)

**PostgreSQL (Metadata)**:

```powershell
# Access database
docker exec -it scada-postgres psql -U scada -d scada

# Useful queries
SELECT * FROM tags LIMIT 10;
SELECT * FROM users;
SELECT * FROM alarm_events WHERE triggered_at > NOW() - INTERVAL '1 day';

# Backup database
docker exec scada-postgres pg_dump -U scada scada > backup_$(date +%Y%m%d).sql

# Restore database
docker exec -i scada-postgres psql -U scada scada < backup_20250101.sql
```

**InfluxDB (Time-Series Data)**:

```powershell
# Access InfluxDB shell
docker exec -it scada-influxdb influx

# Query data
> use scada-data
> SELECT * FROM tags WHERE time > now() - 1h LIMIT 10
> SELECT mean(value) FROM tags WHERE tagName='SITE01.TURBINE01.Power' AND time > now() - 24h GROUP BY time(1h)
```

**Exercise 6**: Backup both databases

---

#### Module 2.2: Monitoring & Performance (2 hours)

**Prometheus Metrics**:

Access: http://localhost:9090

**Useful Queries**:
```promql
# CPU usage
container_cpu_usage_seconds_total

# Memory usage
container_memory_usage_bytes

# Data ingestion rate
rate(scada_tags_processed_total[5m])

# API response time
histogram_quantile(0.95, scada_api_request_duration_seconds_bucket)
```

**Grafana Dashboards**:

Access: http://localhost:3001 (admin/admin)

**Pre-built Dashboards**:
1. System Overview
2. Service Health
3. Tag Ingestion
4. API Performance
5. Database Stats

**Exercise 7**: Create alert rule for high CPU usage

---

#### Module 2.3: Troubleshooting (3 hours)

**Common Issues**:

**Issue 1: Service Won't Start**

```powershell
# Check logs
docker logs scada-core --tail 50

# Check dependencies
docker-compose ps  # Are databases running?

# Restart with fresh logs
docker-compose restart scada-core
docker logs -f scada-core
```

**Issue 2: High Memory Usage**

```powershell
# Check memory stats
docker stats

# Restart memory-heavy service
docker-compose restart data-acquisition

# Increase Docker memory limit
# Docker Desktop → Settings → Resources → Memory
```

**Issue 3: Database Connection Errors**

```powershell
# Test PostgreSQL connection
docker exec scada-postgres psql -U scada -d scada -c "SELECT version();"

# Check connection string in .env
cat .env | grep POSTGRES

# Restart database
docker-compose restart postgres
```

**Exercise 8**: Troubleshoot intentionally broken service

---

#### Module 2.4: Backup & Recovery (2 hours)

**Automated Backup Script**:

```powershell
# Create backup directory
mkdir -p backups/$(date +%Y%m%d)

# Backup PostgreSQL
docker exec scada-postgres pg_dump -U scada scada > backups/$(date +%Y%m%d)/postgres.sql

# Backup InfluxDB
docker exec scada-influxdb influx backup /tmp/influx-backup
docker cp scada-influxdb:/tmp/influx-backup backups/$(date +%Y%m%d)/

# Backup configuration
cp .env backups/$(date +%Y%m%d)/
cp -r protocols backups/$(date +%Y%m%d)/

echo "Backup completed: backups/$(date +%Y%m%d)/"
```

**Schedule Daily Backups** (Windows Task Scheduler):

1. Open Task Scheduler
2. Create Task → Name: "SCADA Daily Backup"
3. Trigger: Daily at 2:00 AM
4. Action: Run `C:\Users\TEST\EMS\scripts\backup.bat`

**Exercise 9**: Perform full backup and test restore

---

## 3. Operator Training

**Duration**: 1 day  
**Level**: Beginner  
**Goal**: Daily operations and monitoring

### Module 3.1: Dashboard Navigation (1 hour)

**Login**:
1. Open http://localhost:3000
2. Enter credentials
3. Enable MFA (if required)

**Main Dashboard**:
- 📊 KPI tiles (power, efficiency, status)
- 📈 Real-time charts
- 🔔 Active alarms panel
- 🗺️ Site map

**Navigation**:
- Dashboard → Home view
- Tags → All data points
- Alarms → Active alerts
- Reports → Generated reports
- Analytics → ML insights

**Exercise 10**: Navigate to all main sections

---

### Module 3.2: Monitoring Tags (2 hours)

**Viewing Real-Time Data**:

1. Go to Tags page
2. Search for tag: "SITE01.TURBINE01"
3. View current value, quality, timestamp
4. Click tag → See historical chart

**Creating Custom Views**:

1. Dashboard → Create Custom View
2. Drag & drop widgets:
   - Line chart (power over time)
   - Gauge (current temperature)
   - Status indicator (pump running)
3. Save as "My View"

**Exercise 11**: Create custom dashboard for your area

---

### Module 3.3: Alarm Management (2 hours)

**Viewing Active Alarms**:

Alarms page shows:
- 🔴 Active alarms (requires action)
- 🟢 Acknowledged alarms (being handled)
- ⚪ Cleared alarms (resolved)

**Acknowledging Alarms**:

1. Click alarm row
2. Click "Acknowledge"
3. Add comment: "Investigating high temperature"
4. Click "Save"

**Alarm Actions**:
- **Acknowledge**: I'm aware, investigating
- **Shelve**: Temporarily suppress (e.g., during maintenance)
- **Clear**: Manually mark as resolved

**Exercise 12**: Practice alarm workflow

---

### Module 3.4: Generating Reports (1 hour)

**On-Demand Reports**:

1. Go to Reports → Generate
2. Select:
   - Report Type: Daily Summary
   - Date Range: Last 7 days
   - Tags: Select sites/devices
3. Click "Generate"
4. Download PDF or Excel

**Scheduled Reports**:

View scheduled reports:
- Daily Production Report (8:00 AM)
- Weekly Trend Report (Monday 9:00 AM)
- Monthly Summary (1st of month)

**Exercise 13**: Generate weekly report

---

### Module 3.5: Mobile Access (1 hour)

**Installing PWA**:

On mobile:
1. Open http://scada.company.com in browser
2. Click "Install App" prompt
3. App installs to home screen

**Mobile Features**:
- ✅ View dashboards
- ✅ Monitor alarms
- ✅ Acknowledge alerts
- ✅ Offline mode (view cached data)
- ✅ Push notifications

**Exercise 14**: Install and test mobile app

---

## 4. Developer Training

**Duration**: 2-3 days  
**Level**: Advanced  
**Goal**: Extend and customize system

### Module 4.1: Development Setup (2 hours)

**Prerequisites**:
- Visual Studio Code
- .NET 8.0 SDK
- Node.js 18+
- Docker Desktop

**Clone Repository**:

```bash
git clone https://github.com/your-org/scada-system.git
cd scada-system
```

**Start Development Environment**:

```bash
# Backend (each service separately)
cd backend/ScadaCore
dotnet run

# Frontend
cd frontend/scada-dashboard
npm install
npm run dev
```

**Exercise 15**: Run backend service locally

---

### Module 4.2: API Development (3 hours)

**Creating New Endpoint**:

Edit `backend/ScadaCore/Controllers/CustomController.cs`:

```csharp
[ApiController]
[Route("api/[controller]")]
public class CustomController : ControllerBase
{
    [HttpGet("status")]
    public ActionResult<object> GetStatus()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
    
    [HttpPost("calculate")]
    public ActionResult<double> Calculate([FromBody] CalculationRequest request)
    {
        var result = request.Value1 + request.Value2;
        return Ok(result);
    }
}

public record CalculationRequest(double Value1, double Value2);
```

**Test API**:

```bash
curl http://localhost:5001/api/custom/status
curl -X POST http://localhost:5001/api/custom/calculate \
  -H "Content-Type: application/json" \
  -d '{"value1":10,"value2":20}'
```

**Exercise 16**: Create custom API endpoint

---

### Module 4.3: Frontend Development (3 hours)

**Creating Custom Component**:

Create `frontend/scada-dashboard/src/components/Custom/MyWidget.tsx`:

```typescript
import { useEffect, useState } from 'react';

export default function MyWidget({ tagName }: { tagName: string }) {
  const [value, setValue] = useState<number>(0);

  useEffect(() => {
    // Fetch tag value
    fetch(`http://localhost:5000/api/tags/${tagName}`)
      .then(res => res.json())
      .then(data => setValue(data.currentValue));
  }, [tagName]);

  return (
    <div className="card p-4">
      <h3 className="text-lg font-bold">{tagName}</h3>
      <p className="text-3xl">{value.toFixed(2)}</p>
    </div>
  );
}
```

**Use Component**:

```typescript
import MyWidget from './components/Custom/MyWidget';

function Dashboard() {
  return (
    <div>
      <MyWidget tagName="SITE01.TURBINE01.Power" />
    </div>
  );
}
```

**Exercise 17**: Create custom dashboard widget

---

### Module 4.4: Database Schema Changes (2 hours)

**Adding New Table** (Entity Framework):

```csharp
// Models/CustomData.cs
public class CustomData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Data/ApplicationDbContext.cs
public DbSet<CustomData> CustomData { get; set; }
```

**Create Migration**:

```bash
cd backend/ScadaCore
dotnet ef migrations add AddCustomData
dotnet ef database update
```

**Exercise 18**: Add custom table and query it

---

### Module 4.5: Integration Development (3 hours)

**Creating Protocol Driver** (Modbus example):

See `protocols/node-red/modbus-flow.json` for Node-RED integration.

**Custom API Integration**:

```csharp
public class ExternalApiService
{
    private readonly HttpClient _httpClient;
    
    public async Task<WeatherData> GetWeatherAsync(string location)
    {
        var response = await _httpClient.GetAsync($"https://api.weather.com/{location}");
        return await response.Content.ReadFromJsonAsync<WeatherData>();
    }
}
```

**Exercise 19**: Integrate with external API

---

## 5. Maintenance Training

**Duration**: 1 day  
**Level**: Intermediate  
**Goal**: Predictive maintenance using ML

### Module 5.1: ML Insights (2 hours)

**Viewing Predictions**:

1. Go to Analytics → Predictive Maintenance
2. Select equipment: TURBINE01
3. View:
   - Failure probability: 23%
   - Time to failure: 45 days
   - Recommendations: "Replace bearing"

**Understanding Anomalies**:

Anomaly Detector shows:
- 🔴 Detected anomalies
- Anomaly score (higher = more unusual)
- Explanation (which parameter is abnormal)

**Exercise 20**: Review ML predictions for your equipment

---

### Module 5.2: Maintenance Planning (2 hours)

**Scheduling Preventive Maintenance**:

1. Analytics → Maintenance Schedule
2. Add task:
   - Equipment: TURBINE01
   - Type: Bearing replacement
   - Scheduled: Based on ML (45 days)
   - Assigned to: Maintenance team

**Work Order Integration**:

System can send work orders to:
- Email
- Ticketing system (via API)
- Mobile app

**Exercise 21**: Schedule maintenance based on ML prediction

---

## 6. Troubleshooting Guide

### Quick Diagnostics

**System Not Loading**:
```powershell
# 1. Check Docker is running
docker ps

# 2. Check all services
docker-compose ps

# 3. Check logs
docker-compose logs --tail=50

# 4. Restart all
docker-compose restart
```

**No Data Showing**:
```powershell
# 1. Check data acquisition
curl http://localhost:5002/health

# 2. Check InfluxDB
docker logs scada-influxdb

# 3. Check RabbitMQ
docker logs scada-rabbitmq

# 4. Verify tag configuration
```

**Alarms Not Working**:
```powershell
# 1. Check alarm service
curl http://localhost:5003/health

# 2. Test email config
Check SMTP settings in .env

# 3. View alarm logs
docker logs scada-alarm-management
```

---

## 7. Best Practices

### For Administrators

1. **Daily**:
   - Check system health dashboard
   - Review overnight alarms
   - Monitor disk space

2. **Weekly**:
   - Review performance metrics
   - Check backup logs
   - Update documentation

3. **Monthly**:
   - Full system backup test
   - Review user access
   - Software updates

### For Operators

1. **Start of Shift**:
   - Review active alarms
   - Check equipment status
   - Read shift notes

2. **During Shift**:
   - Monitor dashboards
   - Acknowledge alarms promptly
   - Document incidents

3. **End of Shift**:
   - Hand over open issues
   - Write shift summary
   - Check next shift schedule

### For Developers

1. **Code Quality**:
   - Write unit tests
   - Document  API changes
   - Follow coding standards

2. **Deployment**:
   - Test in staging first
   - Use feature flags
   - Have rollback plan

3. **Monitoring**:
   - Add Prometheus metrics
   - Log important events
   - Set up alerts

---

## 8. Certification

### Knowledge Assessment

**Administrator Exam** (50 questions):
- System architecture (20%)
- User management (15%)
- Tag & alarm configuration (25%)
- Database management (20%)
- Troubleshooting (20%)

**Operator Exam** (30 questions):
- Dashboard navigation (20%)
- Tag monitoring (30%)
- Alarm management (30%)
- Report generation (20%)

**Developer Exam** (40 questions):
- API development (30%)
- Frontend development (25%)
- Database design (20%)
- Integration (25%)

### Passing Criteria

- Score ≥ 80% to pass
- Hands-on practical exam
- Certificate valid for 2 years

### Recertification

- Every 2 years
- Online refresher course
- Short exam (20 questions)

---

## 9. Additional Resources

### Documentation
- Architecture Guide: `ARCHITECTURE.md`
- API Reference: http://localhost:5001/swagger
- Administrator Guide: `docs/administrator-guide.md`

### Community
- GitHub Issues: Report bugs
- Discussion Forum: Ask questions
- Stack Overflow: Tag `scada-v2`

### Support
- Email: support@company.com
- Slack: #scada-support
- Emergency: +1-555-SCADA (24/7)

---

## 10. Training Schedule Template

### 2-Day Admin Training

**Day 1**:
- 09:00-10:00: System architecture overview
- 10:00-11:00: Starting/stopping system
- 11:00-12:00: User management
- 12:00-13:00: Lunch
- 13:00-16:00: Tag management (hands-on)
- 16:00-17:00: Alarm configuration

**Day 2**:
- 09:00-11:00: Database management
- 11:00-12:00: Monitoring & performance
- 12:00-13:00: Lunch
- 13:00-16:00: Troubleshooting lab
- 16:00-17:00: Backup & recovery
- 17:00-18:00: Certification exam

### 1-Day Operator Training

- 09:00-10:00: System overview
- 10:00-11:00: Dashboard navigation
- 11:00-13:00: Monitoring tags (hands-on)
- 13:00-14:00: Lunch
- 14:00-16:00: Alarm management
- 16:00-17:00: Report generation
- 17:00-18:00: Mobile access & exam

---

**Training Version**: 1.0  
**Last Updated**: 2025-01-01  
**Next Review**: 2025-07-01

**For Training Registration**: training@company.com  
**For Training Materials**: Access learning portal
