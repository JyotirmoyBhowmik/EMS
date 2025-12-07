# Adding New Equipment & Tags - Complete Guide

**Version**: 2.0  
**Last Updated**: 2025-01-08  
**Purpose**: Step-by-step guide to add new industrial equipment and tags to the SCADA system

---

## 📋 **Table of Contents**

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Method 1: Add Tags via SQL Migration](#method-1-sql-migration)
4. [Method 2: Add Tags via API](#method-2-api)
5. [Method 3: Bulk Import via CSV](#method-3-bulk-import)
6. [Adding Alarm Rules](#adding-alarm-rules)
7. [Configuring OPC UA Connections](#configuring-opc-ua)
8. [Testing & Verification](#testing-verification)
9. [Common Equipment Examples](#common-examples)

---

## 🎯 **Overview**

This guide covers three methods to add new equipment and tags:
- **SQL Migration**: For permanent equipment setup (recommended for new installations)
- **REST API**: For dynamic tag creation via code/scripts
- **CSV Bulk Import**: For importing large tag lists from Excel

---

## ✅ **Prerequisites**

Before adding equipment:
- [ ] System is running (`docker-compose up -d`)
- [ ] Database is accessible
- [ ] You have admin credentials
- [ ] Equipment specifications are documented

---

## 📝 **Method 1: Add Tags via SQL Migration**

**Best For**: Permanent equipment setup, version-controlled changes

### **Step 1: Create Migration File**

Create a new file: `database/migrations/00X_your_equipment.sql`

```sql
-- ================================================
-- MIGRATION: Add New Equipment - [Your Equipment Name]
-- Date: 2025-01-08
-- Description: Add tags for [Manufacturer] [Model]
-- ================================================

BEGIN;

-- ===== Add New Site (if needed) =====
INSERT INTO sites (id, name, location, description, timezone, is_active, created_at) VALUES
    (
        gen_random_uuid(),                          -- Auto-generate UUID
        'YOUR_SITE_CODE',                          -- Site code (e.g., 'PLANT_02')
        'Physical Location',                        -- Location description
        'Site description',                         -- Detailed description
        'UTC',                                      -- Timezone
        true,                                       -- Is active
        NOW()                                       -- Created timestamp
    )
ON CONFLICT (name) DO NOTHING;                     -- Prevent duplicates

-- ===== Add Equipment Tags =====

-- EXAMPLE 1: Adding a Siemens S7-300 PLC
INSERT INTO tags (id, name, description, data_type, unit, min_value, max_value, site_id, device_type, is_enabled, created_at) VALUES
    -- Digital Input: Emergency Stop Button
    (
        gen_random_uuid(),                                              -- Auto-generate ID
        'SIEMENS.S7_300.PLC05.DI.EmergencyStop',                       -- Tag name (hierarchical)
        'Emergency stop button on production line 2',                   -- Human-readable description
        'Boolean',                                                      -- Data type: Boolean, Integer, Float, String
        '',                                                             -- Unit (empty for boolean)
        0,                                                              -- Min value
        1,                                                              -- Max value
        (SELECT id FROM sites WHERE name = 'FACTORY_01'),              -- Link to site
        'Siemens_S7-300',                                              -- Device type for filtering
        true,                                                           -- Is tag enabled
        NOW()                                                           -- Created timestamp
    ),
    
    -- Analog Input: Tank Level Sensor
    (
        gen_random_uuid(),
        'SIEMENS.S7_300.PLC05.AI.Tank02_Level',                       -- Descriptive tag name
        'Chemical tank 2 level sensor (0-100%)',                       -- Clear description
        'Float',                                                        -- Float for analog values
        '%',                                                            -- Percentage unit
        0,                                                              -- Min: 0%
        100,                                                            -- Max: 100%
        (SELECT id FROM sites WHERE name = 'PROCESS_01'),
        'Siemens_S7-300',
        true,
        NOW()
    ),
    
    -- Analog Input: Pressure Transmitter
    (
        gen_random_uuid(),
        'SIEMENS.S7_300.PLC05.AI.Pressure_Line02',
        'Production line 2 pressure transmitter',
        'Float',
        'bar',                                                          -- Pressure unit
        0,
        16,                                                             -- 0-16 bar range
        (SELECT id FROM sites WHERE name = 'FACTORY_01'),
        'Siemens_S7-300',
        true,
        NOW()
    );

-- EXAMPLE 2: Adding ABB Variable Frequency Drive (VFD)
INSERT INTO tags (id, name, description, data_type, unit, min_value, max_value, site_id, device_type, is_enabled, created_at) VALUES
    (
        gen_random_uuid(),
        'ABB.VFD.DRIVE01.Status',
        'VFD running status',
        'Boolean',
        '',
        0,
        1,
        (SELECT id FROM sites WHERE name = 'FACTORY_01'),
        'ABB_ACS880',                                                   -- VFD model
        true,
        NOW()
    ),
    (
        gen_random_uuid(),
        'ABB.VFD.DRIVE01.Speed',
        'Motor speed setpoint',
        'Float',
        'Hz',                                                           -- Frequency in Hz
        0,
        60,
        (SELECT id FROM sites WHERE name = 'FACTORY_01'),
        'ABB_ACS880',
        true,
        NOW()
    ),
    (
        gen_random_uuid(),
        'ABB.VFD.DRIVE01.Current',
        'Motor current',
        'Float',
        'A',
        0,
        500,
        (SELECT id FROM sites WHERE name = 'FACTORY_01'),
        'ABB_ACS880',
        true,
        NOW()
    );

-- EXAMPLE 3: Adding Modbus RTU Device (Temperature Controller)
INSERT INTO tags (id, name, description, data_type, unit, min_value, max_value, site_id, device_type, is_enabled, created_at) VALUES
    (
        gen_random_uuid(),
        'EUROTHERM.TEMP_CTRL.TC01.ProcessValue',                      -- Current temperature
        'Oven temperature process value',
        'Float',
        '°C',
        0,
        1200,                                                           -- High-temp oven
        (SELECT id FROM sites WHERE name = 'FACTORY_01'),
        'Eurotherm_2408',
        true,
        NOW()
    ),
    (
        gen_random_uuid(),
        'EUROTHERM.TEMP_CTRL.TC01.Setpoint',                          -- Temperature setpoint
        'Oven temperature setpoint',
        'Float',
        '°C',
        0,
        1200,
        (SELECT id FROM sites WHERE name = 'FACTORY_01'),
        'Eurotherm_2408',
        true,
        NOW()
    ),
    (
        gen_random_uuid(),
        'EUROTHERM.TEMP_CTRL.TC01.Output',                            -- Controller output %
        'Heating element output percentage',
        'Float',
        '%',
        0,
        100,
        (SELECT id FROM sites WHERE name = 'FACTORY_01'),
        'Eurotherm_2408',
        true,
        NOW()
    );

COMMIT;

-- ===== VERIFICATION =====
-- Check that tags were created
SELECT name, description, device_type, unit 
FROM tags 
WHERE device_type IN ('Siemens_S7-300', 'ABB_ACS880', 'Eurotherm_2408')
ORDER BY name;

-- Count tags by device type
SELECT device_type, COUNT(*) as tag_count 
FROM tags 
GROUP BY device_type 
ORDER BY tag_count DESC;
```

### **Step 2: Apply Migration**

**Option A: Using Docker**
```powershell
# Copy file to container
docker cp database/migrations/00X_your_equipment.sql scada-postgres:/tmp/

# Execute migration
docker exec -it scada-postgres psql -U scada -d scada -f /tmp/00X_your_equipment.sql
```

**Option B: Direct Connection**
```powershell
# If PostgreSQL client is installed locally
psql -h localhost -U scada -d scada -f database/migrations/00X_your_equipment.sql
```

### **Step 3: Verify Tags**

```powershell
# Check tags via API
curl http://localhost:5001/api/Tags?deviceType=Siemens_S7-300

# Or use database query
docker exec -it scada-postgres psql -U scada -d scada -c "SELECT COUNT(*) FROM tags WHERE device_type='Siemens_S7-300';"
```

---

## 🌐 **Method 2: Add Tags via REST API**

**Best For**: Dynamic tag creation, integration with external systems

### **Step 1: Prepare Tag Data**

Create a PowerShell script: `add-tags.ps1`

```powershell
# ================================================
# Script: Add New Tags via SCADA REST API
# ================================================

# Configuration
$baseUrl = "http://localhost:5001"
$token = "your-jwt-token-here"  # Get from login

# Function to create a tag
function New-ScadaTag {
    param(
        [string]$Name,
        [string]$Description,
        [string]$DataType = "Float",
        [string]$Unit = "",
        [double]$MinValue = 0,
        [double]$MaxValue = 100,
        [string]$SiteId,
        [string]$DeviceType,
        [bool]$IsEnabled = $true
    )
    
    $body = @{
        name = $Name
        description = $Description
        dataType = $DataType
        unit = $Unit
        minValue = $MinValue
        maxValue = $MaxValue
        siteId = $SiteId
        deviceType = $DeviceType
        isEnabled = $IsEnabled
    } | ConvertTo-Json
    
    $headers = @{
        "Content-Type" = "application/json"
        "Authorization" = "Bearer $token"
    }
    
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/Tags" -Method Post -Body $body -Headers $headers
        Write-Host "✓ Created: $Name" -ForegroundColor Green
        return $response
    }
    catch {
        Write-Host "✗ Failed: $Name - $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

# Get Site ID first
$sites = Invoke-RestMethod -Uri "$baseUrl/api/Tags/sites" -Method Get
$factorySite = $sites | Where-Object { $_.name -eq "FACTORY_01" }

# Example 1: Add Allen-Bradley PLC Tags
Write-Host "`n=== Adding Allen-Bradley PLC Tags ===" -ForegroundColor Cyan

New-ScadaTag -Name "AB.CONTROL_LOGIX.PLC10.Status" `
             -Description "ControlLogix PLC status" `
             -DataType "Boolean" `
             -Unit "" `
             -MinValue 0 `
             -MaxValue 1 `
             -SiteId $factorySite.id `
             -DeviceType "AllenBradley_ControlLogix"

New-ScadaTag -Name "AB.CONTROL_LOGIX.PLC10.Tank_Level" `
             -Description "Main process tank level" `
             -DataType "Float" `
             -Unit "%" `
             -MinValue 0 `
             -MaxValue 100 `
             -SiteId $factorySite.id `
             -DeviceType "AllenBradley_ControlLogix"

# Example 2: Add Flow Meter Tags
Write-Host "`n=== Adding Flow Meter Tags ===" -ForegroundColor Cyan

New-ScadaTag -Name "ENDRESS.FLOWMETER.FM10.Flow" `
             -Description "Cooling water flow rate" `
             -DataType "Float" `
             -Unit "m³/h" `
             -MinValue 0 `
             -MaxValue 200 `
             -SiteId $factorySite.id `
             -DeviceType "Endress_Promag"

Write-Host "`n=== Tag Creation Complete ===" -ForegroundColor Green
```

### **Step 2: Run Script**

```powershell
# Execute the script
.\add-tags.ps1
```

---

## 📊 **Method 3: Bulk Import via CSV**

**Best For**: Importing large tag databases from spreadsheets

### **Step 1: Prepare CSV File**

Create `tags-import.csv`:

```csv
Name,Description,DataType,Unit,MinValue,MaxValue,SiteId,DeviceType,IsEnabled
"YOKOGAWA.DCS.NODE01.AI.Temp01","Reactor temperature 1","Float","°C",0,500,"<SITE_ID>","Yokogawa_Centum",true
"YOKOGAWA.DCS.NODE01.AI.Temp02","Reactor temperature 2","Float","°C",0,500,"<SITE_ID>","Yokogawa_Centum",true
"YOKOGAWA.DCS.NODE01.AI.Pressure","Reactor pressure","Float","bar",0,25,"<SITE_ID>","Yokogawa_Centum",true
"YOKOGAWA.DCS.NODE01.DI.Alarm","High pressure alarm","Boolean","",0,1,"<SITE_ID>","Yokogawa_Centum",true
```

### **Step 2: Import Script**

Create `import-tags-csv.ps1`:

```powershell
# ================================================
# Bulk Import Tags from CSV
# ================================================

$csvPath = ".\tags-import.csv"
$baseUrl = "http://localhost:5001"

# Import CSV
$tags = Import-Csv -Path $csvPath

Write-Host "Importing $($tags.Count) tags..." -ForegroundColor Cyan

foreach ($tag in $tags) {
    $body = @{
        name = $tag.Name
        description = $tag.Description
        dataType = $tag.DataType
        unit = $tag.Unit
        minValue = [double]$tag.MinValue
        maxValue = [double]$tag.MaxValue
        siteId = $tag.SiteId
        deviceType = $tag.DeviceType
        isEnabled = [bool]::Parse($tag.IsEnabled)
    } | ConvertTo-Json
    
    try {
        Invoke-RestMethod -Uri "$baseUrl/api/Tags/import" -Method Post -Body "[$body]" -ContentType "application/json"
        Write-Host "✓ $($tag.Name)" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ $($tag.Name): $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`nImport Complete!" -ForegroundColor Green
```

---

## 🚨 **Adding Alarm Rules**

### **Create Alarm Rule for New Equipment**

```sql
-- Example: Add alarm rule for high temperature
INSERT INTO alarm_rules (id, tag_id, name, condition, threshold_value, priority, message, is_enabled, created_at) VALUES
    (
        gen_random_uuid(),
        (SELECT id FROM tags WHERE name = 'EUROTHERM.TEMP_CTRL.TC01.ProcessValue'),
        'Oven High Temperature Alarm',                  -- Rule name
        'GreaterThan',                                   -- Condition type
        1100,                                            -- Threshold value
        'Critical',                                      -- Priority: Low, Medium, High, Critical
        'Oven temperature exceeded 1100°C safety limit',-- Alarm message
        true,                                            -- Enable alarm
        NOW()
    );

-- Example: Low level alarm
INSERT INTO alarm_rules (id, tag_id, name, condition, threshold_value, priority, message, is_enabled, created_at) VALUES
    (
        gen_random_uuid(),
        (SELECT id FROM tags WHERE name = 'SIEMENS.S7_300.PLC05.AI.Tank02_Level'),
        'Tank Low Level Warning',
        'LessThan',
        20,                                              -- 20% level
        'Medium',
        'Chemical tank 2 level below 20%',
        true,
        NOW()
    );
```

---

## 🔌 **Configuring OPC UA Connections**

### **Add OPC UA Server Configuration**

Edit `backend/OpcUaServer/Services/OpcUaServerService.cs`:

```csharp
// Add your equipment nodes
private void InitializeNodes()
{
    // ... existing code ...

    // Add new Siemens S7-300 nodes
    var s7300Folder = _server.NodeManager.CreateFolder("S7-300 PLC05");
    server.NodeManager.CreateVariable(s7300Folder, "EmergencyStop", false);
    _server.NodeManager.CreateVariable(s7300Folder, "Tank02_Level", 50.0);
    _server.NodeManager.CreateVariable(s7300Folder, "Pressure_Line02", 5.2);
    
    // Add ABB VFD nodes
    var vfdFolder = _server.NodeManager.CreateFolder("ABB VFD Drive01");
    _server.NodeManager.CreateVariable(vfdFolder, "Status", true);
    _server.NodeManager.CreateVariable(vfdFolder, "Speed", 45.0);
    _server.NodeManager.CreateVariable(vfdFolder, "Current", 120.5);
}
```

---

## ✅ **Testing & Verification**

### **1. Verify Tags Created**

```powershell
# Get all tags for your device
curl "http://localhost:5001/api/Tags?deviceType=Siemens_S7-300"

# Get specific tag
curl "http://localhost:5001/api/Tags/SIEMENS.S7_300.PLC05.AI.Tank02_Level"
```

### **2. Test Tag Values**

```powershell
# Update tag value
$body = @{ value = 75.5 } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5001/api/Tags/SIEMENS.S7_300.PLC05.AI.Tank02_Level/value" -Method Post -Body $body -ContentType "application/json"

# Get current value
curl "http://localhost:5001/api/Tags/SIEMENS.S7_300.PLC05.AI.Tank02_Level/value"
```

### **3. Check Alarm Triggering**

```powershell
# Set value above threshold
$body = @{ value = 1150 } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5001/api/Tags/EUROTHERM.TEMP_CTRL.TC01.ProcessValue/value" -Method Post -Body $body -ContentType "application/json"

# Check active alarms
curl "http://localhost:5002/api/Alarms/active"
```

---

## 📖 **Common Equipment Examples**

### **Example 1: Rockwell Automation PLC**

```sql
INSERT INTO tags (id, name, description, data_type, unit, min_value, max_value, site_id, device_type, is_enabled, created_at) VALUES
    (gen_random_uuid(), 'ROCKWELL.MICROLOGIX.PLC20.Status', 'PLC online status', 'Boolean', '', 0, 1, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Rockwell_MicroLogix', true, NOW()),
    (gen_random_uuid(), 'ROCKWELL.MICROLOGIX.PLC20.PartCount', 'Production count', 'Integer', 'pcs', 0, 999999, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Rockwell_MicroLogix', true, NOW());
```

### **Example 2: Yokogawa DCS**

```sql
INSERT INTO tags (id, name, description, data_type, unit, min_value, max_value, site_id, device_type, is_enabled, created_at) VALUES
    (gen_random_uuid(), 'YOKOGAWA.CENTUM.FCS01.Temp', 'Process temperature', 'Float', '°C', 0, 800, (SELECT id FROM sites WHERE name = 'PROCESS_01'), 'Yokogawa_Centum', true, NOW()),
    (gen_random_uuid(), 'YOKOGAWA.CENTUM.FCS01.Flow', 'Steam flow rate', 'Float', 'kg/h', 0, 50000, (SELECT id FROM sites WHERE name = 'PROCESS_01'), 'Yokogawa_Centum', true, NOW());
```

### **Example 3: Honeywell RTU**

```sql
INSERT INTO tags (id, name, description, data_type, unit, min_value, max_value, site_id, device_type, is_enabled, created_at) VALUES
    (gen_random_uuid(), 'HONEYWELL.RTU.REMOTE01.Tank_Level', 'Remote tank level', 'Float', '%', 0, 100, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'Honeywell_RTU', true, NOW());
```

---

## 🎯 **Best Practices**

### **Naming Conventions**
✅ **DO**: `MANUFACTURER.MODEL.DEVICE.PARAMETER`  
❌ **DON'T**: `tag1`, `temp`, `sensor_a`

### **Tag Organization**
✅ Group by equipment hierarchy  
✅ Use consistent units (SI preferred)  
✅ Set realistic min/max values  
✅ Add detailed descriptions

### **Data Types**
- **Boolean**: On/Off, Running/Stopped, Open/Closed
- **Integer**: Counters, part counts, status codes
- **Float**: Analog sensors, measurements, percentages
- **String**: Messages, comments, batch IDs

---

## 🆘 **Troubleshooting**

### **Tags Not Appearing**
```sql
-- Check if tag was created
SELECT * FROM tags WHERE name LIKE '%YOUR_TAG%';

-- Check if enabled
SELECT name, is_enabled FROM tags WHERE is_enabled = false;
```

### **API Errors**
```powershell
# Check SCADA Core service logs
docker logs scada-core

# Check database connectivity
docker exec -it scada-postgres psql -U scada -d scada -c "SELECT COUNT(*) FROM tags;"
```

---

## 📚 **Additional Resources**

- **Equipment Reference**: `docs/INDUSTRIAL_EQUIPMENT_REFERENCE.md`
- **API Documentation**: http://localhost:5001/swagger
- **Database Schema**: `database/migrations/001_initial_schema.sql`

---

**Updated**: 2025-01-08  
**Version**: 2.0  
**Questions?** Check the comprehensive tag examples in `003_extended_equipment_tags.sql`
