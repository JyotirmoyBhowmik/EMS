# Quick Reference - Adding Equipment & Managing Tags

**Quick command reference for daily operations**

---

## 🚀 **Quick Start Commands**

### Start/Stop System
```powershell
# Start all services
docker-compose up -d

# Stop all services
docker-compose down

# Restart specific service
docker-compose restart scada-core

# View logs
docker-compose logs -f scada-core
```

---

## 📊 **View Equipment & Tags**

### Check Loaded Tags
```powershell
# Count total tags
docker exec -it scada-postgres psql -U scada -d scada -c "SELECT COUNT(*) FROM tags;"

# Count by device type
docker exec -it scada-postgres psql -U scada -d scada -c "SELECT device_type, COUNT(*) FROM tags GROUP BY device_type;"

# List all Siemens tags
docker exec -it scada-postgres psql -U scada -d scada -c "SELECT name, description FROM tags WHERE device_type LIKE 'Siemens%';"
```

### Via API
```powershell
# Get all tags
curl http://localhost:5001/api/Tags

# Get tags by device type
curl "http://localhost:5001/api/Tags?deviceType=Siemens_S7-1500"

# Get specific tag
curl http://localhost:5001/api/Tags/SIEMENS.S7_1500.PLC01.CPU.Status
```

---

## ➕ **Add New Equipment (3 Methods)**

### Method 1: SQL Migration (Permanent)
```powershell
# 1. Create file: database/migrations/00X_my_equipment.sql
# 2. Add your INSERT statements (see examples below)
# 3. Run migration:
docker exec -i scada-postgres psql -U scada -d scada < database/migrations/00X_my_equipment.sql
```

###Method 2: Direct SQL
```powershell
# Quick one-time tag addition
docker exec -it scada-postgres psql -U scada -d scada

# Then run SQL:
INSERT INTO tags (id, name, description, data_type, unit, min_value, max_value, site_id, device_type, is_enabled, created_at) 
VALUES (gen_random_uuid(), 'MY.DEVICE.TAG01', 'My sensor', 'Float', '°C', 0, 100, 
        (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'MyDevice', true, NOW());
```

### Method 3: REST API
```powershell
# PowerShell script
$body = @{
    name = "MY.DEVICE.TAG01"
    description = "My sensor"
    dataType = "Float"
    unit = "°C"
    minValue = 0
    maxValue = 100
    deviceType = "MyDevice"
    isEnabled = $true
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5001/api/Tags" -Method Post -Body $body -ContentType "application/json"
```

---

## 📝 **Common Tag Examples - Copy & Paste**

### Siemens S7-1500 PLC Tag
```sql
INSERT INTO tags (id, name, description, data_type, unit, min_value, max_value, site_id, device_type, is_enabled, created_at) 
VALUES (
    gen_random_uuid(),
    'SIEMENS.S7_1500.PLC99.AI.Temperature',
    'Process temperature sensor',
    'Float',
    '°C',
    -50,
    500,
    (SELECT id FROM sites WHERE name = 'FACTORY_01'),
    'Siemens_S7-1500',
    true,
    NOW()
);
```

### Schneider Power Meter Tag
```sql
INSERT INTO tags VALUES (
    gen_random_uuid(),
    'SCHNEIDER.PM8000.METER99.Power.Active',
    'Active power consumption',
    'Float',
    'kW',
    0,
    10000,
    (SELECT id FROM sites WHERE name = 'UTILITY_01'),
    'Schneider_PM8000',
    true,
    NOW()
);
```

### Modbus RTU Device Tag
```sql
INSERT INTO tags VALUES (
    gen_random_uuid(),
    'MODBUS.RTU.DEVICE01.Holding_Register_001',
    'Tank level sensor',
    'Float',
    '%',
    0,
    100,
    (SELECT id FROM sites WHERE name = 'PROCESS_01'),
    'Modbus_RTU',
    true,
    NOW()
);
```

### Flow Meter Tag
```sql
INSERT INTO tags VALUES (
    gen_random_uuid(),
    'FLOWMETER.ENDRESS.FM10.FlowRate',
    'Water flow rate',
    'Float',
    'm³/h',
    0,
    500,
    (SELECT id FROM sites WHERE name = 'PROCESS_01'),
    'Endress_Promag',
    true,
    NOW()
);
```

---

## 🚨 **Add Alarm Rules**

### High Temperature Alarm
```sql
INSERT INTO alarm_rules (id, tag_id, name, condition, threshold_value, priority, message, is_enabled, created_at) 
VALUES (
    gen_random_uuid(),
    (SELECT id FROM tags WHERE name = 'SIEMENS.S7_1500.PLC99.AI.Temperature'),
    'High Temperature Alarm',
    'GreaterThan',
    450,
    'Critical',
    'Process temperature exceeded safe limit',
    true,
    NOW()
);
```

### Low Pressure Warning
```sql
INSERT INTO alarm_rules VALUES (
    gen_random_uuid(),
    (SELECT id FROM tags WHERE name = 'YOUR.TAG.NAME'),
    'Low Pressure Warning',
    'LessThan',
    2,
    'Medium',
    'System pressure below minimum',
    true,
    NOW()
);
```

---

## 🔍 **Troubleshooting**

### Check if Tag Exists
```sql
SELECT * FROM tags WHERE name = 'YOUR.TAG.NAME';
```

### Check Active Alarms
```powershell
curl http://localhost:5002/api/Alarms/active
```

### View Service Logs
```powershell
# Backend logs
docker logs scada-core

# Database logs
docker logs scada-postgres

# All services
docker-compose logs
```

### Restart Service After Changes
```powershell
docker-compose restart scada-core
```

---

## 📖 **Naming Convention Template**

**Standard Format**: `MANUFACTURER.MODEL.DEVICE.PARAMETER`

**Examples**:
- `SIEMENS.S7_1500.PLC01.AI.Temperature_01`
- `ABB.VFD.DRIVE05.Speed`
- `SCHNEIDER.PM8000.METER01.Power.Active`
- `HONEYWELL.DCS.NODE01.Pressure`

**Tips**:
- Use uppercase for manufacturer
- Be descriptive but concise
- Use underscores for multi-word parameters
- Include unit in description, not name

---

## 🎯 **Common Data Types**

| Type | Use For | Examples |
|------|---------|----------|
| `Boolean` | On/Off, Status | Running, Alarm, Open/Closed |
| `Integer` | Counters, Codes | Part count, Status code |
| `Float` | Measurements | Temperature, Pressure, Flow |
| `String` | Text | Messages, Batch IDs |

---

## 📚 **More Information**

- **Complete Guide**: `docs/ADDING_EQUIPMENT_GUIDE.md`
- **Equipment Reference**: `docs/INDUSTRIAL_EQUIPMENT_REFERENCE.md`
- **Example Tags**: `database/migrations/003_extended_equipment_tags.sql`
- **Windows Setup**: `WINDOWS_SETUP.md`
- **API Docs**: http://localhost:5001/swagger

---

**Last Updated**: 2025-01-08
