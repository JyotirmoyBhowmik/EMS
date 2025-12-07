# Enterprise SCADA System v2.1 - User Manual

**Version**: 2.1  
**Date**: 2025-01-08  
**For**: Operators, Engineers, and Administrators

---

## 📚 **Table of Contents**

1. [Introduction](#1-introduction)
2. [Getting Started](#2-getting-started)
3. [Dashboard Overview](#3-dashboard-overview)
4. [Energy Management](#4-energy-management) ⭐ **NEW**
5. [Work Order Management](#5-work-order-management) ⭐ **NEW**
6. [Alarm Management](#6-alarm-management)
7. [Reports](#7-reports)
8. [User Administration](#8-user-administration)
9. [Troubleshooting](#9-troubleshooting)

---

## 1. Introduction

### What is the SCADA System?

The Enterprise SCADA System is a comprehensive platform for:
- **Real-time Monitoring** - Track equipment status, sensors, and production metrics
- **Energy Management** - Monitor consumption, track carbon footprint, manage 92+ meters
- **Alarm Management** - Get instant notifications when issues occur
- **Work Order Management** - Create and track maintenance tasks
- **Predictive Analytics** - Use AI to predict equipment failures
- **Automated Reporting** - Generate and email reports automatically

### Who Should Use This Manual?

- **Operators** - Daily monitoring and basic tasks
- **Engineers** - Configuration, troubleshooting, analysis
- **Maintenance** - Work order management, equipment tracking
- **Administrators** - User management, system configuration

---

## 2. Getting Started

### 2.1 Accessing the System

1. Open your web browser (Chrome, Edge, Firefox)
2. Navigate to: `http://localhost:3000` (or your server IP)
3. You'll see the login screen

### 2.2 Logging In

**Default Credentials**:
- **Administrator**: `admin@scada.local` / `Admin123!`
- **Engineer**: `engineer@scada.local` / `Engineer123!`
- **Operator**: `operator@scada.local` / `Operator123!`

⚠️ **IMPORTANT**: Change these passwords immediately after first login!

### 2.3 First Login Steps

1. Enter your credentials
2. Click "Sign In"
3. You'll be prompted to change your password
4. Set a strong password (min 8 characters, uppercase, lowercase, number, symbol)
5. Enable Two-Factor Authentication (2FA) for added security

---

## 3. Dashboard Overview

### 3.1 Main Dashboard

The main dashboard shows:
- **Overview Cards** - Quick statistics (total tags, active alarms, energy consumption)
- **Live Charts** - Real-time data visualization
- **Alarm Panel** - Recent and active alarms
- **Equipment Status** - Status of monitored equipment

### 3.2 Navigation Menu

Located on the left side:
- 🏠 **Dashboard** - Main overview
- ⚡ **Energy Management** - Energy meters and consumption ⭐ **NEW**
- 📋 **Work Orders** - Maintenance tasks ⭐ **NEW**
- 🚨 **Alarms** - Alert management
- 📊 **Reports** - Generate and view reports
- 🏭 **Sites** - Manage facilities
- 📌 **Tags** - Configure data points
- 👥 **Users** - User management (admin only)

---

## 4. Energy Management ⭐ **NEW in v2.1**

### 4.1 Overview

The Energy Management module helps you:
- Monitor real-time energy consumption
- Track 92+ hierarchical meters
- Calculate carbon footprint
- Identify power losses
- Track diesel consumption (DG sets)
- Monitor solar energy generation

### 4.2 Meter Setup

#### Accessing Meter Setup
1. Click "Energy Management" in sidebar
2. Select "Meter Setup"

#### Adding a New Meter
1. Click "+ Add Meter" button
2. Fill in meter details:
   - **Meter Number**: e.g., "METER-PMD-001"
   - **Meter Name**: e.g., "Power Distribution Main"
   - **Type**: Main, Submeter, DG, Solar, Unit, Machine
   - **Parent Meter**: Select from dropdown (for hierarchy)
   - **Location**: Building, floor information

3. Configure CT/PT values:
   - **CT Primary**: e.g., 1000 A
   - **CT Secondary**: Typically 5 A
   - **PT Primary**: e.g., 11000 V
   - **PT Secondary**: Typically 110 V
   - Ratios are auto-calculated

4. Set communication details:
   - **IP Address**: e.g., 192.168.1.100
   - **Modbus Address**: e.g., 1
   - **Protocol**: Modbus TCP/RTU

5. Click "Save Meter"

#### Understanding Meter Hierarchy

```
Grid (Level 0)
 └─ Power Station (Level 1)
     ├─ DG Set (Level 2)
     ├─ Solar Array (Level 2)
     └─ Main Distribution (Level 2)
         ├─ Unit A (Level 3)
         │   ├─ Machine 1 (Level 4)
         │   └─ Machine 2 (Level 4)
         └─ Unit B (Level 3)
             ├─ Machine 3 (Level 4)
             └─ Machine 4 (Level 4)
```

#### Meter Status Indicators

- 🟢 **Green** - Active and healthy
- 🔴 **Red** - Inactive or offline
- 🟡 **Yellow** - Maintenance mode or warning
- 🟠 **Orange** - Faulty or critical issue

### 4.3 Energy Dashboard

#### Accessing Dashboard
1. Click "Energy Management"
2. Select "Dashboard"

#### Dashboard Features

**1. Power Flow Diagram**
- Sankey diagram showing energy distribution
- Visual representation of parent-child power flow
- Interactive - click nodes for details

**2. Status Distribution**
- Pie chart of meter statuses (Active/Inactive/Maintenance)
- Quick health overview

**3. Consumption Pattern**
- Line chart showing 7-day consumption trend
- Identify peak usage times
- Spot anomalies

**4. Meter Distribution Map**
- Hierarchical tree view of all meters
- Color-coded status
- Expandable/collapsible nodes

### 4.4 Energy Monitoring

#### Real-Time Consumption
- View current power draw (kW)
- Total energy consumed (kWh)
- Power factor
- Voltage and current readings

#### Carbon Footprint
- Total carbon emissions (kg CO2)
- Daily/weekly/monthly trends
- Solar offset calculations
- Export reports

#### Power Loss Analysis
- Compare parent meter vs sum of children
- Identify transmission/distribution losses
- Set acceptable loss thresholds
- Get alerts for excessive losses

### 4.5 Diesel Generator (DG) Monitoring

For DG meters:
- Track diesel consumption (liters)
- Calculate fuel efficiency (kWh per liter)
- Monitor runtime hours
- Calculate carbon emissions from diesel

### 4.6 Solar Energy Tracking

For solar meters:
- Generation capacity (kW)
- Total energy generated (kWh)
- Performance ratio
- Carbon offset achieved
- Export to grid tracking

---

## 5. Work Order Management ⭐ **NEW in v2.1**

### 5.1 Overview

Work Order Management helps you:
- Create maintenance tasks
- Assign work to technicians
- Track time and materials
- Auto-create work orders from alarms
- Capture completion signatures

### 5.2 Creating a Work Order

1. Click "Work Orders" in sidebar
2. Click "+ New Work Order"
3. Fill in details:
   - **Title**: e.g., "Pump P-101 Maintenance"
   - **Description**: Detailed procedure
   - **Priority**: Low / Medium / High / Critical
   - **Type**: Corrective / Preventive / Predictive / Inspection
   - **Assign To**: Select technician
   - **Scheduled Start/End**: Date and time

4. Add Tasks (optional):
   - Click "Add Task"
   - Enter task description
   - Set sequence number

5. Add Materials (optional):
   - Click "Add Material"
   - Material name, part number
   - Quantity and unit cost

6. Click "Create Work Order"

### 5.3 Work Order Lifecycle

```
New → Assigned → In Progress → Completed
                      ↓
                   On Hold
```

#### Changing Status

1. Open work order
2. Click status dropdown
3. Select new status
4. Add notes (optional)
5. Save

### 5.4 Assigning Work Orders

1. Open work order
2. Click "Assign"
3. Select technician from dropdown
4. Click "Assign"
5. Technician receives notification

### 5.5 Completing a Work Order

1. Open work order
2. Click "Complete"
3. Fill in completion form:
   - **Actual Hours**: Time spent
   - **Actual Cost**: Total cost
   - **Completion Notes**: What was done
   - **Signature**: Capture digital signature
4. Click "Submit"

### 5.6 Auto-Create from Alarms

When a critical alarm occurs:
1. System detects high-priority alarm
2. Automatically creates work order
3. Links alarm ID to work order
4. Assigns to default maintenance team
5. Sends notification

**To configure:**
- Go to Alarm Settings
- Enable "Auto-create Work Order"
- Set priority threshold (e.g., High, Critical)
- Set default assignee

### 5.7 Work Order Statistics

View dashboard showing:
- Total work orders
- Open vs closed
- Overdue count
- Average completion time
- Cost tracking

---

## 6. Alarm Management

### 6.1 Viewing Alarms

1. Click "Alarms" in sidebar
2. See list of all alarms
3. Filter by:
   - Status (Active/Acknowledged/Cleared)
   - Priority (Low/Medium/High/Critical)
   - Date range
   - Equipment type

### 6.2 Acknowledging Alarms

1. Select alarm from list
2. Click "Acknowledge"
3. Add acknowledgment note
4. Click "Submit"

### 6.3 Alarm Notifications

Alarms can trigger:
- Email notifications
- SMS alerts (if configured)
- Work order creation
- Dashboard pop-ups

---

## 7. Reports

### 7.1 Available Reports

- **Production Report** - Daily/weekly production summary
- **Energy Report** - Consumption, carbon footprint
- **Alarm Summary** - Alarm counts and response times
- **Work Order Report** - Maintenance statistics
- **Equipment Report** - Status and utilization

### 7.2 Generating a Report

1. Click "Reports" in sidebar
2. Select report type
3. Choose parameters:
   - Date range
   - Site/equipment filter
   - Output format (PDF/Excel)
4. Click "Generate"

### 7.3 Scheduled Reports ⭐ **NEW**

#### Setting Up Scheduled Reports

1. Go to Reports → Scheduled Reports
2. Click "+ New Schedule"
3. Configure:
   - **Report Type**: Daily Production, Weekly Energy, etc.
   - **Schedule**: Daily at 8:00 AM, Weekly Monday 9:00 AM
   - **Recipients**: Email addresses (comma-separated)
   - **Format**: PDF, Excel, or both
4. Click "Create Schedule"

Reports will auto-generate and email!

---

## 8. User Administration

*Available to Administrators only*

### 8.1 Adding Users

1. Click "Users" → "+ Add User"
2. Enter details:
   - Email
   - Name
   - Role (Admin/Engineer/Operator)
3. User receives invitation email
4. They set their own password

### 8.2 Managing Permissions

Each role has different permissions:
- **Administrator** - Full access
- **Engineer** - Read/write data, configure system
- **Operator** - Monitor, acknowledge alarms

---

## 9. Troubleshooting

### Common Issues

**Problem**: Meter showing offline  
**Solution**: Check communication settings, verify IP address, check Modbus configuration

**Problem**: Work order not creating from alarm  
**Solution**: Verify auto-create is enabled in alarm settings, check alarm priority threshold

**Problem**: Report not emailing  
**Solution**: Check email server settings in configuration, verify SMTP credentials

**Problem**: Can't see Energy Management menu  
**Solution**: Check user role permissions, ensure migrations were run

---

## Quick Reference

### Keyboard Shortcuts
- `Ctrl + D` - Go to Dashboard
- `Ctrl + A` - View Alarms
- `Ctrl + E` - Energy Management
- `Ctrl + W` - Work Orders

### Support
- **Technical Support**: support@scada.local
- **Documentation**: `/docs` folder
- **Training Videos**: Coming soon

---

**End of User Manual v2.1**
